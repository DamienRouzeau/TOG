// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ANGRYMESH/PBR TopProjection Standard Cutout"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_BaseSmoothness("Base Smoothness", Range( 0 , 1)) = 0.5
		_BaseNormalMapIntensity("Base Normal Map Intensity", Range( 0 , 1)) = 1
		_BaseAOIntensity("Base AO Intensity", Range( 0 , 1)) = 1
		_BaseMetallic("Base Metallic", Range( 0 , 1)) = 0
		_BaseAlbedoColor("Base Albedo Color", Color) = (1,1,1,0)
		_BaseBackSideMultiply("Base Back Side Multiply", Color) = (1,1,1,0)
		_BaseAlbedoACutoutMask("Base Albedo (A CutoutMask)", 2D) = "gray" {}
		_BaseMetallicASmoothness("Base Metallic (A Smoothness)", 2D) = "white" {}
		[Normal]_BaseNormalMap("Base Normal Map", 2D) = "bump" {}
		_BaseAO("Base AO", 2D) = "white" {}
		_TopSmoothness("Top Smoothness", Range( 0 , 1)) = 0.5
		_TopUVScale("Top UV Scale", Range( 0.01 , 1)) = 1
		_TopIntensity("Top Intensity", Range( 0 , 1)) = 1
		_TopOffset("Top Offset", Range( 0 , 1)) = 0.5
		_TopContrast("Top Contrast", Range( 0 , 2)) = 1
		_TopNormalIntensity("Top Normal Intensity", Range( 0 , 2)) = 1
		_TopColor("Top Color", Color) = (1,1,1,0)
		[NoScaleOffset]_TopAlbedoASmoothness("Top Albedo (A Smoothness)", 2D) = "gray" {}
		[Normal][NoScaleOffset]_TopNormalMap("Top Normal Map", 2D) = "bump" {}
		_DetailUVScale("Detail UV Scale", Range( 0 , 40)) = 5
		_DetailAlbedoIntensity("Detail Albedo Intensity", Range( 0 , 1)) = 1
		_DetailNormalMapIntensity("Detail Normal Map Intensity", Range( 0 , 2)) = 1
		[NoScaleOffset]_DetailAlbedo("Detail Albedo", 2D) = "gray" {}
		[Normal][NoScaleOffset]_DetailNormalMap("Detail Normal Map", 2D) = "bump" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Off
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			fixed ASEVFace : VFACE;
		};

		uniform fixed _DetailNormalMapIntensity;
		uniform sampler2D _DetailNormalMap;
		uniform half _DetailUVScale;
		uniform sampler2D _BaseNormalMap;
		uniform fixed _BaseNormalMapIntensity;
		uniform float4 _BaseNormalMap_ST;
		uniform sampler2D _TopNormalMap;
		uniform half _TopUVScale;
		uniform fixed _TopNormalIntensity;
		uniform fixed _TopOffset;
		uniform fixed _TopContrast;
		uniform fixed _TopIntensity;
		uniform fixed4 _BaseAlbedoColor;
		uniform sampler2D _BaseAlbedoACutoutMask;
		uniform float4 _BaseAlbedoACutoutMask_ST;
		uniform sampler2D _DetailAlbedo;
		uniform fixed _DetailAlbedoIntensity;
		uniform fixed4 _TopColor;
		uniform sampler2D _TopAlbedoASmoothness;
		uniform float4 _BaseBackSideMultiply;
		uniform sampler2D _BaseMetallicASmoothness;
		uniform float4 _BaseMetallicASmoothness_ST;
		uniform float _BaseMetallic;
		uniform fixed _BaseSmoothness;
		uniform fixed _TopSmoothness;
		uniform sampler2D _BaseAO;
		uniform float4 _BaseAO_ST;
		uniform fixed _BaseAOIntensity;
		uniform float _Cutoff = 0.5;


		inline float3 TriplanarSamplingSNF( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float tilling, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= projNormal.x + projNormal.y + projNormal.z;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = ( tex2D( topTexMap, tilling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( topTexMap, tilling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( topTexMap, tilling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			xNorm.xyz = half3( UnpackNormal( xNorm ).xy * float2( nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz = half3( UnpackNormal( yNorm ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz = half3( UnpackNormal( zNorm ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z );
		}


		inline float4 TriplanarSamplingSF( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float tilling, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= projNormal.x + projNormal.y + projNormal.z;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = ( tex2D( topTexMap, tilling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( topTexMap, tilling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( topTexMap, tilling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord183 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float2 temp_output_182_0 = ( uv_TexCoord183 * _DetailUVScale );
			float2 DetailUVScale191 = temp_output_182_0;
			float2 uv_BaseNormalMap = i.uv_texcoord * _BaseNormalMap_ST.xy + _BaseNormalMap_ST.zw;
			float3 tex2DNode96 = UnpackScaleNormal( tex2D( _BaseNormalMap, uv_BaseNormalMap ) ,_BaseNormalMapIntensity );
			float TopUVScale197 = _TopUVScale;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 triplanar352 = TriplanarSamplingSNF( _TopNormalMap, ase_worldPos, ase_worldNormal, 1.0, TopUVScale197, 0 );
			float3 tanTriplanarNormal352 = mul( ase_worldToTangent, triplanar352 );
			float2 appendResult355 = (float2(_TopNormalIntensity , _TopNormalIntensity));
			float3 appendResult356 = (float3(appendResult355 , 1.0));
			float3 NormalMap166 = tex2DNode96;
			float3 newWorldNormal93 = WorldNormalVector( i , NormalMap166 );
			float clampResult112 = clamp( newWorldNormal93.y , 0.0 , 1.0 );
			float TopMask168 = saturate( ( pow( ( clampResult112 + _TopOffset ) , (1.0 + (_TopContrast - 0.0) * (20.0 - 1.0) / (1.0 - 0.0)) ) * _TopIntensity ) );
			float3 lerpResult158 = lerp( BlendNormals( UnpackScaleNormal( tex2D( _DetailNormalMap, DetailUVScale191 ) ,_DetailNormalMapIntensity ) , tex2DNode96 ) , BlendNormals( UnpackScaleNormal( tex2D( _BaseNormalMap, uv_BaseNormalMap ) ,0.5 ) , ( tanTriplanarNormal352 * appendResult356 ) ) , TopMask168);
			float3 normalizeResult136 = normalize( lerpResult158 );
			o.Normal = normalizeResult136;
			float2 uv_BaseAlbedoACutoutMask = i.uv_texcoord * _BaseAlbedoACutoutMask_ST.xy + _BaseAlbedoACutoutMask_ST.zw;
			float4 tex2DNode162 = tex2D( _BaseAlbedoACutoutMask, uv_BaseAlbedoACutoutMask );
			float4 temp_output_163_0 = ( _BaseAlbedoColor * tex2DNode162 );
			float4 blendOpSrc178 = ( tex2D( _DetailAlbedo, temp_output_182_0 ) * 2.0 );
			float4 blendOpDest178 = temp_output_163_0;
			float4 lerpResult187 = lerp( temp_output_163_0 , ( saturate( (( blendOpDest178 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpDest178 - 0.5 ) ) * ( 1.0 - blendOpSrc178 ) ) : ( 2.0 * blendOpDest178 * blendOpSrc178 ) ) )) , _DetailAlbedoIntensity);
			float4 triplanar353 = TriplanarSamplingSF( _TopAlbedoASmoothness, ase_worldPos, ase_worldNormal, 1.0, _TopUVScale, 0 );
			float4 lerpResult334 = lerp( lerpResult187 , ( _TopColor * triplanar353 ) , TopMask168);
			float4 switchResult359 = (((i.ASEVFace>0)?(lerpResult334):(( lerpResult334 * _BaseBackSideMultiply ))));
			o.Albedo = switchResult359.rgb;
			float2 uv_BaseMetallicASmoothness = i.uv_texcoord * _BaseMetallicASmoothness_ST.xy + _BaseMetallicASmoothness_ST.zw;
			float4 tex2DNode307 = tex2D( _BaseMetallicASmoothness, uv_BaseMetallicASmoothness );
			float4 temp_cast_2 = (0.0).xxxx;
			float4 lerpResult319 = lerp( ( tex2DNode307 * _BaseMetallic ) , temp_cast_2 , TopMask168);
			float4 Metalic340 = lerpResult319;
			o.Metallic = Metalic340.r;
			float MetalicAlphaSmoothness309 = tex2DNode307.a;
			float TopSmoothness220 = triplanar353.w;
			float lerpResult217 = lerp( ( MetalicAlphaSmoothness309 + (-1.0 + (_BaseSmoothness - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) ) , ( TopSmoothness220 + (-1.0 + (_TopSmoothness - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) ) , TopMask168);
			float Smoothness223 = lerpResult217;
			o.Smoothness = Smoothness223;
			float4 temp_cast_4 = (1.0).xxxx;
			float2 uv_BaseAO = i.uv_texcoord * _BaseAO_ST.xy + _BaseAO_ST.zw;
			float4 lerpResult201 = lerp( temp_cast_4 , tex2D( _BaseAO, uv_BaseAO ) , _BaseAOIntensity);
			float4 AOTexture207 = lerpResult201;
			float4 temp_cast_5 = (1.0).xxxx;
			float4 lerpResult303 = lerp( AOTexture207 , temp_cast_5 , 0.8);
			float4 lerpResult290 = lerp( AOTexture207 , lerpResult303 , TopMask168);
			float4 AmbientOcclusion349 = lerpResult290;
			o.Occlusion = AmbientOcclusion349.r;
			o.Alpha = 1;
			float AlbedoAlpha212 = tex2DNode162.a;
			clip( AlbedoAlpha212 - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			# include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 tSpace0 : TEXCOORD1;
				float4 tSpace1 : TEXCOORD2;
				float4 tSpace2 : TEXCOORD3;
				float4 texcoords01 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.texcoords01 = float4( v.texcoord.xy, v.texcoord1.xy );
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord.xy = IN.texcoords01.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=13801
1929;30;1901;1002;5534.703;-1341.013;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;314;-2688,512;Fixed;False;Property;_BaseNormalMapIntensity;Base Normal Map Intensity;2;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.TexturePropertyNode;287;-2640,608;Float;True;Property;_BaseNormalMap;Base Normal Map;9;1;[Normal];None;True;bump;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.SamplerNode;96;-2176,512;Float;True;Property;_Test;Test;4;0;None;True;0;True;bump;Auto;True;Instance;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;167;-5168,384;Float;False;166;0;1;FLOAT3
Node;AmplifyShaderEditor.RegisterLocalVarNode;166;-1792,576;Float;False;NormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.WorldNormalVector;93;-4864,384;Float;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;181;-2688,-1536;Half;False;Property;_DetailUVScale;Detail UV Scale;20;0;5;0;40;0;1;FLOAT
Node;AmplifyShaderEditor.TextureCoordinatesNode;183;-2640,-1728;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;103;-4608,576;Fixed;False;Property;_TopOffset;Top Offset;14;0;0.5;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;112;-4480,384;Float;False;3;0;FLOAT;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;107;-4608,704;Fixed;False;Property;_TopContrast;Top Contrast;15;0;1;0;2;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;182;-2304,-1664;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0.0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.SimpleAddOpNode;111;-4224,384;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemapNode;114;-4096,640;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;1.0;False;4;FLOAT;20.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;176;-2048,-1792;Float;True;Property;_DetailAlbedo;Detail Albedo;23;1;[NoScaleOffset];None;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;162;-2048,-2048;Float;True;Property;_BaseAlbedoACutoutMask;Base Albedo (A CutoutMask);7;0;None;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;165;-1968,-2320;Fixed;False;Property;_BaseAlbedoColor;Base Albedo Color;5;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;246;-4096,832;Fixed;False;Property;_TopIntensity;Top Intensity;13;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;139;-2688,1280;Fixed;False;Property;_TopNormalIntensity;Top Normal Intensity;16;0;1;0;2;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;196;-2560,-512;Half;False;Property;_TopUVScale;Top UV Scale;12;0;1;0.01;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;268;-1664,-1680;Fixed;False;Constant;_Multiplyx2;Multiplyx2;20;0;2;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.PowerNode;102;-3968,384;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;267;-1408,-1792;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;163;-1664,-2176;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;245;-3712,384;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.TexturePropertyNode;351;-2640,976;Float;True;Property;_TopNormalMap;Top Normal Map;19;2;[Normal];[NoScaleOffset];None;True;bump;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.RangedFloatNode;358;-2176,1408;Float;False;Constant;_Blue;Blue;24;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.TexturePropertyNode;354;-2528,-768;Float;True;Property;_TopAlbedoASmoothness;Top Albedo (A Smoothness);18;1;[NoScaleOffset];None;False;gray;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.GetLocalVarNode;198;-2608,1184;Float;False;197;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;197;-2048,-512;Float;False;TopUVScale;-1;True;1;0;FLOAT;0,0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;203;-5248,-1792;Fixed;False;Property;_BaseAOIntensity;Base AO Intensity;3;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;231;-5120,-2176;Fixed;False;Constant;_White1;White1;19;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;355;-2176,1280;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.SamplerNode;315;-5264,-2048;Float;True;Property;_BaseAO;Base AO;10;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.BlendOpsNode;178;-1280,-2032;Float;False;Overlay;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.TriplanarNode;352;-2176,1024;Float;True;Spherical;World;True;TriplanarNormal;_TriplanarNormal;bump;0;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Normal;False;8;0;SAMPLER2D;;False;5;FLOAT;1.0;False;1;SAMPLER2D;;False;6;FLOAT;0.0;False;2;SAMPLER2D;;False;7;FLOAT;0.0;False;3;FLOAT;0.0;False;4;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.TriplanarNode;353;-2048,-768;Float;True;Spherical;World;False;TriplanarAlbedo;_TriplanarAlbedo;gray;17;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Albedo;False;8;0;SAMPLER2D;;False;5;FLOAT;1.0;False;1;SAMPLER2D;;False;6;FLOAT;0.0;False;2;SAMPLER2D;;False;7;FLOAT;0.0;False;3;FLOAT;0.0;False;4;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SaturateNode;143;-3456,384;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;180;-1408,-1664;Fixed;False;Property;_DetailAlbedoIntensity;Detail Albedo Intensity;21;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;175;-2048,-1040;Fixed;False;Property;_TopColor;Top Color;17;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;189;-2688,384;Fixed;False;Property;_DetailNormalMapIntensity;Detail Normal Map Intensity;22;0;1;0;2;0;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;356;-1920,1280;Float;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.LerpOp;201;-4736,-2048;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;193;-2624,256;Float;False;191;0;1;FLOAT2
Node;AmplifyShaderEditor.RangedFloatNode;289;-2576,832;Fixed;False;Constant;_TopInt;Top Int;22;0;0.5;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;191;-2048,-1536;Float;False;DetailUVScale;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.RangedFloatNode;216;-5248,-704;Fixed;False;Property;_BaseSmoothness;Base Smoothness;1;0;0.5;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;218;-5248,-448;Fixed;False;Property;_TopSmoothness;Top Smoothness;11;0;0.5;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;307;-5120,1792;Float;True;Property;_BaseMetallicASmoothness;Base Metallic (A Smoothness);8;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;188;-2176,256;Float;True;Property;_DetailNormalMap;Detail Normal Map;24;2;[Normal];[NoScaleOffset];None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;173;-1282.119,-806.6863;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.LerpOp;187;-1024,-2176;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;310;-5248,-832;Float;False;309;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;220;-1664,-640;Float;False;TopSmoothness;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;168;-3216,384;Float;False;TopMask;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;338;-5120,2048;Float;False;Property;_BaseMetallic;Base Metallic;4;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;306;-2560,1968;Fixed;False;Constant;_White;White;22;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;170;-1312,-640;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemapNode;347;-4864,-720;Float;False;5;0;FLOAT;-1.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;-1.0;False;4;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;208;-2560,1776;Float;False;207;0;1;COLOR
Node;AmplifyShaderEditor.TFHCRemapNode;344;-4864,-512;Float;False;5;0;FLOAT;-1.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;-1.0;False;4;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;207;-4352,-2048;Float;False;AOTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;304;-2560,2096;Fixed;False;Constant;_AOTopIntensity;AO Top Intensity;22;0;0.8;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;221;-5200,-576;Float;False;220;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;357;-1792,1072;Float;False;2;2;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.RegisterLocalVarNode;309;-4608,1792;Float;False;MetalicAlphaSmoothness;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;288;-2176,768;Float;True;Property;_TextureSample2;Texture Sample 2;4;0;None;True;0;True;bump;Auto;True;Instance;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;361;-928,-544;Float;False;Property;_BaseBackSideMultiply;Base Back Side Multiply;6;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;320;-4608,2176;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;334;-768,-832;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;291;-2176,2032;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;321;-4608,2048;Float;False;Constant;_NonMetalic;NonMetalic;23;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;337;-4608,1920;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;171;-1568,1024;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;222;-4608,-448;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.BlendNormalsNode;190;-1584,384;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.BlendNormalsNode;252;-1584,768;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleAddOpNode;345;-4608,-576;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;303;-2176,1872;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;346;-4608,-768;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;158;-1280,768;Float;False;3;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0.0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.LerpOp;319;-4224,1920;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;360;-590,-560;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.LerpOp;217;-4352,-704;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;290;-1920,1776;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;350;-640,1120;Float;False;349;0;1;COLOR
Node;AmplifyShaderEditor.RegisterLocalVarNode;223;-3968,-688;Float;False;Smoothness;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;224;-640,1024;Float;False;223;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;349;-1664,1792;Float;False;AmbientOcclusion;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RegisterLocalVarNode;340;-3968,1920;Float;False;Metalic;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RegisterLocalVarNode;212;-1648,-1952;Float;False;AlbedoAlpha;-1;True;1;0;FLOAT;0,0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;343;-640,1216;Float;False;212;0;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;341;-640,896;Float;False;340;0;1;COLOR
Node;AmplifyShaderEditor.NormalizeNode;136;-1024,768;Float;False;1;0;FLOAT3;0,0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.SwitchByFaceNode;359;-448,-832;Float;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;112,752;Float;False;True;2;Float;;0;0;Standard;ANGRYMESH/PBR TopProjection Standard Cutout;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;Off;0;0;False;0;0;Custom;0.5;True;True;0;True;TransparentCutout;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;8.6;10;25;False;0.5;True;0;SrcAlpha;OneMinusSrcAlpha;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;241;-5248.225,102.8944;Float;False;100;100;;0;// Top World Mapping;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;339;-5232,1664;Float;False;100;100;;0;// Metallic;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;230;-5248,-1120;Float;False;100;100;;0;// Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;240;-2688,64;Float;False;100;100;;0;// Blend Normal Maps;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;210;-5248,-2304;Float;False;100;100;;0;// Ambient Occlussion;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;342;-2688,1664;Float;False;100;100;;0;// Decreased AO intensity for the Top Mapping;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;239;-2688,-1120;Float;False;100;100;;0;// Top Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;237;-2672,-2304;Float;False;100;100;;0;// Blend Albedo Detail;1,1,1,1;0;0
WireConnection;96;0;287;0
WireConnection;96;5;314;0
WireConnection;166;0;96;0
WireConnection;93;0;167;0
WireConnection;112;0;93;2
WireConnection;182;0;183;0
WireConnection;182;1;181;0
WireConnection;111;0;112;0
WireConnection;111;1;103;0
WireConnection;114;0;107;0
WireConnection;176;1;182;0
WireConnection;102;0;111;0
WireConnection;102;1;114;0
WireConnection;267;0;176;0
WireConnection;267;1;268;0
WireConnection;163;0;165;0
WireConnection;163;1;162;0
WireConnection;245;0;102;0
WireConnection;245;1;246;0
WireConnection;197;0;196;0
WireConnection;355;0;139;0
WireConnection;355;1;139;0
WireConnection;178;0;267;0
WireConnection;178;1;163;0
WireConnection;352;0;351;0
WireConnection;352;3;198;0
WireConnection;353;0;354;0
WireConnection;353;3;196;0
WireConnection;143;0;245;0
WireConnection;356;0;355;0
WireConnection;356;2;358;0
WireConnection;201;0;231;0
WireConnection;201;1;315;0
WireConnection;201;2;203;0
WireConnection;191;0;182;0
WireConnection;188;1;193;0
WireConnection;188;5;189;0
WireConnection;173;0;175;0
WireConnection;173;1;353;0
WireConnection;187;0;163;0
WireConnection;187;1;178;0
WireConnection;187;2;180;0
WireConnection;220;0;353;4
WireConnection;168;0;143;0
WireConnection;347;0;216;0
WireConnection;344;0;218;0
WireConnection;207;0;201;0
WireConnection;357;0;352;0
WireConnection;357;1;356;0
WireConnection;309;0;307;4
WireConnection;288;0;287;0
WireConnection;288;5;289;0
WireConnection;334;0;187;0
WireConnection;334;1;173;0
WireConnection;334;2;170;0
WireConnection;337;0;307;0
WireConnection;337;1;338;0
WireConnection;190;0;188;0
WireConnection;190;1;96;0
WireConnection;252;0;288;0
WireConnection;252;1;357;0
WireConnection;345;0;221;0
WireConnection;345;1;344;0
WireConnection;303;0;208;0
WireConnection;303;1;306;0
WireConnection;303;2;304;0
WireConnection;346;0;310;0
WireConnection;346;1;347;0
WireConnection;158;0;190;0
WireConnection;158;1;252;0
WireConnection;158;2;171;0
WireConnection;319;0;337;0
WireConnection;319;1;321;0
WireConnection;319;2;320;0
WireConnection;360;0;334;0
WireConnection;360;1;361;0
WireConnection;217;0;346;0
WireConnection;217;1;345;0
WireConnection;217;2;222;0
WireConnection;290;0;208;0
WireConnection;290;1;303;0
WireConnection;290;2;291;0
WireConnection;223;0;217;0
WireConnection;349;0;290;0
WireConnection;340;0;319;0
WireConnection;212;0;162;4
WireConnection;136;0;158;0
WireConnection;359;0;334;0
WireConnection;359;1;360;0
WireConnection;0;0;359;0
WireConnection;0;1;136;0
WireConnection;0;3;341;0
WireConnection;0;4;224;0
WireConnection;0;5;350;0
WireConnection;0;10;343;0
ASEEND*/
//CHKSM=687FF56924CC0893356E0ED2E914B052FF44494E