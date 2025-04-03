using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
#endif

[RequireComponent(typeof(CapsuleCollider)),RequireComponent(typeof(Rigidbody)),AddComponentMenu("First Person AIO")]

public class FirstPersonAIO_Lite : MonoBehaviour {

    public string versionNum = "19.9.13cu";

    #region Look Settings
    public float verticalRotationRange = 170;
    public float mouseSensitivityInternal;
    public  float FOVToMouseInternal = 1;
    public Camera playerCamera;
    internal Vector3 cameraStartingPosition;
    float baseCamFOV;
    

    public Vector3 targetAngles;
    private Vector3 followAngles;
    private Vector3 followVelocity;
    private Vector3 originalRotation;
    #endregion

    #region Movement Settings

    public float speed;
    internal float walkSpeedInternal;
    internal float jumpPowerInternal;
    internal float colliderHeight;
    private CapsuleCollider capsule;
    public bool IsGrounded { get; private set; }
    Vector2 inputXY;
    public bool isCrouching;
    bool isSprinting = false;
    PhysicMaterial highFrictionMaterial;
    PhysicMaterial zeroFrictionMaterial;

    public Rigidbody fps_Rigidbody;

    #endregion

    #region Headbobbing Settings
    public Transform head = null;
    public float headbobFrequency = 1.5f; 
    private Vector3 originalLocalPosition;
    private float headbobCycle = 0.0f;
    private float headbobFade = 0.0f;
    private float springPosition = 0.0f;
    private float springVelocity = 0.0f;
    Vector3 previousPosition;
    Vector3 previousVelocity = Vector3.zero;
    Vector3 miscRefVel;
    AudioSource audioSource;

    #endregion

    private void Awake()
    {
        #region Look Settings - Awake
        originalRotation = transform.localRotation.eulerAngles;

        #endregion 

        #region Movement Settings - Awake
        walkSpeedInternal = 4;
        jumpPowerInternal = 5;
        capsule = GetComponent<CapsuleCollider>();
        IsGrounded = true;
        isCrouching = false;
        fps_Rigidbody = GetComponent<Rigidbody>();
        colliderHeight = capsule.height;
        #endregion
    }

    private void Start()
    {
        #region Look Settings - Start
        playerCamera = transform.GetComponentInChildren<Camera>();
        mouseSensitivityInternal = 10;
        FOVToMouseInternal = 1;
        cameraStartingPosition = playerCamera.transform.localPosition;
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        baseCamFOV = playerCamera.fieldOfView;
        #endregion

        #region Headbobbing Settings - Start
        head = transform.GetChild(0);
        originalLocalPosition = head.localPosition;
        if(GetComponent<AudioSource>() == null) { gameObject.AddComponent<AudioSource>(); }
        previousPosition = fps_Rigidbody.position;
        audioSource = GetComponent<AudioSource>();
        #endregion

        zeroFrictionMaterial = new PhysicMaterial("Zero_Friction");
        zeroFrictionMaterial.dynamicFriction =0;
        zeroFrictionMaterial.staticFriction =0;
        zeroFrictionMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        zeroFrictionMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
        highFrictionMaterial = new PhysicMaterial("Max_Friction");
        highFrictionMaterial.dynamicFriction =1;
        highFrictionMaterial.staticFriction =1;
        highFrictionMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
        highFrictionMaterial.bounceCombine = PhysicMaterialCombine.Average;
    }

    private void Update()
    {
        #region Look Settings - Update
            float mouseXInput;
            float mouseYInput;
            float camFOV = playerCamera.fieldOfView;
            mouseXInput = Input.GetAxis("Mouse Y");
            mouseYInput = Input.GetAxis("Mouse X");
            if(targetAngles.y > 180) { targetAngles.y -= 360; followAngles.y -= 360; } else if(targetAngles.y < -180) { targetAngles.y += 360; followAngles.y += 360; }
            if(targetAngles.x > 180) { targetAngles.x -= 360; followAngles.x -= 360; } else if(targetAngles.x < -180) { targetAngles.x += 360; followAngles.x += 360; }
            targetAngles.y += mouseYInput * (mouseSensitivityInternal - ((baseCamFOV-camFOV)*FOVToMouseInternal)/5f);
            targetAngles.x += mouseXInput * (mouseSensitivityInternal - ((baseCamFOV-camFOV)*FOVToMouseInternal)/5f);
            targetAngles.y = Mathf.Clamp(targetAngles.y, -0.5f * Mathf.Infinity, 0.5f * Mathf.Infinity);
            targetAngles.x = Mathf.Clamp(targetAngles.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);
            followAngles = Vector3.SmoothDamp(followAngles, targetAngles, ref followVelocity, 0.05f);
            playerCamera.transform.localRotation = Quaternion.Euler(-followAngles.x + originalRotation.x,0,0);
            transform.localRotation =  Quaternion.Euler(0, followAngles.y+originalRotation.y, 0);
        #endregion
    }

