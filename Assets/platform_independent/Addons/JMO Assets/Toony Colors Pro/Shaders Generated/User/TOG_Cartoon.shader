// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

Shader "Toony Colors Pro 2/User/TOG_Cartoon"
{
	Properties
	{
	[TCP2HeaderHelp(BASE, Base Properties)]
		//TOONY COLORS
		_Color ("Color", Color) = (1,1,1,1)
		_HColor ("Highlight Color", Color) = (0.785,0.785,0.785,1.0)
		_SColor ("Shadow Color", Color) = (0.195,0.195,0.195,1.0)
		_HighlightMultiplier ("Highlight Multiplier", Range(0,4)) = 1
		_ShadowMultiplier ("Shadow Multiplier", Range(0,4)) = 1
		_WrapFactor ("Light Wrapping", Range(-1,3)) = 1.0

		//DIFFUSE
		_MainTex ("Main Texture", 2D) = "white" {}
		_Detail ("Detail (RGB)", 2D) = "gray" {}
		_DiffTint ("Diffuse Tint", Color) = (0.7,0.8,1,1)
	[TCP2Separator]

		//TOONY COLORS RAMP
		[TCP2Header(RAMP SETTINGS)]

		[Header(Main Directional Light)]
		_RampThreshold ("Ramp Threshold", Range(0,1)) = 0.5
		_RampSmooth ("Ramp Smoothing", Range(0.001,1)) = 0.1
		[Header(Other Lights)]
		_RampThresholdOtherLights ("Threshold", Range(0,1)) = 0.5
		_RampSmoothOtherLights ("Smoothing", Range(0.001,1)) = 0.5
		[Space]
	[TCP2Separator]

	[TCP2HeaderHelp(EMISSION, Emission)]
		[NoScaleOffset] _EmissionMap ("Emission (RGB)", 2D) = "black" {}
		[HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1.0)
	[TCP2Separator]

	[TCP2HeaderHelp(NORMAL MAPPING, Normal Bump Map)]
		//BUMP
		_BumpMap ("Normal map (RGB)", 2D) = "bump" {}
		_BumpScale ("Scale", Float) = 1.0
	[TCP2Separator]

	[TCP2HeaderHelp(AMBIENT OCCLUSION, Ambient Occlusion)]
		//AMBIENT OCCLUSION
		_OcclusionMap ("Occlusion (Alpha)", 2D) = "white" {}
		_OcclusionStrength ("Strength", Range(0.0, 1.0)) = 1.0
	[TCP2Separator]

	[TCP2HeaderHelp(SPECULAR, Specular)]
		//SPECULAR
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Smoothness ("Size", Float) = 0.2
		_SpecSmooth ("Smoothness", Range(0,1)) = 0.05
	[TCP2Separator]

	[TCP2HeaderHelp(RIM, Rim)]
		//RIM LIGHT
		_RimColor ("Rim Color", Color) = (0.8,0.8,0.8,0.6)
		_RimMin ("Rim Min", Range(0,2)) = 0.5
		_RimMax ("Rim Max", Range(0,2)) = 1.0
	[TCP2Separator]

	[TCP2HeaderHelp(DISSOLVE)]
		_DissolveMap ("Dissolve Map", 2D) = "white" {}
		_DissolveValue ("Dissolve Value", Range(0,1)) = 0.5
		[TCP2Gradient] _DissolveRamp ("Dissolve Ramp", 2D) = "white" {}
		_DissolveGradientWidth ("Ramp Width", Range(0,1)) = 0.2
	[TCP2Separator]


		//Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0
	}

	SubShader
	{

		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}

		CGPROGRAM

		#pragma surface surf ToonyColorsCustom addshadow noambient vertex:vert exclude_path:deferred exclude_path:prepass
		#pragma target 3.0

		//================================================================
		// VARIABLES

		fixed4 _Color;
		sampler2D _MainTex;
		sampler2D _Detail;
		sampler2D _DissolveMap;
		half _DissolveValue;
		sampler2D _DissolveRamp;
		half _DissolveGradientWidth;
		half4 _EmissionColor;
		sampler2D _EmissionMap;
		sampler2D _BumpMap;
		half _BumpScale;
		sampler2D _OcclusionMap;
		half _OcclusionStrength;
		fixed _Smoothness;
		fixed4 _RimColor;
		fixed _RimMin;
		fixed _RimMax;
		float4 _RimDir;

		#define UV_MAINTEX uv_MainTex

		struct Input
		{
			half2 uv_MainTex;
			half2 uv2_Detail;
			half2 uv_BumpMap;
			float3 viewDir;
			fixed3 ambient;
			half2 uv_DissolveMap;
		};

		//================================================================
		// CUSTOM LIGHTING

		//Lighting-related variables
		fixed4 _HColor;
		fixed4 _SColor;
		half _WrapFactor;
		fixed _HighlightMultiplier;
		fixed _ShadowMultiplier;
		half _RampThreshold;
		half _RampSmooth;
		half _RampThresholdOtherLights;
		half _RampSmoothOtherLights;
		fixed _SpecSmooth;
		fixed4 _DiffTint;

		// Instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		//Custom SurfaceOutput
		struct SurfaceOutputCustom
		{
			half atten;
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Gloss;
			fixed Alpha;
		};

		inline half4 LightingToonyColorsCustom (inout SurfaceOutputCustom s, half3 viewDir, UnityGI gi)
		{
		#define IN_NORMAL s.Normal
	
			half3 lightDir = gi.light.dir;
		#if defined(UNITY_PASS_FORWARDBASE)
			half3 lightColor = _LightColor0.rgb;
			half atten = s.atten;
		#else
			half3 lightColor = gi.light.color.rgb;
			half atten = 1;
		#endif

			IN_NORMAL = normalize(IN_NORMAL);
			fixed ndl = max(0, (dot(IN_NORMAL, lightDir) + _WrapFactor) / (1+_WrapFactor));
			#define NDL ndl

		#if defined(UNITY_PASS_FORWARDBASE)
			#define		RAMP_THRESHOLD	_RampThreshold
			#define		RAMP_SMOOTH		_RampSmooth
		#else
			#define		RAMP_THRESHOLD	_RampThresholdOtherLights
			#define		RAMP_SMOOTH		_RampSmoothOtherLights
		#endif

			fixed3 ramp = smoothstep(RAMP_THRESHOLD - RAMP_SMOOTH*0.5, RAMP_THRESHOLD + RAMP_SMOOTH*0.5, NDL);
		#if !(POINT) && !(SPOT)
			ramp *= atten;
		#endif
		// Note: we consider that a directional light with a cookie is supposed to be the main one (even though Unity renders it as an additional light).
		// Thus when using a main directional light AND another directional light with a cookie, then the shadow color might be applied twice.
		// You can remove the DIRECTIONAL_COOKIE check below the prevent that.
		#if !defined(UNITY_PASS_FORWARDBASE) && !defined(DIRECTIONAL_COOKIE)
			_SColor = fixed4(0,0,0,1);
		#endif
			_SColor = lerp(_HColor, _SColor, _SColor.a * _ShadowMultiplier);	//Shadows intensity through alpha
			_HColor *= _HighlightMultiplier;
			ramp = lerp(_SColor.rgb, _HColor.rgb, ramp);
			fixed3 wrappedLight = saturate(_DiffTint.rgb + saturate(dot(IN_NORMAL, lightDir)));
			ramp *= wrappedLight;
			//Blinn-Phong Specular (legacy)
			half3 h = normalize(lightDir + viewDir);
			float ndh = max(0, dot (IN_NORMAL, h));
			float spec = pow(ndh, s.Specular*128.0) * s.Gloss * 2.0;
			spec = smoothstep(0.5-_SpecSmooth*0.5, 0.5+_SpecSmooth*0.5, spec);
			spec *= atten;
			fixed4 c;
			c.rgb = s.Albedo * lightColor.rgb * ramp;
		#if (POINT || SPOT)
			c.rgb *= atten;
		#endif

			#define SPEC_COLOR	_SpecColor.rgb
			c.rgb += lightColor.rgb * SPEC_COLOR * spec;
			c.a = s.Alpha;

		#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
			c.rgb += s.Albedo * gi.indirect.diffuse;
		#endif

			return c;
		}

		void LightingToonyColorsCustom_GI(inout SurfaceOutputCustom s, UnityGIInput data, inout UnityGI gi)
		{
			gi = UnityGlobalIllumination(data, 1.0, IN_NORMAL);

			s.atten = data.atten;	//transfer attenuation to lighting function
			gi.light.color = _LightColor0.rgb;	//remove attenuation
		}

		//Vertex input
		struct appdata_tcp2
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 tangent : TANGENT;
	#if UNITY_VERSION >= 550
			UNITY_VERTEX_INPUT_INSTANCE_ID
	#endif
		};

		//================================================================
		// VERTEX FUNCTION

		void vert(inout appdata_tcp2 v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			float3 worldN = UnityObjectToWorldNormal(v.normal);
	#if defined(UNITY_PASS_FORWARDBASE)
			o.ambient = ShadeSH9(float4(worldN,1.0));
	#endif
		}

		//================================================================
		// SURFACE FUNCTION

		void surf(Input IN, inout SurfaceOutputCustom o)
		{
			fixed4 mainTex = tex2D(_MainTex, IN.UV_MAINTEX);

			//Detail Tex
			fixed4 detail = tex2D(_Detail, IN.uv2_Detail);
			mainTex.rgb *= (detail.rgb * 2.0);
			o.Albedo = mainTex.rgb * _Color.rgb;
			o.Emission = 0;	//needed so that surface shader takes emission into account if o.Emission is written inside an #if/#endif block
			o.Alpha = mainTex.a * _Color.a;

			//Dissolve
			fixed4 dslv = tex2D(_DissolveMap, IN.uv_DissolveMap.xy);
			#define DSLV dslv.a
			float dissValue = lerp(-_DissolveGradientWidth, 1, _DissolveValue);
			float dissolveUV = smoothstep(DSLV - _DissolveGradientWidth, DSLV + _DissolveGradientWidth, dissValue);
			half4 dissolveColor = tex2D(_DissolveRamp, dissolveUV.xx);
			dissolveColor *= lerp(0, 2.0, dissolveUV);
			o.Emission += dissolveColor.rgb;

			o.Alpha *= DSLV - dissValue;

			//Specular
			o.Gloss = 1;
			o.Specular = _Smoothness;

			//Normal map
			half4 normalMap = tex2D(_BumpMap, IN.uv_BumpMap.xy);
			o.Normal = UnpackScaleNormal(normalMap, _BumpScale);

			//Rim
			float3 viewDir = normalize(IN.viewDir);
			half rim = 1.0f - saturate( dot(viewDir, o.Normal) );
			rim = smoothstep(_RimMin, _RimMax, rim);
			o.Emission += (_RimColor.rgb * rim) * _RimColor.a;

			//Emission
			half3 emissiveColor = half3(1,1,1);
			emissiveColor *= tex2D(_EmissionMap, IN.UV_MAINTEX);
			emissiveColor *= _EmissionColor.rgb * _EmissionColor.a;
			o.Emission += emissiveColor;

			//Custom Ambient
			half3 customAmbient = IN.ambient;	//either Dir_Ambient or regular Unity SH ambient
			//Occlusion Map
			fixed occlusion = tex2D(_OcclusionMap, IN.UV_MAINTEX).a;
			occlusion = lerp(1, occlusion, _OcclusionStrength);
			customAmbient *= occlusion;
			o.Emission += customAmbient * o.Albedo;
		}

		ENDCG
	}

	Fallback "Diffuse"
	CustomEditor "TCP2_MaterialInspector_SG"
}
