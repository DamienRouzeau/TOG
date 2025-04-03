using UnityEngine;
using UnityEngine.Rendering;

[ImageEffectAllowedInSceneView]
[ExecuteInEditMode()]
public class NGSS_FrustumShadows : MonoBehaviour
{
    [Header("REFERENCES")]
    public Light mainShadowsLight;
    public Shader frustumShadowsShader;
    [SerializeField] private bool m_debug = false;
    [Header("SHADOWS SETTINGS")]
    [Tooltip("Poisson Noise. Randomize samples to remove repeated patterns.")]
    public bool m_dithering = false;

    [Tooltip("If enabled a faster separable blur will be used.\nIf disabled a slower depth aware blur will be used.")]
    public bool m_fastBlur = true;

    [Tooltip("If enabled, backfaced lit fragments will be skipped increasing performance. Requires GBuffer normals.")]
    public bool m_deferredBackfaceOptimization = false;

    [Range(0f, 1f), Tooltip("Set how backfaced lit fragments are shaded. Requires DeferredBackfaceOptimization to be enabled.")]
    public float m_deferredBackfaceTranslucency = 0f;

    [Tooltip("Tweak this value to remove soft-shadows leaking around edges.")]
    [Range(0.01f, 1f)]
    public float m_shadowsEdgeBlur = 0.25f;

    [Tooltip("Overall softness of the shadows.")]
    [Range(0.01f, 1.0f)]
    public float m_shadowsBlur = 0.5f;

    //[Tooltip("The distance where shadows start to fade.")]
    //[Range(0.1f, 4.0f)]
    //public float m_shadowsFade = 1f;

    [Tooltip("Tweak this value if your objects display backface shadows.")]
    [Range(0.0f, 1f)]
    public float m_shadowsBias = 0.05f;

#if !UNITY_5
    [Tooltip("The distance in metters from camera where shadows start to shown.")]
    //[Min(0f)]
    public float m_shadowsDistanceStart = 0f;
#else
    [Tooltip("The distance in metters from camera where shadows start to shown.")]
    public float m_shadowsDistanceStart = 0f;
#endif
    [Header("RAY SETTINGS")]
    [Tooltip("If enabled the ray length will be scaled at screen space instead of world space. Keep it enabled for an infinite view shadows coverage. Disable it for a ContactShadows like effect. Adjust the Ray Scale property accordingly.")]
    public bool m_rayScreenScale = true;

    [Tooltip("Number of samplers between each step. The higher values produces less gaps between shadows but is more costly.")]
    [Range(16, 128)]
    public int m_raySamples = 64;

    [Tooltip("The higher the value, the larger the shadows ray will be.")]
    [Range(0.01f, 1f)]
    public float m_rayScale = 0.25f;

    [Tooltip("The higher the value, the ticker the shadows will look.")]
    [Range(0.0f, 1.0f)]
    public float m_rayThickness = 0.01f;

    [Header("TEMPORAL SETTINGS (EXPERIMENTAL)")]
    [Tooltip("Temporal filtering. Improves the shadows aliasing by adding an extra temporal pass. Currently experimental, does not work when the Scene View is open, only in Game View.")]
    public bool m_Temporal = false;
    private bool isTemporal = false;
    [Range(0f, 1f)]
    [Tooltip("Temporal scale in seconds. The bigger the smoother the shadows but produces trail/blur within shadows.")]
    public float m_Scale = 0.75f;
    [Tooltip("Improves the temporal filter by shaking the screen space shadows at different frames.")]
    [Range(0f, 0.25f)]
    public float m_Jittering = 0f;

    /*******************************************************************************************************************/

    //private Texture2D noMixTexture;    
    int mainTexID = Shader.PropertyToID("_MainTex");
    int debugSource = Shader.PropertyToID("Debug RT");
    int cShadow = Shader.PropertyToID("NGSS_ContactShadowRT");
    int cShadow2 = Shader.PropertyToID("NGSS_ContactShadowRT2");
    int dSource = Shader.PropertyToID("NGSS_DepthSourceRT");
    
    private int m_SampleIndex = 0;
    private RenderingPath currentRenderingPath;
    private CommandBuffer computeShadowsCB, debugCB;
    private bool isInitialized = false;

    bool IsNotSupported()
    {
#if UNITY_2018_1_OR_NEWER
        return (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2);
#elif UNITY_2017_4_OR_EARLIER
        return (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStationVita || SystemInfo.graphicsDeviceType == GraphicsDeviceType.N3DS);
#else
        return (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D9 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStationMobile || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStationVita || SystemInfo.graphicsDeviceType == GraphicsDeviceType.N3DS);
#endif
    }

