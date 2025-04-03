// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ANGRYMESH/PBR TopProjection Specular"
{
	Properties
	{
		[Toggle]_BaseSmoothnessUseAlbedoAlpha("Base Smoothness Use Albedo Alpha", Int) = 0
		_BaseSmoothness("Base Smoothness", Range( 0 , 1)) = 0.5
		_BaseNormalMapIntensity("Base Normal Map Intensity", Range( 0 , 1)) = 1
		_BaseAOIntensity("Base AO Intensity", Range( 0 , 1)) = 1
		_BaseAlbedoColor("Base Albedo Color", Color) = (1,1,1,0)
		_BaseAlbedo("Base Albedo", 2D) = "gray" {}
		_BaseSpecularASmoothness("Base Specular (A Smoothness)", 2D) = "black" {}
		[Normal]_BaseNormalMap("Base Normal Map", 2D) = "bump" {}
		_BaseAO("Base AO", 2D) = "white" {}
		_TopSmoothness("Top Smoothness", Range( 0 , 1)) = 0.5
		_TopUVScale("Top UV Scale", Range( 0.1 , 10)) = 1
		_TopIntensity("Top Intensity", Range( 0 , 1)) = 1
		_TopOffset("Top Offset", Range( 0 , 1)) = 0.5
		_TopContrast("Top Contrast", Range( 0 , 2)) = 1
		_TopNormalIntensity("Top Normal Intensity", Range( 0 , 2)) = 2
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
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile __ _BASESMOOTHNESSUSEALBEDOALPHA_ON
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
		uniform sampler2D _BaseAlbedo;
		uniform float4 _BaseAlbedo_ST;
		uniform sampler2D _DetailAlbedo;
		uniform fixed _DetailAlbedoIntensity;
		uniform fixed4 _TopColor;
		uniform sampler2D _TopAlbedoASmoothness;
		uniform sampler2D _BaseSpecularASmoothness;
		uniform float4 _BaseSpecularASmoothness_ST;
		uniform fixed _BaseSmoothness;
		uniform fixed _TopSmoothness;
		uniform sampler2D _BaseAO;
		uniform float4 _BaseAO_ST;
		uniform fixed _BaseAOIntensity;


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


		void surf( Input i , inout SurfaceOutputStandardSpecular o )
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
			float3 triplanar361 = TriplanarSamplingSNF( _TopNormalMap, ase_worldPos, ase_worldNormal, 1.0, TopUVScale197, 0 );
			float3 tanTriplanarNormal361 = mul( ase_worldToTangent, triplanar361 );
			float2 appendResult365 = (float2(_TopNormalIntensity , _TopNormalIntensity));
			float3 appendResult366 = (float3(appendResult365 , 1.0));
			float3 NormalMap166 = tex2DNode96;
			float3 newWorldNormal93 = WorldNormalVector( i , NormalMap166 );
			float clampResult112 = clamp( newWorldNormal93.y , 0.0 , 1.0 );
			float TopMask168 = saturate( ( pow( ( clampResult112 + _TopOffset ) , (1.0 + (_TopContrast - 0.0) * (20.0 - 1.0) / (1.0 - 0.0)) ) * _TopIntensity ) );
			float3 lerpResult158 = lerp( BlendNormals( UnpackScaleNormal( tex2D( _DetailNormalMap, DetailUVScale191 ) ,_DetailNormalMapIntensity ) , tex2DNode96 ) , BlendNormals( UnpackScaleNormal( tex2D( _BaseNormalMap, uv_BaseNormalMap ) ,0.5 ) , ( tanTriplanarNormal361 * appendResult366 ) ) , TopMask168);
			float3 normalizeResult136 = normalize( lerpResult158 );
			o.Normal = normalizeResult136;
			float2 uv_BaseAlbedo = i.uv_texcoord * _BaseAlbedo_ST.xy + _BaseAlbedo_ST.zw;
			float4 tex2DNode162 = tex2D( _BaseAlbedo, uv_BaseAlbedo );
			float4 temp_output_163_0 = ( _BaseAlbedoColor * tex2DNode162 );
			float4 blendOpSrc178 = ( tex2D( _DetailAlbedo, temp_output_182_0 ) * 2.0 );
			float4 blendOpDest178 = temp_output_163_0;
			float4 lerpResult187 = lerp( temp_output_163_0 , ( saturate( (( blendOpDest178 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpDest178 - 0.5 ) ) * ( 1.0 - blendOpSrc178 ) ) : ( 2.0 * blendOpDest178 * blendOpSrc178 ) ) )) , _DetailAlbedoIntensity);
			float4 triplanar363 = TriplanarSamplingSF( _TopAlbedoASmoothness, ase_worldPos, ase_worldNormal, 1.0, _TopUVScale, 0 );
			float4 lerpResult334 = lerp( lerpResult187 , ( _TopColor * triplanar363 ) , TopMask168);
			o.Albedo = lerpResult334.rgb;
			float2 uv_BaseSpecularASmoothness = i.uv_texcoord * _BaseSpecularASmoothness_ST.xy + _BaseSpecularASmoothness_ST.zw;
			float4 tex2DNode307 = tex2D( _BaseSpecularASmoothness, uv_BaseSpecularASmoothness );
			float TopAlphaSmoothness220 = triplanar363.w;
			float4 temp_cast_2 = (TopAlphaSmoothness220).xxxx;
			float4 lerpResult319 = lerp( tex2DNode307 , temp_cast_2 , TopMask168);
			float4 Specular340 = lerpResult319;
			o.Specular = Specular340.rgb;
			float BaseAlphaSmoothness212 = tex2DNode162.a;
			float SpecularAlphaSmoothness309 = tex2DNode307.a;
			#ifdef _BASESMOOTHNESSUSEALBEDOALPHA_ON
				float staticSwitch312 = BaseAlphaSmoothness212;
			#else
				float staticSwitch312 = SpecularAlphaSmoothness309;
			#endif
			float lerpResult217 = lerp( ( staticSwitch312 + (-1.0 + (_BaseSmoothness - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) ) , ( TopAlphaSmoothness220 + (-1.0 + (_TopSmoothness - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) ) , TopMask168);
			float Smoothness223 = lerpResult217;
			o.Smoothness = Smoothness223;
			float4 temp_cast_4 = (1.0).xxxx;
			float2 uv_BaseAO = i.uv_texcoord * _BaseAO_ST.xy + _BaseAO_ST.zw;
			float4 lerpResult201 = lerp( temp_cast_4 , tex2D( _BaseAO, uv_BaseAO ) , _BaseAOIntensity);
			float4 AOTexture207 = lerpResult201;
			float4 temp_cast_5 = (1.0).xxxx;
			float4 lerpResult303 = lerp( AOTexture207 , temp_cast_5 , 0.8);
			float4 lerpResult290 = lerp( AOTexture207 , lerpResult303 , TopMask168);
			float4 AmbientOcclusion356 = lerpResult290;
			o.Occlusion = AmbientOcclusion356.r;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows 

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
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
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
1929;30;1901;1002;7767.808;3542.472;6.921046;True;True
Node;AmplifyShaderEditor.TexturePropertyNode;287;-2640,608;Float;True;Property;_BaseNormalMap;Base Normal Map;7;1;[Normal];None;True;bump;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.RangedFloatNode;314;-2688,512;Fixed;False;Property;_BaseNormalMapIntensity;Base Normal Map Intensity;2;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;96;-2176,512;Float;True;Property;_Test;Test;4;0;None;True;0;True;bump;Auto;True;Instance;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;167;-5168,393.9471;Float;False;166;0;1;FLOAT3
Node;AmplifyShaderEditor.RegisterLocalVarNode;166;-1792,576;Float;False;NormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.WorldNormalVector;93;-4864,384;Float;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;112;-4480,384;Float;False;3;0;FLOAT;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;103;-4608,576;Fixed;False;Property;_TopOffset;Top Offset;12;0;0.5;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;107;-4608,704;Fixed;False;Property;_TopContrast;Top Contrast;13;0;1;0;2;0;1;FLOAT
Node;AmplifyShaderEditor.TextureCoordinatesNode;183;-2640,-1728;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;111;-4096,384;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;196;-2560,-608;Half;False;Property;_TopUVScale;Top UV Scale;10;0;1;0.1;10;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;181;-2688,-1536;Half;False;Property;_DetailUVScale;Detail UV Scale;18;0;5;0;40;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;139;-2672,1296;Fixed;False;Property;_TopNormalIntensity;Top Normal Intensity;14;0;2;0;2;0;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemapNode;114;-4096,640;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;1.0;False;4;FLOAT;20.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;367;-2304,1408;Float;False;Constant;_Blue;Blue;23;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;203;-5248,-1792;Fixed;False;Property;_BaseAOIntensity;Base AO Intensity;3;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.PowerNode;102;-3840,384;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;231;-5120,-2176;Fixed;False;Constant;_White1;White1;19;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.TexturePropertyNode;364;-2512,-832;Float;True;Property;_TopAlbedoASmoothness;Top Albedo (A Smoothness);16;1;[NoScaleOffset];Assets/ANGRY MESH/Top Projection Shader/Sources/Textures/Snow_01_Albedo.tga;False;gray;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.TexturePropertyNode;362;-2640,976;Float;True;Property;_TopNormalMap;Top Normal Map;17;2;[Normal];[NoScaleOffset];Assets/ANGRY MESH/Top Projection Shader/Sources/Textures/Snow_01_NM.tga;True;bump;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.SamplerNode;315;-5264,-2048;Float;True;Property;_BaseAO;Base AO;8;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;198;-2608,1184;Float;False;197;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;246;-3856,640;Fixed;False;Property;_TopIntensity;Top Intensity;11;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;365;-2304,1280;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.RegisterLocalVarNode;197;-2128,-512;Float;False;TopUVScale;-1;True;1;0;FLOAT;0,0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;182;-2304,-1664;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0.0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.SamplerNode;307;-5120,1792;Float;True;Property;_BaseSpecularASmoothness;Base Specular (A Smoothness);6;0;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;162;-2048,-2048;Float;True;Property;_BaseAlbedo;Base Albedo;5;0;None;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;366;-1920,1280;Float;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.RangedFloatNode;189;-2688,384;Fixed;False;Property;_DetailNormalMapIntensity;Detail Normal Map Intensity;20;0;1;0;2;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;176;-2048,-1792;Float;True;Property;_DetailAlbedo;Detail Albedo;21;1;[NoScaleOffset];None;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;193;-2624,256;Float;False;191;0;1;FLOAT2
Node;AmplifyShaderEditor.TriplanarNode;361;-2176,1024;Float;True;Spherical;World;True;TriplanarNormal;_TriplanarNormal;bump;0;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Normal;False;8;0;SAMPLER2D;;False;5;FLOAT;1.0;False;1;SAMPLER2D;;False;6;FLOAT;0.0;False;2;SAMPLER2D;;False;7;FLOAT;0.0;False;3;FLOAT;0.0;False;4;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.TriplanarNode;363;-2128,-768;Float;True;Spherical;World;False;Top Texture 1;_TopTexture1;gray;0;None;Mid Texture 1;_MidTexture1;white;-1;None;Bot Texture 1;_BotTexture1;white;-1;None;Top Albedo (A Smoothness);False;8;0;SAMPLER2D;;False;5;FLOAT;1.0;False;1;SAMPLER2D;;False;6;FLOAT;0.0;False;2;SAMPLER2D;;False;7;FLOAT;0.0;False;3;FLOAT;0.0;False;4;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;268;-1664,-1680;Fixed;False;Constant;_Multiplyx2;Multiplyx2;20;0;2;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;216;-4992,-704;Fixed;False;Property;_BaseSmoothness;Base Smoothness;1;0;0.5;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;289;-2576,832;Fixed;False;Constant;_TopInt;Top Int;22;0;0.5;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;245;-3584,384;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0;False;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;201;-4736,-2048;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RegisterLocalVarNode;191;-2048,-1536;Float;False;DetailUVScale;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.ColorNode;165;-1968,-2320;Fixed;False;Property;_BaseAlbedoColor;Base Albedo Color;4;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;310;-5248,-816;Float;False;309;0;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;311;-5248,-896;Float;False;212;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;309;-4608,1792;Float;False;SpecularAlphaSmoothness;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;212;-1648,-1952;Float;False;BaseAlphaSmoothness;-1;True;1;0;FLOAT;0,0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;218;-5008,-512;Fixed;False;Property;_TopSmoothness;Top Smoothness;9;0;0.5;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;267;-1408,-1792;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.TFHCRemapNode;354;-4608,-768;Float;False;5;0;FLOAT;-1.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;-1.0;False;4;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;220;-1664,-640;Float;False;TopAlphaSmoothness;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;208;-2560,1776;Float;False;207;0;1;COLOR
Node;AmplifyShaderEditor.SamplerNode;288;-2176,768;Float;True;Property;_TextureSample2;Texture Sample 2;4;0;None;True;0;True;bump;Auto;True;Instance;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;221;-5003.007,-590.5001;Float;False;220;0;1;FLOAT
Node;AmplifyShaderEditor.StaticSwitch;312;-4928,-896;Float;False;Property;_BaseSmoothnessUseAlbedoAlpha;Base Smoothness Use Albedo Alpha;0;0;1;False;True;;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemapNode;349;-4608,-512;Float;False;5;0;FLOAT;-1.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;-1.0;False;4;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.SaturateNode;143;-3376,384;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;207;-4352,-2048;Float;False;AOTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;306;-2560,1968;Fixed;False;Constant;_White;White;22;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;163;-1664,-2176;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;304;-2560,2096;Fixed;False;Constant;_AOTopIntensity;AO Top Intensity;22;0;0.8;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;368;-1744,1024;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.SamplerNode;188;-2176,256;Float;True;Property;_DetailNormalMap;Detail Normal Map;22;2;[Normal];[NoScaleOffset];None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;359;-4643.862,1978.698;Float;False;220;0;1;FLOAT
Node;AmplifyShaderEditor.BlendNormalsNode;252;-1584,768;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.ColorNode;175;-2048,-1040;Fixed;False;Property;_TopColor;Top Color;15;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.LerpOp;303;-2176,1872;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;171;-1456,1024;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;180;-1408,-1664;Fixed;False;Property;_DetailAlbedoIntensity;Detail Albedo Intensity;19;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.BlendNormalsNode;190;-1584,384;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.GetLocalVarNode;291;-2176,2032;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;168;-3152,384;Float;False;TopMask;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;348;-4352,-896;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;320;-4608,2176;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;355;-4352,-592;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.BlendOpsNode;178;-1280,-2032;Float;False;Overlay;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;222;-4352,-448;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;158;-1280,768;Float;False;3;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0.0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.GetLocalVarNode;170;-1312,-640;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;217;-4096,-640;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;290;-1920,1776;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;173;-1282.119,-806.6863;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.LerpOp;187;-1024,-2176;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.LerpOp;319;-4224,1920;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;224;-640,1024;Float;False;223;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;356;-1664,1792;Float;False;AmbientOcclusion;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.LerpOp;334;-768,-832;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RegisterLocalVarNode;223;-3840,-640;Float;False;Smoothness;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;341;-640,896;Float;False;340;0;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;357;-640,1152;Float;False;356;0;1;COLOR
Node;AmplifyShaderEditor.NormalizeNode;136;-1024,768;Float;False;1;0;FLOAT3;0,0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.RegisterLocalVarNode;340;-3968,1920;Float;False;Specular;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;112,752;Float;False;True;2;Float;;0;0;StandardSpecular;ANGRYMESH/PBR TopProjection Specular;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;-0.09;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;8.6;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;237;-2672,-2304;Float;False;100;100;;0;// Blend Albedo Detail;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;241;-5248.225,102.8944;Float;False;100;100;;0;// Top World Mapping;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;210;-5248,-2304;Float;False;100;100;;0;// Ambient Occlussion;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;342;-2688,1664;Float;False;100;100;;0;// Decreased AO intensity for the Top Mapping;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;239;-2688,-1120;Float;False;100;100;;0;// Top Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;240;-2688,64;Float;False;100;100;;0;// Blend Normal Maps;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;339;-5232,1664;Float;False;100;100;;0;// Specular;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;230;-5248,-1120;Float;False;100;100;;0;// Smoothness;1,1,1,1;0;0
WireConnection;96;0;287;0
WireConnection;96;5;314;0
WireConnection;166;0;96;0
WireConnection;93;0;167;0
WireConnection;112;0;93;2
WireConnection;111;0;112;0
WireConnection;111;1;103;0
WireConnection;114;0;107;0
WireConnection;102;0;111;0
WireConnection;102;1;114;0
WireConnection;365;0;139;0
WireConnection;365;1;139;0
WireConnection;197;0;196;0
WireConnection;182;0;183;0
WireConnection;182;1;181;0
WireConnection;366;0;365;0
WireConnection;366;2;367;0
WireConnection;176;1;182;0
WireConnection;361;0;362;0
WireConnection;361;3;198;0
WireConnection;363;0;364;0
WireConnection;363;3;196;0
WireConnection;245;0;102;0
WireConnection;245;1;246;0
WireConnection;201;0;231;0
WireConnection;201;1;315;0
WireConnection;201;2;203;0
WireConnection;191;0;182;0
WireConnection;309;0;307;4
WireConnection;212;0;162;4
WireConnection;267;0;176;0
WireConnection;267;1;268;0
WireConnection;354;0;216;0
WireConnection;220;0;363;4
WireConnection;288;0;287;0
WireConnection;288;5;289;0
WireConnection;312;0;311;0
WireConnection;312;1;310;0
WireConnection;349;0;218;0
WireConnection;143;0;245;0
WireConnection;207;0;201;0
WireConnection;163;0;165;0
WireConnection;163;1;162;0
WireConnection;368;0;361;0
WireConnection;368;1;366;0
WireConnection;188;1;193;0
WireConnection;188;5;189;0
WireConnection;252;0;288;0
WireConnection;252;1;368;0
WireConnection;303;0;208;0
WireConnection;303;1;306;0
WireConnection;303;2;304;0
WireConnection;190;0;188;0
WireConnection;190;1;96;0
WireConnection;168;0;143;0
WireConnection;348;0;312;0
WireConnection;348;1;354;0
WireConnection;355;0;221;0
WireConnection;355;1;349;0
WireConnection;178;0;267;0
WireConnection;178;1;163;0
WireConnection;158;0;190;0
WireConnection;158;1;252;0
WireConnection;158;2;171;0
WireConnection;217;0;348;0
WireConnection;217;1;355;0
WireConnection;217;2;222;0
WireConnection;290;0;208;0
WireConnection;290;1;303;0
WireConnection;290;2;291;0
WireConnection;173;0;175;0
WireConnection;173;1;363;0
WireConnection;187;0;163;0
WireConnection;187;1;178;0
WireConnection;187;2;180;0
WireConnection;319;0;307;0
WireConnection;319;1;359;0
WireConnection;319;2;320;0
WireConnection;356;0;290;0
WireConnection;334;0;187;0
WireConnection;334;1;173;0
WireConnection;334;2;170;0
WireConnection;223;0;217;0
WireConnection;136;0;158;0
WireConnection;340;0;319;0
WireConnection;0;0;334;0
WireConnection;0;1;136;0
WireConnection;0;3;341;0
WireConnection;0;4;224;0
WireConnection;0;5;357;0
ASEEND*/
//CHKSM=027F6D4F605365608D1EE7A4463CF6BBB276B15A