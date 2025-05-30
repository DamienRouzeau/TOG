﻿// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

 // Use the non-batching version of this shader if you are using
// local mode, and are seeing the textures move when you zoom in
// due to dynamic batching. (Dynamic batching converts local vertex
// data into world space, making local vertex position data unavailable
// to the shader.)

Shader "UVFree/Legacy/Specular" {
	Properties {
		// Triplanar space, for UI
		[Toggle(_UVFREE_LOCAL)] _UVFreeLocal ("Use Local Space (instead of Global)", Float) = 0.0
		[Toggle(_UVFREE_VERTEX_COLOR)] _UVFreeVertexColor ("Use Vertex Color", Float) = 0.0	
		[Toggle(_UVFREE_BUMPED)] _UVFreeBumped ("Use Normal Mapping", Float) = 1.0
		[Toggle(_UVFREE_RIM)] _UVFreeRim ("Use Rim Lighting", Float) = 0.0
		[Toggle(_EMISSION)] _Emission ("Use Emission", Float) = 0.0
		
		_TexPower("Texture Power", Range(0.0,20.0)) = 10.0
		
		_Color ("Tint", Color) = (1.0,1.0,1.0,1.0)
		_MainTex ("Base Color (RGB) Gloss (A)", 2D) = "white" {}
		
		_VertexColorStrength("Vertex Color Strength", Range(0.0,1.0)) = 0.0
		
		_BumpScale("Bump Scale", Range(-2.0, 2.0)) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}
		
		_Shininess("Shininess", Range(0.01, 1.0)) = 0.078125
		_SpecColor("Specular", Color) = (0.5,0.5,0.5,1.0)
		
		_RimMultiplier ("Rim Multiplier", Range(0.0,8.0)) = 0.0
		_RimColor ("Rim Color (RGB) Strength (A)", Color) = (1.0,1.0,1.0,1.0)
		_RimPower ("Rim Power", Range(0.0,16.0)) = 5.0

		_EmissionMultiplier("Emission Multiplier", Float) = 0.0
		_EmissionColor("Emission Color", Color) = (0.0,0.0,0.0)
		_EmissionMap("Emission", 2D) = "white" {}
	}
	SubShader {
		Tags {
			"RenderType"="Opaque"
			"PerformanceChecks"="False"
		}	
		LOD 300
		
		CGPROGRAM
				
		#pragma target 3.0
		#pragma shader_feature_local _UVFREE_LOCAL
		#pragma shader_feature_local _UVFREE_BUMPED
		#pragma shader_feature_local _UVFREE_RIM
		#pragma shader_feature_local _UVFREE_VERTEX_COLOR
		#pragma shader_feature_local _EMISSION
		
		// Uncomment to enable experimental feature which flips
		// backward textures. Note: Causes some normals to be flipped.
		// #define _UVFREE_FLIP_BACKWARD_TEXTURES
		
		#pragma surface surf BlinnPhong vertex:vert fullforwardshadows
		#include "UnityCG.cginc"

		// Instanced Properties
		// https://docs.unity3d.com/Manual/GPUInstancing.html

		UNITY_INSTANCING_BUFFER_START (MyProperties)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
#define _Color_arr MyProperties
		UNITY_INSTANCING_BUFFER_END(MyProperties)

		// Non-instanced properties
		half _TexPower;
		fixed _VertexColorStrength;

		sampler2D	_MainTex;
		float4 _MainTex_ST;
		sampler2D	_BumpMap;
		half _BumpScale;
		sampler2D	_EmissionMap;
		half _EmissionMultiplier;
		half4 _EmissionColor;
		fixed4 _RimColor;
		half _RimPower;
		half _RimMultiplier;
		float _Shininess;
		
		struct Input {
			fixed3 powerNormal;
			
			#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
				fixed3 normal;
			#endif
						
			float3 worldPos;

			#ifdef _UVFREE_RIM
				fixed3 viewDir;
			#endif
			
			#ifdef _UVFREE_VERTEX_COLOR
				fixed4 color:COLOR;
			#endif

			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		void vert (inout appdata_full v, out Input o) {
		
			UNITY_INITIALIZE_OUTPUT(Input,o);
						
			#ifdef _UVFREE_LOCAL
				#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
					o.normal = v.normal;
				#endif
				o.powerNormal = pow(abs(v.normal), _TexPower);	
				
				o.powerNormal = max(o.powerNormal, 0.0001);
				o.powerNormal /= dot(o.powerNormal, 1.0);
				
				v.tangent.xyz = 
					cross(v.normal, fixed3(0.0,sign(v.normal.x),0.0)) * (o.powerNormal.x)
				  + cross(v.normal, fixed3(0.0,0.0,sign(v.normal.y))) * (o.powerNormal.y)
				  + cross(v.normal, fixed3(0.0,sign(v.normal.z),0.0)) * (o.powerNormal.z)
				;
				
				v.tangent.w = 
					(-(v.normal.x) * (o.powerNormal.x))
				  +	(-(v.normal.y) * (o.powerNormal.y))
				  +	(-(v.normal.z) * (o.powerNormal.z))
				;
				
			#else
				fixed3 worldNormal = normalize(mul(unity_ObjectToWorld, fixed4(v.normal, 0.0)).xyz);
				#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
					o.normal = worldNormal;
				#endif
				
				o.powerNormal = pow(abs(worldNormal), _TexPower);
				o.powerNormal = max(o.powerNormal, 0.0001);
				o.powerNormal /= dot(o.powerNormal, 1.0);
								
				v.tangent.xyz = 
					cross(v.normal, mul(unity_WorldToObject,fixed4(0.0,sign(worldNormal.x),0.0,0.0)).xyz * (o.powerNormal.x))
				  + cross(v.normal, mul(unity_WorldToObject,fixed4(0.0,0.0,sign(worldNormal.y),0.0)).xyz * (o.powerNormal.y))
				  + cross(v.normal, mul(unity_WorldToObject,fixed4(0.0,sign(worldNormal.z),0.0,0.0)).xyz * (o.powerNormal.z))
				;
				
				v.tangent.w = 
					(-(worldNormal.x) * (o.powerNormal.x))
				  +	(-(worldNormal.y) * (o.powerNormal.y))
				  +	(-(worldNormal.z) * (o.powerNormal.z))
				;
			#endif	
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
		
			// TRIPLANAR UVs BASED ON WORLD OR LOCAL POSITION
			//
			
			#ifdef _UVFREE_LOCAL
				float3 pos = mul(unity_WorldToObject, float4(IN.worldPos, 1.0)).xyz;
				
				float2 posX = pos.zy;
				float2 posY = pos.xz;
				float2 posZ = float2(-pos.x, pos.y);
			#else
				float2 posX = IN.worldPos.zy;
				float2 posY = IN.worldPos.xz;
				float2 posZ = float2(-IN.worldPos.x, IN.worldPos.y);				
			#endif

			float2 xUV = posX * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 yUV = posY * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 zUV = posZ * _MainTex_ST.xy + _MainTex_ST.zw;
			
			#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
				fixed3 powerSign = sign(IN.normal);
				xUV.x *= powerSign.x;
				zUV.x *= powerSign.z;
				yUV.y *= powerSign.y;
			#endif
			
			// DIFFUSE
			//
			
			fixed4 texX = tex2D(_MainTex, xUV);
			fixed4 texY = tex2D(_MainTex, yUV);
			fixed4 texZ = tex2D(_MainTex, zUV);			
			
			fixed4 tex = 
			    texX * IN.powerNormal.x
			  + texY * IN.powerNormal.y
			  + texZ * IN.powerNormal.z;
			 
			#ifdef _UVFREE_VERTEX_COLOR			 
				tex *= lerp(fixed4(1.0,1.0,1.0,1.0), IN.color, _VertexColorStrength);
			#endif
			
			fixed4 tint = UNITY_ACCESS_INSTANCED_PROP (_Color_arr, _Color);
			o.Albedo = max(fixed3(0.001,0.001,0.001), tex.rgb * tint.rgb);
			o.Alpha = 1.0;
			
			// NORMAL
			//

			#ifdef _UVFREE_BUMPED
				fixed3 bumpX = UnpackScaleNormal(tex2D(_BumpMap, xUV), _BumpScale);
				fixed3 bumpY = UnpackScaleNormal(tex2D(_BumpMap, yUV), _BumpScale);
				fixed3 bumpZ = UnpackScaleNormal(tex2D(_BumpMap, zUV), _BumpScale);
				
				fixed3 bump = 
				    bumpX * IN.powerNormal.x
				  + bumpY * IN.powerNormal.y
				  + bumpZ * IN.powerNormal.z
				;
				
				o.Normal = normalize(bump);
			#else
				o.Normal = fixed3(0.0,0.0,1.0);
			#endif

			// SPECULAR/GLOSS
			//
			o.Gloss = tex.a * _SpecColor.a;
			o.Specular = _Shininess;

			// RIM
			//
			#ifdef _UVFREE_RIM
				half rimStrength = 1.0 - max(0.0, dot(normalize(IN.viewDir), o.Normal));
				o.Emission = _RimMultiplier * _RimColor.rgb * pow(rimStrength, _RimPower) * _RimColor.a;
			#endif
			
			// EMISSION
			//
			#ifdef _EMISSION
				fixed3 emissionX = tex2D(_EmissionMap, xUV).rgb;
				fixed3 emissionY = tex2D(_EmissionMap, yUV).rgb;
				fixed3 emissionZ = tex2D(_EmissionMap, zUV).rgb;
				o.Emission +=
					(emissionX * IN.powerNormal.x + emissionY * IN.powerNormal.y + emissionZ * IN.powerNormal.z)
					* _EmissionColor.rgb
					* _EmissionMultiplier;
				
			#endif			
		}
		ENDCG
	} 
	FallBack "Diffuse"	
	CustomEditor "UVFreeLegacyTopBottomGUI"
}