    private RenderTexture mTempRT;
    private RenderTexture TempRT
    {
        get
        {
            if (mTempRT == null || (mTempRT.width != mCamera.pixelWidth || mTempRT.height != mCamera.pixelHeight))
            {
                if (mTempRT) { RenderTexture.ReleaseTemporary(mTempRT); }

                mTempRT = RenderTexture.GetTemporary(mCamera.pixelWidth, mCamera.pixelHeight, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
                mTempRT.hideFlags = HideFlags.HideAndDontSave;
            }
            return mTempRT;
        }
        set { mTempRT = value; }
    }

    private Camera _mCamera;
    private Camera mCamera
    {
        get
        {
            if (_mCamera == null)
            {
                _mCamera = GetComponent<Camera>();
                if (_mCamera == null) { _mCamera = Camera.main; }
                if (_mCamera == null) { Debug.LogError("NGSS Error: No MainCamera found, please provide one.", this); enabled = false; }
                //#if UNITY_EDITOR
                //if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera != null)
                //UnityEditor.SceneView.currentDrawingSceneView.camera.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
                //#endif

                //MonoBehaviour ngss_cs = _mCamera.GetComponent("NGSS_ContactShadows") as MonoBehaviour;
                //if (ngss_cs != null) { ngss_cs.enabled = false; DestroyImmediate(ngss_cs); }
            }
            return _mCamera;
        }
    }

    private Material _mMaterial;
    private Material mMaterial
    {
        get
        {
            if (_mMaterial == null)
            {
                //_mMaterial = new Material(Shader.Find("Hidden/NGSS_FrustumShadows"));//Automatic (sometimes it bugs)
                if (frustumShadowsShader == null) { frustumShadowsShader = Shader.Find("Hidden/NGSS_FrustumShadows"); }
                _mMaterial = new Material(frustumShadowsShader);//Manual
                if (_mMaterial == null) { Debug.LogWarning("NGSS Warning: can't find NGSS_FrustumShadows shader, make sure it's on your project.", this); enabled = false; }
            }
            return _mMaterial;
        }
    }

    void AddCommandBuffers()
    {
        currentRenderingPath = mCamera.renderingPath;
        
        if (computeShadowsCB == null) { computeShadowsCB = new CommandBuffer { name = "NGSS FrustumShadows: Compute" }; } else { computeShadowsCB.Clear(); }

        bool canAddBuff = true;
        if(mCamera.renderingPath == RenderingPath.DeferredShading)
        {
            foreach (CommandBuffer cb in mCamera.GetCommandBuffers(m_Temporal ? CameraEvent.AfterGBuffer : CameraEvent.BeforeLighting)) { if (cb.name == computeShadowsCB.name) { canAddBuff = false; break; } }
            if (canAddBuff) { mCamera.AddCommandBuffer(m_Temporal ? CameraEvent.AfterGBuffer : CameraEvent.BeforeLighting, computeShadowsCB); }
        }
        else
        {
            foreach (CommandBuffer cb in mCamera.GetCommandBuffers(CameraEvent.AfterDepthTexture)) { if (cb.name == computeShadowsCB.name) { canAddBuff = false; break; } }
            if (canAddBuff) { mCamera.AddCommandBuffer(m_Temporal ? CameraEvent.AfterForwardOpaque : CameraEvent.AfterDepthTexture, computeShadowsCB); }
        }
        /*
#if UNITY_EDITOR
        if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera != null)
        {
            UnityEditor.SceneView.currentDrawingSceneView.camera.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
            UnityEditor.SceneView.currentDrawingSceneView.camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, computeShadowsCB);
        }
#endif*/
        if (m_debug)
        {
            //render the mesh to a temporary RT.
            if (debugCB == null)
            {
                debugCB = new CommandBuffer();
                debugCB.name = "Draw to Temporary RT";
            }

            debugCB.Clear();
            //debugCB.GetTemporaryRT(debugSource, UnityEngine.XR.XRSettings.eyeTextureDesc);
            debugCB.GetTemporaryRT(debugSource, UnityEngine.XR.XRSettings.eyeTextureWidth, UnityEngine.XR.XRSettings.eyeTextureHeight, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
            CustomBlit(debugCB, BuiltinRenderTextureType.CurrentActive, debugSource, mMaterial, 0);
            //debugCB.ReleaseTemporaryRT(debugSource);
            mCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, debugCB);
        }
    }

    void RemoveCommandBuffers()
    {
        _mMaterial = null;
        if(mCamera.renderingPath == RenderingPath.DeferredShading)
            if (mCamera) { mCamera.RemoveCommandBuffer(isTemporal ? CameraEvent.AfterGBuffer : CameraEvent.BeforeLighting, computeShadowsCB); }
        else
            if (mCamera) { mCamera.RemoveCommandBuffer(m_Temporal ? CameraEvent.AfterForwardOpaque : CameraEvent.AfterDepthTexture, computeShadowsCB); }
        
        //We dont need this anymore as the contact shadows mix is done directly on shadow internal files
        /*
#if UNITY_EDITOR
        if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera != null)
            UnityEditor.SceneView.currentDrawingSceneView.camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, computeShadowsCB);
#endif*/

        isInitialized = false;
        
        if (m_debug && debugCB != null && mCamera) mCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, debugCB);
    }

