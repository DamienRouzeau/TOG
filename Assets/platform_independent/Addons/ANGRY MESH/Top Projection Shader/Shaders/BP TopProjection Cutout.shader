// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ANGRYMESH/BP TopProjection Cutout"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = -0.09
		_BaseShininess("Base Shininess", Range( 0.01 , 1)) = 0.5
		_BaseNormalMapIntensity("Base Normal Map Intensity", Range( 0 , 1)) = 1
		_BaseColor("Base Color", Color) = (1,1,1,0)
		[NoScaleOffset]_BaseDiffuseACutoutMask("Base Diffuse (A CutoutMask)", 2D) = "gray" {}
		[NoScaleOffset][Normal]_BaseNormalMap("Base NormalMap", 2D) = "bump" {}
		_TopShininess("Top Shininess", Range( 0.01 , 1)) = 0.5
		_TopUVScale("Top UV Scale", Range( 0.01 , 1)) = 0.1
		_TopIntensity("Top Intensity", Range( 0 , 1)) = 1
		_TopOffset("Top Offset", Range( 0 , 1)) = 0.5
		_TopContrast("Top Contrast", Range( 0 , 2)) = 0.5
		_TopNormalIntensity("Top Normal Intensity", Range( 0 , 2)) = 1
		_TopColor("Top Color", Color) = (1,1,1,0)
		[NoScaleOffset]_TopDiffuseAGloss("Top Diffuse (A Gloss)", 2D) = "gray" {}
		[NoScaleOffset][Normal]_TopNormalMap("Top Normal Map", 2D) = "bump" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 2.0
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
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _TopDiffuseAGloss;
		uniform half _TopUVScale;
		uniform fixed _BaseNormalMapIntensity;
		uniform sampler2D _BaseNormalMap;
		uniform fixed _TopOffset;
		uniform fixed _TopContrast;
		uniform fixed _TopIntensity;
		uniform sampler2D _TopNormalMap;
		uniform fixed _TopNormalIntensity;
		uniform fixed _BaseShininess;
		uniform fixed _TopShininess;
		uniform fixed4 _BaseColor;
		uniform sampler2D _BaseDiffuseACutoutMask;
		uniform fixed4 _TopColor;
		uniform float _Cutoff = -0.09;


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


		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#if DIRECTIONAL
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float4 triplanar505 = TriplanarSamplingSF( _TopDiffuseAGloss, ase_worldPos, ase_worldNormal, 1.0, _TopUVScale, 0 );
			float TopAlpha220 = triplanar505.w;
			float4 temp_cast_0 = (TopAlpha220).xxxx;
			float2 uv_BaseNormalMap = i.uv_texcoord;
			float3 tex2DNode291 = UnpackScaleNormal( tex2D( _BaseNormalMap, uv_BaseNormalMap ) ,_BaseNormalMapIntensity );
			float3 BaseNormalMap166 = tex2DNode291;
			float3 newWorldNormal93 = WorldNormalVector( i , BaseNormalMap166 );
			float TopMask168 = saturate( ( pow( ( saturate( newWorldNormal93.y ) + _TopOffset ) , (1.0 + (_TopContrast - 0.0) * (20.0 - 1.0) / (1.0 - 0.0)) ) * _TopIntensity ) );
			float4 lerpResult217 = lerp( float4(0.3921569,0.3921569,0.3921569,1) , temp_cast_0 , TopMask168);
			fixed4 Gloss223 = lerpResult217;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			float3 normalizeResult486 = normalize( ( ase_worldViewDir + ase_worldlightDir ) );
			float TopUVScale197 = _TopUVScale;
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 triplanar508 = TriplanarSamplingSNF( _TopNormalMap, ase_worldPos, ase_worldNormal, 1.0, TopUVScale197, 0 );
			float3 tanTriplanarNormal508 = mul( ase_worldToTangent, triplanar508 );
			float2 appendResult510 = (float2(_TopNormalIntensity , _TopNormalIntensity));
			float3 appendResult512 = (float3(appendResult510 , 1.0));
			float3 lerpResult158 = lerp( tex2DNode291 , ( tanTriplanarNormal508 * appendResult512 ) , TopMask168);
			float3 NormalMap387 = lerpResult158;
			float3 normalizeResult473 = normalize( WorldNormalVector( i , NormalMap387 ) );
			float dotResult465 = dot( normalizeResult486 , normalizeResult473 );
			float lerpResult299 = lerp( _BaseShininess , _TopShininess , TopMask168);
			fixed Specular301 = lerpResult299;
			float3 temp_output_470_0 = ( _LightColor0.rgb * ase_lightAtten );
			float4 LightingSpecular494 = ( Gloss223 * pow( max( dotResult465 , 0.0 ) , ( Specular301 * 128.0 ) ) * float4( temp_output_470_0 , 0.0 ) );
			float dotResult458 = dot( normalizeResult473 , ase_worldlightDir );
			UnityGI gi481 = gi;
			gi481 = UnityGI_Base( data, 1, WorldNormalVector( i , normalizeResult473 ) );
			float3 indirectDiffuse481 = gi481.indirect.diffuse;
			float2 uv_BaseDiffuseACutoutMask = i.uv_texcoord;
			float4 tex2DNode162 = tex2D( _BaseDiffuseACutoutMask, uv_BaseDiffuseACutoutMask );
			float4 lerpResult157 = lerp( ( _BaseColor * tex2DNode162 ) , ( _TopColor * triplanar505 ) , TopMask168);
			float4 Diffuse394 = lerpResult157;
			float4 LightingLambert497 = ( float4( ( ( temp_output_470_0 * max( dotResult458 , 0.0 ) ) + indirectDiffuse481 ) , 0.0 ) * Diffuse394 );
			float DiffuseAlpha212 = tex2DNode162.a;
			c.rgb = ( LightingSpecular494 + LightingLambert497 ).rgb;
			c.a = 1;
			clip( DiffuseAlpha212 - _Cutoff );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma exclude_renderers d3d9 
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows noambient novertexlights 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
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
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				LightingStandardCustomLighting( o, worldViewDir, gi );
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
1929;30;1901;1002;7943.424;3546.281;7.111089;True;True
Node;AmplifyShaderEditor.RangedFloatNode;516;-2688,400;Fixed;False;Property;_BaseNormalMapIntensity;Base Normal Map Intensity;2;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;291;-2176,384;Float;True;Property;_BaseNormalMap;Base NormalMap;5;2;[NoScaleOffset];[Normal];None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;166;-1792,576;Float;False;BaseNormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.GetLocalVarNode;167;-5248,384;Float;False;166;0;1;FLOAT3
Node;AmplifyShaderEditor.WorldNormalVector;93;-4896,384;Float;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;103;-4736,576;Fixed;False;Property;_TopOffset;Top Offset;9;0;0.5;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;107;-4736,704;Fixed;False;Property;_TopContrast;Top Contrast;10;0;0.5;0;2;0;1;FLOAT
Node;AmplifyShaderEditor.SaturateNode;390;-4560,384;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemapNode;114;-4368,640;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;1.0;False;4;FLOAT;20.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;111;-4304,384;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;139;-2672,944;Fixed;False;Property;_TopNormalIntensity;Top Normal Intensity;11;0;1;0;2;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;196;-2432,-769.2863;Half;False;Property;_TopUVScale;Top UV Scale;7;0;0.1;0.01;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;246;-4096,576;Fixed;False;Property;_TopIntensity;Top Intensity;8;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.PowerNode;102;-4048,384;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;511;-2288,1072;Float;False;Constant;_Float0;Float 0;24;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;510;-2304,944;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.RegisterLocalVarNode;197;-2048,-704;Float;False;TopUVScale;-1;True;1;0;FLOAT;0,0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;393;-3792,384;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;198;-2608,768;Float;False;197;0;1;FLOAT
Node;AmplifyShaderEditor.TexturePropertyNode;509;-2640,560;Float;True;Property;_TopNormalMap;Top Normal Map;14;2;[NoScaleOffset];[Normal];None;False;bump;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.DynamicAppendNode;512;-2048,944;Float;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.TriplanarNode;508;-2192,656;Float;True;Spherical;World;True;TriplanarNormal;_TriplanarNormal;bump;0;None;Mid Texture 2;_MidTexture2;white;-1;None;Bot Texture 2;_BotTexture2;white;-1;None;Triplanar Normal;False;8;0;SAMPLER2D;;False;5;FLOAT;1.0;False;1;SAMPLER2D;;False;6;FLOAT;0.0;False;2;SAMPLER2D;;False;7;FLOAT;0.0;False;3;FLOAT;0.0;False;4;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SaturateNode;143;-3584,384;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;168;-3424,384;Float;False;TopMask;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;171;-1648,784;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;513;-1792,688;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.LerpOp;158;-1280,528;Float;False;3;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0.0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.GetLocalVarNode;489;-5168,2560;Float;False;387;0;1;FLOAT3
Node;AmplifyShaderEditor.RegisterLocalVarNode;387;-976,528;Float;False;NormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;484;-5248,1792;Float;False;World;0;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.WorldNormalVector;475;-4896,2560;Float;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;483;-5248,1952;Float;False;1;0;FLOAT;0.0;False;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;290;-5248,-2432;Fixed;False;Property;_BaseShininess;Base Shininess;1;0;0.5;0.01;1;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;485;-4864,1920;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0.0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.TexturePropertyNode;507;-2432,-1024;Float;True;Property;_TopDiffuseAGloss;Top Diffuse (A Gloss);13;1;[NoScaleOffset];None;False;gray;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.RangedFloatNode;292;-5248,-2304;Fixed;False;Property;_TopShininess;Top Shininess;6;0;0.5;0.01;1;0;1;FLOAT
Node;AmplifyShaderEditor.NormalizeNode;473;-4608,2560;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;461;-4944,2752;Float;False;1;0;FLOAT;0.0;False;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;300;-5152,-2176;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;165;-1968,-1824;Fixed;False;Property;_BaseColor;Base Color;3;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.TriplanarNode;505;-2048,-1024;Float;True;Spherical;World;False;TriplanarAlbedo;_TriplanarAlbedo;gray;17;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Albedo;False;8;0;SAMPLER2D;;False;5;FLOAT;1.0;False;1;SAMPLER2D;;False;6;FLOAT;0.0;False;2;SAMPLER2D;;False;7;FLOAT;0.0;False;3;FLOAT;0.0;False;4;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;162;-2064,-1584;Float;True;Property;_BaseDiffuseACutoutMask;Base Diffuse (A CutoutMask);4;1;[NoScaleOffset];None;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;175;-1968,-1280;Fixed;False;Property;_TopColor;Top Color;12;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.LerpOp;299;-4864,-2352;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.NormalizeNode;486;-4608,1920;Float;False;1;0;FLOAT3;0,0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.LightColorNode;466;-3840,2176;Float;False;0;3;COLOR;FLOAT3;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;458;-4320,2560;Float;False;2;0;FLOAT3;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.LightAttenuation;469;-3840,2304;Float;False;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;220;-1429,-897;Float;False;TopAlpha;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;173;-1392,-1104;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;163;-1584,-1616;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;170;-1056,-896;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;301;-4496,-2352;Fixed;False;Specular;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMaxOpNode;471;-3584,2560;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;488;-4304,1728;Float;False;301;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;529;-4304,1840;Float;False;Constant;_128;128;20;0;128;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;465;-4320,1920;Float;False;2;0;FLOAT3;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;222;-5152,-688;Float;False;168;0;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;531;-5200,-992;Float;False;Constant;_BaseGlossColor;Base Gloss Color;1;0;0.3921569,0.3921569,0.3921569,1;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;470;-3584,2224;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.GetLocalVarNode;221;-5152,-816;Float;False;220;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;482;-3312,2224;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.IndirectDiffuseLighting;481;-4352,2752;Float;False;Tangent;1;0;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;468;-4080,1792;Float;False;2;2;0;FLOAT;0,0,0,0;False;1;FLOAT;128.0;False;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;157;-768,-1120;Float;False;3;0;COLOR;0.0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMaxOpNode;457;-4064,1920;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;217;-4864,-832;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.PowerNode;462;-3712,1872;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0,0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;463;-3072,2736;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0.0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.GetLocalVarNode;526;-3488,1728;Float;False;223;0;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;491;-2976,2944;Float;False;394;0;1;COLOR
Node;AmplifyShaderEditor.RegisterLocalVarNode;223;-4608,-832;Fixed;False;Gloss;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RegisterLocalVarNode;394;-496,-1120;Float;False;Diffuse;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;479;-3200,1792;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;2;FLOAT3;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;459;-2656,2736;Float;False;2;2;0;FLOAT3;0.0,0,0,0;False;1;COLOR;0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RegisterLocalVarNode;497;-2432,2736;Float;False;LightingLambert;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RegisterLocalVarNode;494;-2432,1792;Float;False;LightingSpecular;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;499;303,128;Float;False;494;0;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;500;303,256;Float;False;497;0;1;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;498;671,128;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;532;640,384;Float;False;212;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;212;-1664,-1344;Float;False;DiffuseAlpha;-1;True;1;0;FLOAT;0,0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;943,64;Float;False;True;0;Float;;0;0;CustomLighting;ANGRYMESH/BP TopProjection Cutout;False;False;False;False;True;True;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Custom;-0.09;True;True;0;True;TransparentCutout;Transparent;All;False;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;8.6;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;0;0;False;14;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;303;-5248,-2688;Float;False;100;100;;0;// Specular;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;503;-5248,1584;Float;False;100;100;;0;// Lighting Specular;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;241;-5248.225,102.8944;Float;False;100;100;;0;// Top World Mapping;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;237;-2576,-1904;Float;False;100;100;;0;// Blend Diffuse;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;240;-2688,64;Float;False;100;100;;0;// Blend Normal Maps;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;504;-5248,2432;Float;False;100;100;;0;// Lighting Lambert;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;230;-5248,-1152;Float;False;100;100;;0;// Gloss;1,1,1,1;0;0
WireConnection;291;5;516;0
WireConnection;166;0;291;0
WireConnection;93;0;167;0
WireConnection;390;0;93;2
WireConnection;114;0;107;0
WireConnection;111;0;390;0
WireConnection;111;1;103;0
WireConnection;102;0;111;0
WireConnection;102;1;114;0
WireConnection;510;0;139;0
WireConnection;510;1;139;0
WireConnection;197;0;196;0
WireConnection;393;0;102;0
WireConnection;393;1;246;0
WireConnection;512;0;510;0
WireConnection;512;2;511;0
WireConnection;508;0;509;0
WireConnection;508;3;198;0
WireConnection;143;0;393;0
WireConnection;168;0;143;0
WireConnection;513;0;508;0
WireConnection;513;1;512;0
WireConnection;158;0;291;0
WireConnection;158;1;513;0
WireConnection;158;2;171;0
WireConnection;387;0;158;0
WireConnection;475;0;489;0
WireConnection;485;0;484;0
WireConnection;485;1;483;0
WireConnection;473;0;475;0
WireConnection;505;0;507;0
WireConnection;505;3;196;0
WireConnection;299;0;290;0
WireConnection;299;1;292;0
WireConnection;299;2;300;0
WireConnection;486;0;485;0
WireConnection;458;0;473;0
WireConnection;458;1;461;0
WireConnection;220;0;505;4
WireConnection;173;0;175;0
WireConnection;173;1;505;0
WireConnection;163;0;165;0
WireConnection;163;1;162;0
WireConnection;301;0;299;0
WireConnection;471;0;458;0
WireConnection;465;0;486;0
WireConnection;465;1;473;0
WireConnection;470;0;466;1
WireConnection;470;1;469;0
WireConnection;482;0;470;0
WireConnection;482;1;471;0
WireConnection;481;0;473;0
WireConnection;468;0;488;0
WireConnection;468;1;529;0
WireConnection;157;0;163;0
WireConnection;157;1;173;0
WireConnection;157;2;170;0
WireConnection;457;0;465;0
WireConnection;217;0;531;0
WireConnection;217;1;221;0
WireConnection;217;2;222;0
WireConnection;462;0;457;0
WireConnection;462;1;468;0
WireConnection;463;0;482;0
WireConnection;463;1;481;0
WireConnection;223;0;217;0
WireConnection;394;0;157;0
WireConnection;479;0;526;0
WireConnection;479;1;462;0
WireConnection;479;2;470;0
WireConnection;459;0;463;0
WireConnection;459;1;491;0
WireConnection;497;0;459;0
WireConnection;494;0;479;0
WireConnection;498;0;499;0
WireConnection;498;1;500;0
WireConnection;212;0;162;4
WireConnection;0;2;498;0
WireConnection;0;10;532;0
ASEEND*/
//CHKSM=FF10F9E920B257F3C767DA30FF0C13B8347A8EF7