    private void FixedUpdate()
    {
        #region Movement Settings - FixedUpdate
        
        bool wasWalking = !isSprinting;
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        Vector3 dMove = Vector3.zero;
        speed = isCrouching ? walkSpeedInternal : (isSprinting ? 8 : walkSpeedInternal);
        Ray ray = new Ray(transform.position, -transform.up);
        if(IsGrounded || fps_Rigidbody.velocity.y < 0.1) {
            RaycastHit[] hits = Physics.RaycastAll(ray, capsule.height * 0.7f);
            float nearest = float.PositiveInfinity;
            IsGrounded = false;
            for(int i = 0; i < hits.Length; i++) {
                if(!hits[i].collider.isTrigger && hits[i].distance < nearest) {
                    IsGrounded = true;
                    nearest = hits[i].distance;
                }
            }
        }
  

        dMove = transform.forward * inputXY.y * speed + transform.right * inputXY.x * walkSpeedInternal;
        


        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        inputXY = new Vector2(horizontalInput, verticalInput);
        if(inputXY.magnitude > 1) { inputXY.Normalize(); }
       
        float yv = fps_Rigidbody.velocity.y;
        bool didJump = Input.GetButton("Jump");

        if(IsGrounded && didJump && jumpPowerInternal > 0)
        {
            yv += jumpPowerInternal;
            IsGrounded = false;
            didJump=false;
        }

        fps_Rigidbody.velocity = dMove + Vector3.up * yv;

        fps_Rigidbody.AddForce(Physics.gravity);

      
            isCrouching = Input.GetKey(KeyCode.LeftControl);

            if(isCrouching) {
                    capsule.height = Mathf.MoveTowards(capsule.height, colliderHeight/2, 5*Time.deltaTime);
                        walkSpeedInternal =2f;
                        jumpPowerInternal = 0;
                } else {
                capsule.height = Mathf.MoveTowards(capsule.height, colliderHeight, 5*Time.deltaTime);    
                walkSpeedInternal = 4;
                jumpPowerInternal = 5;
            }
        
        if(dMove.magnitude > 0 || !IsGrounded) {
            capsule.sharedMaterial = zeroFrictionMaterial;
        } else { capsule.sharedMaterial = highFrictionMaterial; }

        #endregion

        #region Headbobbing Settings - FixedUpdate
        float yPos = 0;
        float xPos = 0;
        float zTilt = 0;
        float xTilt = 0;
        float bobSwayFactor;
        float bobFactor;
        float strideLangthen;
        float flatVel;

            Vector3 vel = (fps_Rigidbody.position - previousPosition) / Time.deltaTime;
            Vector3 velChange = vel - previousVelocity;
            previousPosition = fps_Rigidbody.position;
            previousVelocity = vel;
            springVelocity -= velChange.y;
            springVelocity -= springPosition * 1.1f;
            springVelocity *= 0.8f;
            springPosition += springVelocity * Time.deltaTime;
            springPosition = Mathf.Clamp(springPosition, -0.3f, 0.3f);

            if(Mathf.Abs(springVelocity) < 0.05f && Mathf.Abs(springPosition) < 0.05f) { springPosition = 0; springVelocity = 0; }
            flatVel = new Vector3(vel.x, 0.0f, vel.z).magnitude;
            strideLangthen = 1 + (flatVel * ((headbobFrequency*2)/10));
            headbobCycle += (flatVel / strideLangthen) * (Time.deltaTime / headbobFrequency);
            bobFactor = Mathf.Sin(headbobCycle * Mathf.PI * 2);
            bobSwayFactor = Mathf.Sin(Mathf.PI * (2 * headbobCycle + 0.5f));
            bobFactor = 1 - (bobFactor * 0.5f + 1);
            bobFactor *= bobFactor;

            yPos = 0;
            xPos = 0;
            zTilt = 0;
            xTilt = -springPosition;

            if(IsGrounded)
            {
                if(new Vector3(vel.x, 0.0f, vel.z).magnitude < 0.1f) { headbobFade = Mathf.MoveTowards(headbobFade, 0.0f, Time.deltaTime); } else { headbobFade = Mathf.MoveTowards(headbobFade, 1.0f, Time.deltaTime); }
                float speedHeightFactor = 1 + (flatVel * 0.3f);
                xPos = -0.05f * bobSwayFactor;
                yPos = springPosition * 0.3f + bobFactor * 0.1f * headbobFade * speedHeightFactor;
                zTilt = bobSwayFactor * 0.25f * headbobFade;
            }

                if(fps_Rigidbody.velocity.magnitude >0.1f){
                    head.localPosition = Vector3.MoveTowards(head.localPosition, originalLocalPosition + new Vector3(xPos, yPos, 0),0.02f);
                }else{
                    head.localPosition = Vector3.SmoothDamp(head.localPosition, originalLocalPosition,ref miscRefVel, 0.15f);
                }
                head.localRotation = Quaternion.Euler(xTilt, 0, zTilt);
        #endregion

    }
}
#if UNITY_EDITOR
    [CustomEditor(typeof(FirstPersonAIO_Lite)),InitializeOnLoadAttribute]
    public class FPAIO_Lite_Editor : Editor{
        FirstPersonAIO_Lite t;
        void OnEnable(){
            t = (FirstPersonAIO_Lite)target;
        }
        public override void OnInspectorGUI(){
            EditorGUILayout.Space();
            GUILayout.Label("<b>First Person AIO</b> <i>Lite</i>",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter, richText = true, fontSize = 16});
            GUILayout.Label("version: "+ t.versionNum,new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter});
            GUILayout.Label("This is a stripped down version of\nthe First Person AIO and has been locked down.", new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter, richText = true,});
            if(GUILayout.Button("Get the full, unlocked version free here",new GUIStyle(GUI.skin.button){alignment = TextAnchor.MiddleCenter,richText = true })){Application.OpenURL("http://u3d.as/1p8g");}
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label("Note that this script is not required by the <i>InfiniGun</i> system,\nalmost any FPS controller will work.", new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter, richText = true,});
            if(GUI.changed){EditorUtility.SetDirty(t); Undo.RecordObject(t,"FPAIO Change");}
        }
    }
#endif