    void CustomBlit(CommandBuffer cmd, RenderTargetIdentifier src, RenderTargetIdentifier dest, Material mat, int pass)
    {
        cmd.SetRenderTarget(dest, 0, CubemapFace.Unknown, -1);
        cmd.ClearRenderTarget(true, true, Color.clear);
        //cmd.SetGlobalTexture(mainTexID, src);
        cmd.DrawMesh(fullScreenTriangle, Matrix4x4.identity, mat, pass);
    }

    void Init()
    {
        if (isInitialized || mainShadowsLight == null) { return; }

        //comment me these 3 lines if sampling directly from internal files
        //noMixTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        //noMixTexture.SetPixel(0, 0, Color.white);
        //noMixTexture.Apply();

        if (mCamera.renderingPath == RenderingPath.VertexLit)
        {
            Debug.LogWarning("Vertex Lit Rendering Path is not supported by NGSS Contact Shadows. Please set the Rendering Path in your game camera or Graphics Settings to something else than Vertex Lit.", this);
            enabled = false;
            //DestroyImmediate(this);
            return;
        }

        CreateFullscreenQuad(mCamera);
        AddCommandBuffers();

        //computeShadowsCB.GetTemporaryRT(cShadow, UnityEngine.XR.XRSettings.eyeTextureDesc, FilterMode.Bilinear);
        computeShadowsCB.GetTemporaryRT(cShadow, UnityEngine.XR.XRSettings.eyeTextureWidth, UnityEngine.XR.XRSettings.eyeTextureHeight, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
        computeShadowsCB.GetTemporaryRT(cShadow2, UnityEngine.XR.XRSettings.eyeTextureWidth, UnityEngine.XR.XRSettings.eyeTextureHeight, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
        computeShadowsCB.GetTemporaryRT(dSource, UnityEngine.XR.XRSettings.eyeTextureWidth, UnityEngine.XR.XRSettings.eyeTextureHeight, 0, FilterMode.Bilinear, RenderTextureFormat.RFloat);
        //computeShadowsCB.SetGlobalTexture(Shader.PropertyToID("ScreenSpaceMask"), BuiltinRenderTextureType.CurrentActive);//requires a commandBuffer on the light, not compatible with local light
        
        // CustomBlit(computeShadowsCB, cShadow, dSource, mMaterial, 0);
        // CustomBlit(computeShadowsCB, dSource, cShadow, mMaterial, 1);
        computeShadowsCB.Blit(cShadow, dSource, mMaterial, 0);//clip edges
        computeShadowsCB.Blit(dSource, cShadow, mMaterial, 1);//compute ssrt shadows

        //blur shadows
        computeShadowsCB.SetGlobalVector("ShadowsKernel", new Vector2(0.0f, 1.0f));
        computeShadowsCB.Blit(cShadow, cShadow2, mMaterial, 2);
        computeShadowsCB.SetGlobalVector("ShadowsKernel", new Vector2(1.0f, 0.0f));
        computeShadowsCB.Blit(cShadow2, cShadow, mMaterial, 2);

        //temporal
        if (m_Temporal)
        {
            //TempRT = RenderTexture.GetTemporary(mCamera.pixelWidth, mCamera.pixelHeight, 0, RenderTextureFormat.R8);
            computeShadowsCB.SetGlobalTexture("NGSS_Temporal_Tex", TempRT);
            computeShadowsCB.Blit(cShadow, cShadow2, mMaterial, 3);
            computeShadowsCB.Blit(cShadow2, TempRT);

            computeShadowsCB.SetGlobalTexture("NGSS_FrustumShadowsTexture", TempRT);//cShadow
        }
        else
        {
            computeShadowsCB.SetGlobalTexture("NGSS_FrustumShadowsTexture", cShadow);
        }
        
        // computeShadowsCB.ReleaseTemporaryRT(cShadow);
        // computeShadowsCB.ReleaseTemporaryRT(cShadow2);
        // computeShadowsCB.ReleaseTemporaryRT(dSource);
        
        isInitialized = true;
    }
    
    /******************************************************************/

    void OnEnable()
    {
        if (IsNotSupported())
        {
            Debug.LogWarning("Unsupported graphics API, NGSS requires at least SM3.0 or higher and DX9 is not supported.", this);
            enabled = false;
            return;
        }

        if (m_Temporal) { mCamera.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.MotionVectors; } else { mCamera.depthTextureMode = DepthTextureMode.Depth; }

        Init();
    }

    void OnDisable()
    {
        Shader.SetGlobalFloat("NGSS_FRUSTUM_SHADOWS_ENABLED", 0f);

        if (isInitialized) { RemoveCommandBuffers(); }

        if (TempRT != null)
        {
            RenderTexture.ReleaseTemporary(TempRT);
            TempRT = null;
        }

        //mCamera.depthTextureMode &= ~(DepthTextureMode.MotionVectors);
        //#if UNITY_EDITOR
        //if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera != null)
        //UnityEditor.SceneView.currentDrawingSceneView.camera.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
        //#endif
    }

    void OnApplicationQuit()
    {
        if (isInitialized) { RemoveCommandBuffers(); }
    }
    
    /******************************************************************/

    void OnPreCull()
    {
        //Vector2 offset = GenerateRandomOffset();
        //mCamera.nonJitteredProjectionMatrix = mCamera.projectionMatrix;
        //mCamera.projectionMatrix = GetPerspectiveProjectionMatrix(offset);
        //mMaterial.SetMatrix("_TemporalMatrix", GetPerspectiveProjectionMatrix(offset));
    }

    void OnPreRender()
    {
        Init();
        if (isInitialized == false || mainShadowsLight == null) { return; }
        
        if (currentRenderingPath != mCamera.renderingPath)
        {
            RemoveCommandBuffers();
            AddCommandBuffers();
        }

        Shader.SetGlobalFloat("NGSS_FRUSTUM_SHADOWS_ENABLED", 1f);
        Shader.SetGlobalFloat("NGSS_FRUSTUM_SHADOWS_OPACITY", 1f - mainShadowsLight.shadowStrength);

        if (m_Temporal != isTemporal) { enabled = false; isTemporal = m_Temporal; enabled = true; }
        mMaterial.SetFloat("_TemporalScale", m_Temporal ? Mathf.Clamp(m_Scale, 0f, 0.99f) : 0f);
        mMaterial.SetVector("_Jitter_Offset", m_Temporal ? (GenerateRandomOffset() * m_Jittering) : Vector2.zero);

        //mMaterial.SetMatrix("InverseProj", Matrix4x4.Inverse(mCamera.projectionMatrix));//proj to cam        
        //mMaterial.SetMatrix("InverseView", mCamera.cameraToWorldMatrix);//cam to world        
        //mMaterial.SetMatrix("InverseViewProj", Matrix4x4.Inverse(GL.GetGPUProjectionMatrix(mCamera.projectionMatrix, false) * mCamera.worldToCameraMatrix));//proj to world        
        mMaterial.SetMatrix("WorldToView", mCamera.worldToCameraMatrix);//cam to world        
        mMaterial.SetVector("LightPos", mainShadowsLight.transform.position);//world position
        mMaterial.SetVector("LightDir", -mCamera.transform.InverseTransformDirection(mainShadowsLight.transform.forward));//view space direction
        mMaterial.SetVector("LightDirWorld", -mainShadowsLight.transform.forward);//view space direction
        //mMaterial.SetFloat("ShadowsOpacity", 1f - mainShadowsLight.shadowStrength);        
        mMaterial.SetFloat("ShadowsEdgeTolerance", m_shadowsEdgeBlur * 0.075f);
        mMaterial.SetFloat("ShadowsSoftness", m_shadowsBlur);
        //mMaterial.SetFloat("ShadowsDistance", m_shadowsDistance);
        mMaterial.SetFloat("RayScale", m_rayScale);
        //mMaterial.SetFloat("ShadowsFade", m_shadowsFade);
        mMaterial.SetFloat("ShadowsBias", m_shadowsBias * 0.02f);
        mMaterial.SetFloat("ShadowsDistanceStart", m_shadowsDistanceStart - 10f);
        mMaterial.SetFloat("RayThickness", m_rayThickness);
        mMaterial.SetFloat("RaySamples", (float)m_raySamples);
        //mMaterial.SetFloat("RaySamplesScale", m_raySamplesScale);
        if (m_deferredBackfaceOptimization && mCamera.actualRenderingPath == RenderingPath.DeferredShading) { mMaterial.EnableKeyword("NGSS_DEFERRED_OPTIMIZATION"); mMaterial.SetFloat("BackfaceOpacity", m_deferredBackfaceTranslucency); } else { mMaterial.DisableKeyword("NGSS_DEFERRED_OPTIMIZATION"); }
        if (m_dithering) { mMaterial.EnableKeyword("NGSS_USE_DITHERING"); } else { mMaterial.DisableKeyword("NGSS_USE_DITHERING"); }
        if (m_fastBlur) { mMaterial.EnableKeyword("NGSS_FAST_BLUR"); } else { mMaterial.DisableKeyword("NGSS_FAST_BLUR"); }
        if (mainShadowsLight.type != LightType.Directional) { mMaterial.EnableKeyword("NGSS_USE_LOCAL_SHADOWS"); } else { mMaterial.DisableKeyword("NGSS_USE_LOCAL_SHADOWS"); }
        
        //if (m_rayScreenScale) { mMaterial.EnableKeyword("NGSS_RAY_SCREEN_SCALE"); } else { mMaterial.DisableKeyword("NGSS_RAY_SCREEN_SCALE"); }
        mMaterial.SetFloat("RayScreenScale", m_rayScreenScale ? 1f : 0f);
    }
    //uncomment me if using the screen space blit | comment me if sampling directly from internal NGSS libraries
    void OnPostRender()
    {
        Shader.SetGlobalFloat("NGSS_FRUSTUM_SHADOWS_ENABLED", 0f);
        //Shader.SetGlobalTexture("NGSS_FrustumShadowsTexture", noMixTexture);//don't render shadows
        //Shader.SetGlobalTexture("NGSS_Temporal_HistoryTex", noMixTexture);
        //mCamera.ResetProjectionMatrix();
    }
    
    /******************************************************************/

    private float GetHaltonValue(int index, int radix)
    {
        float result = 0.0f;
        float fraction = 1.0f / (float)radix;

        while (index > 0)
        {
            result += (float)(index % radix) * fraction;

            index /= radix;
            fraction /= (float)radix;
        }

        return result;
    }

    private Vector2 GenerateRandomOffset()
    {
        Vector2 offset = new Vector2(GetHaltonValue(m_SampleIndex & 1023, 2), GetHaltonValue(m_SampleIndex & 1023, 3));

        if (++m_SampleIndex >= 16)
            m_SampleIndex = 0;

        float vertical = Mathf.Tan(0.5f * Mathf.Deg2Rad * mCamera.fieldOfView);
        float horizontal = vertical * mCamera.aspect;

        offset.x *= horizontal / (0.5f * mCamera.pixelWidth);
        offset.y *= vertical / (0.5f * mCamera.pixelHeight);

        return offset;
    }
    
    /******************************************************************/
    
    Mesh fullScreenTriangle;
    
    void InitializeTriangle ()
    {
        if (fullScreenTriangle) {
            return;
        }
        fullScreenTriangle = new Mesh {
            name = "My Post-Processing Stack Full-Screen Triangle",
            vertices = new Vector3[] {
                new Vector3(-1f, -1f, 0f),
                new Vector3(-1f,  3f, 0f),
                new Vector3( 3f, -1f, 0f)
            },
            triangles = new int[] { 0, 1, 2 },
        };
        fullScreenTriangle.UploadMeshData(true);
    }
    
    void CreateFullscreenQuad(Camera cam)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        vertices[0] = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane + 0.01f));
        vertices[1] = cam.ViewportToWorldPoint(new Vector3(1f, 0f, cam.nearClipPlane + 0.01f));
        vertices[2] = cam.ViewportToWorldPoint(new Vector3(0f, 1f, cam.nearClipPlane + 0.01f));
        vertices[3] = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cam.nearClipPlane + 0.01f));

        Vector2[] uvs = new Vector2[4] {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };

        int[] triangles = new int[6] {
            0, 3, 1, 0, 2, 3
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        fullScreenTriangle = mesh;
    }
}
