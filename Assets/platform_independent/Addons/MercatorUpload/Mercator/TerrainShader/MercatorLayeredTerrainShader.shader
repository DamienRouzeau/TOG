Shader "Mercator/Layered Terrain Shader"
{
	Properties
	{
		_Prop_MinHeight ("", Float) = 0.0
		_Prop_MaxHeight ("", Float) = 0.0
		[MaterialToggle] _Prop_ClipWarning ("", Float) = 1.0
		_Prop_DebugMode ("", Float) = 0.0

		_Layer0_Enabled ("", Float) = 0.0
		_Layer0_Albedo ("", 2D) = "black" {}
		_Layer0_Normal ("", 2D) = "bump" {}
		_Layer0_Glossiness ("", Range(0,1)) = 0.5
		_Layer0_GlossFlip ("", Float) = 0.0
		_Layer0_Metallic ("", Range(0,1)) = 0.0
		_Layer0_UV_ScaleX ("", Float) = 1.0
		_Layer0_UV_ScaleY ("", Float) = 1.0
		_Layer0_UV_Rotation ("", Range(0,360)) = 0.0
		_Layer0_UV2 ("", Float) = 0.0
		_Layer0_UV2_ScaleX ("", Float) = 1.0
		_Layer0_UV2_ScaleY ("", Float) = 1.0
		_Layer0_UV2_Rotation ("", Range(0,360)) = 0.0
		_Layer0_UV2_NoiseFactor ("", Float) = 0.0
		_Layer0_UV2_NoiseScale ("", Float) = 1.0
		_Layer0_Color ("", Color) = (1,1,1,1)
		_Layer0_Blend_NoiseFactor ("", Float) = 0.0
		_Layer0_Blend_Bias ("", Float) = 0.0
		_Layer0_Blend_NoiseScale ("", Float) = 1.0
		_Layer0_Height ("", Float) = 0.0
		_Layer0_Height_Alpha0 ("", Range(0,1)) = 1.0
		_Layer0_Height_Alpha1 ("", Range(0,1)) = 1.0
		_Layer0_Height_Fade0 ("", Range(0,1)) = 0.0
		_Layer0_Height_Fade1 ("", Range(0,1)) = 0.0
		_Layer0_Height_Fade2 ("", Range(0,1)) = 1.0
		_Layer0_Height_Fade3 ("", Range(0,1)) = 1.0
		_Layer0_Slope ("", Float) = 0.0
		_Layer0_Slope_Alpha0 ("", Range(0,1)) = 1.0
		_Layer0_Slope_Alpha1 ("", Range(0,1)) = 1.0
		_Layer0_Slope_Fade0 ("", Range(0,1)) = 0.0
		_Layer0_Slope_Fade1 ("", Range(0,1)) = 0.0
		_Layer0_Slope_Fade2 ("", Range(0,1)) = 1.0
		_Layer0_Slope_Fade3 ("", Range(0,1)) = 1.0

		_Layer1_Enabled ("", Float) = 0.0
		_Layer1_Albedo ("", 2D) = "black" {}
		_Layer1_Normal ("", 2D) = "bump" {}
		_Layer1_Glossiness ("", Range(0,1)) = 0.5
		_Layer1_GlossFlip ("", Float) = 0.0
		_Layer1_Metallic ("", Range(0,1)) = 0.0
		_Layer1_UV_ScaleX ("", Float) = 1.0
		_Layer1_UV_ScaleY ("", Float) = 1.0
		_Layer1_UV_Rotation ("", Range(0,360)) = 0.0
		_Layer1_UV2 ("", Float) = 0.0
		_Layer1_UV2_ScaleX ("", Float) = 1.0
		_Layer1_UV2_ScaleY ("", Float) = 1.0
		_Layer1_UV2_Rotation ("", Range(0,360)) = 0.0
		_Layer1_UV2_NoiseFactor ("", Float) = 0.0
		_Layer1_UV2_NoiseScale ("", Float) = 1.0
		_Layer1_Color ("", Color) = (1,1,1,1)
		_Layer1_Blend_NoiseFactor ("", Float) = 0.0
		_Layer1_Blend_Bias ("", Float) = 0.0
		_Layer1_Blend_NoiseScale ("", Float) = 1.0
		_Layer1_Height ("", Float) = 0.0
		_Layer1_Height_Alpha0 ("", Range(0,1)) = 1.0
		_Layer1_Height_Alpha1 ("", Range(0,1)) = 1.0
		_Layer1_Height_Fade0 ("", Range(0,1)) = 0.0
		_Layer1_Height_Fade1 ("", Range(0,1)) = 0.0
		_Layer1_Height_Fade2 ("", Range(0,1)) = 1.0
		_Layer1_Height_Fade3 ("", Range(0,1)) = 1.0
		_Layer1_Slope ("", Float) = 0.0
		_Layer1_Slope_Alpha0 ("", Range(0,1)) = 1.0
		_Layer1_Slope_Alpha1 ("", Range(0,1)) = 1.0
		_Layer1_Slope_Fade0 ("", Range(0,1)) = 0.0
		_Layer1_Slope_Fade1 ("", Range(0,1)) = 0.0
		_Layer1_Slope_Fade2 ("", Range(0,1)) = 1.0
		_Layer1_Slope_Fade3 ("", Range(0,1)) = 1.0

		_Layer2_Enabled ("", Float) = 0.0
		_Layer2_Albedo ("", 2D) = "black" {}
		_Layer2_Normal ("", 2D) = "bump" {}
		_Layer2_Glossiness ("", Range(0,1)) = 0.5
		_Layer2_GlossFlip ("", Float) = 0.0
		_Layer2_Metallic ("", Range(0,1)) = 0.0
		_Layer2_UV_ScaleX ("", Float) = 1.0
		_Layer2_UV_ScaleY ("", Float) = 1.0
		_Layer2_UV_Rotation ("", Range(0,360)) = 0.0
		_Layer2_UV2 ("", Float) = 0.0
		_Layer2_UV2_ScaleX ("", Float) = 1.0
		_Layer2_UV2_ScaleY ("", Float) = 1.0
		_Layer2_UV2_Rotation ("", Range(0,360)) = 0.0
		_Layer2_UV2_NoiseFactor ("", Float) = 0.0
		_Layer2_UV2_NoiseScale ("", Float) = 1.0
		_Layer2_Color ("", Color) = (1,1,1,1)
		_Layer2_Blend_NoiseFactor ("", Float) = 0.0
		_Layer2_Blend_Bias ("", Float) = 0.0
		_Layer2_Blend_NoiseScale ("", Float) = 1.0
		_Layer2_Height ("", Float) = 0.0
		_Layer2_Height_Alpha0 ("", Range(0,1)) = 1.0
		_Layer2_Height_Alpha1 ("", Range(0,1)) = 1.0
		_Layer2_Height_Fade0 ("", Range(0,1)) = 0.0
		_Layer2_Height_Fade1 ("", Range(0,1)) = 0.0
		_Layer2_Height_Fade2 ("", Range(0,1)) = 1.0
		_Layer2_Height_Fade3 ("", Range(0,1)) = 1.0
		_Layer2_Slope ("", Float) = 0.0
		_Layer2_Slope_Alpha0 ("", Range(0,1)) = 1.0
		_Layer2_Slope_Alpha1 ("", Range(0,1)) = 1.0
		_Layer2_Slope_Fade0 ("", Range(0,1)) = 0.0
		_Layer2_Slope_Fade1 ("", Range(0,1)) = 0.0
		_Layer2_Slope_Fade2 ("", Range(0,1)) = 1.0
		_Layer2_Slope_Fade3 ("", Range(0,1)) = 1.0

		_Layer3_Enabled ("", Float) = 0.0
		_Layer3_Albedo ("", 2D) = "black" {}
		_Layer3_Normal ("", 2D) = "bump" {}
		_Layer3_Glossiness ("", Range(0,1)) = 0.5
		_Layer3_GlossFlip ("", Float) = 0.0
		_Layer3_Metallic ("", Range(0,1)) = 0.0
		_Layer3_UV_ScaleX ("", Float) = 1.0
		_Layer3_UV_ScaleY ("", Float) = 1.0
		_Layer3_UV_Rotation ("", Range(0,360)) = 0.0
		_Layer3_UV2 ("", Float) = 0.0
		_Layer3_UV2_ScaleX ("", Float) = 1.0
		_Layer3_UV2_ScaleY ("", Float) = 1.0
		_Layer3_UV2_Rotation ("", Range(0,360)) = 0.0
		_Layer3_UV2_NoiseFactor ("", Float) = 0.0
		_Layer3_UV2_NoiseScale ("", Float) = 1.0
		_Layer3_Color ("", Color) = (1,1,1,1)
		_Layer3_Blend_NoiseFactor ("", Float) = 0.0
		_Layer3_Blend_Bias ("", Float) = 0.0
		_Layer3_Blend_NoiseScale ("", Float) = 1.0
		_Layer3_Height ("", Float) = 0.0
		_Layer3_Height_Alpha0 ("", Range(0,1)) = 1.0
		_Layer3_Height_Alpha1 ("", Range(0,1)) = 1.0
		_Layer3_Height_Fade0 ("", Range(0,1)) = 0.0
		_Layer3_Height_Fade1 ("", Range(0,1)) = 0.0
		_Layer3_Height_Fade2 ("", Range(0,1)) = 1.0
		_Layer3_Height_Fade3 ("", Range(0,1)) = 1.0
		_Layer3_Slope ("", Float) = 0.0
		_Layer3_Slope_Alpha0 ("", Range(0,1)) = 1.0
		_Layer3_Slope_Alpha1 ("", Range(0,1)) = 1.0
		_Layer3_Slope_Fade0 ("", Range(0,1)) = 0.0
		_Layer3_Slope_Fade1 ("", Range(0,1)) = 0.0
		_Layer3_Slope_Fade2 ("", Range(0,1)) = 1.0
		_Layer3_Slope_Fade3 ("", Range(0,1)) = 1.0

		_Layer4_Enabled ("", Float) = 0.0
		_Layer4_Albedo ("", 2D) = "black" {}
		_Layer4_Normal ("", 2D) = "bump" {}
		_Layer4_Glossiness ("", Range(0,1)) = 0.5
		_Layer4_GlossFlip ("", Float) = 0.0
		_Layer4_Metallic ("", Range(0,1)) = 0.0
		_Layer4_UV_ScaleX ("", Float) = 1.0
		_Layer4_UV_ScaleY ("", Float) = 1.0
		_Layer4_UV_Rotation ("", Range(0,360)) = 0.0
		_Layer4_UV2 ("", Float) = 0.0
		_Layer4_UV2_ScaleX ("", Float) = 1.0
		_Layer4_UV2_ScaleY ("", Float) = 1.0
		_Layer4_UV2_Rotation ("", Range(0,360)) = 0.0
		_Layer4_UV2_NoiseFactor ("", Float) = 0.0
		_Layer4_UV2_NoiseScale ("", Float) = 1.0
		_Layer4_Color ("", Color) = (1,1,1,1)
		_Layer4_Blend_NoiseFactor ("", Float) = 0.0
		_Layer4_Blend_Bias ("", Float) = 0.0
		_Layer4_Blend_NoiseScale ("", Float) = 1.0
		_Layer4_Height ("", Float) = 0.0
		_Layer4_Height_Alpha0 ("", Range(0,1)) = 1.0
		_Layer4_Height_Alpha1 ("", Range(0,1)) = 1.0
		_Layer4_Height_Fade0 ("", Range(0,1)) = 0.0
		_Layer4_Height_Fade1 ("", Range(0,1)) = 0.0
		_Layer4_Height_Fade2 ("", Range(0,1)) = 1.0
		_Layer4_Height_Fade3 ("", Range(0,1)) = 1.0
		_Layer4_Slope ("", Float) = 0.0
		_Layer4_Slope_Alpha0 ("", Range(0,1)) = 1.0
		_Layer4_Slope_Alpha1 ("", Range(0,1)) = 1.0
		_Layer4_Slope_Fade0 ("", Range(0,1)) = 0.0
		_Layer4_Slope_Fade1 ("", Range(0,1)) = 0.0
		_Layer4_Slope_Fade2 ("", Range(0,1)) = 1.0
		_Layer4_Slope_Fade3 ("", Range(0,1)) = 1.0

		_Layer5_Enabled ("", Float) = 0.0
		_Layer5_Albedo ("", 2D) = "black" {}
		_Layer5_Normal ("", 2D) = "bump" {}
		_Layer5_Glossiness ("", Range(0,1)) = 0.5
		_Layer5_GlossFlip ("", Float) = 0.0
		_Layer5_Metallic ("", Range(0,1)) = 0.0
		_Layer5_UV_ScaleX ("", Float) = 1.0
		_Layer5_UV_ScaleY ("", Float) = 1.0
		_Layer5_UV_Rotation ("", Range(0,360)) = 0.0
		_Layer5_UV2 ("", Float) = 0.0
		_Layer5_UV2_ScaleX ("", Float) = 1.0
		_Layer5_UV2_ScaleY ("", Float) = 1.0
		_Layer5_UV2_Rotation ("", Range(0,360)) = 0.0
		_Layer5_UV2_NoiseFactor ("", Float) = 0.0
		_Layer5_UV2_NoiseScale ("", Float) = 1.0
		_Layer5_Color ("", Color) = (1,1,1,1)
		_Layer5_Blend_NoiseFactor ("", Float) = 0.0
		_Layer5_Blend_Bias ("", Float) = 0.0
		_Layer5_Blend_NoiseScale ("", Float) = 1.0
		_Layer5_Height ("", Float) = 0.0
		_Layer5_Height_Alpha0 ("", Range(0,1)) = 1.0
		_Layer5_Height_Alpha1 ("", Range(0,1)) = 1.0
		_Layer5_Height_Fade0 ("", Range(0,1)) = 0.0
		_Layer5_Height_Fade1 ("", Range(0,1)) = 0.0
		_Layer5_Height_Fade2 ("", Range(0,1)) = 1.0
		_Layer5_Height_Fade3 ("", Range(0,1)) = 1.0
		_Layer5_Slope ("", Float) = 0.0
		_Layer5_Slope_Alpha0 ("", Range(0,1)) = 1.0
		_Layer5_Slope_Alpha1 ("", Range(0,1)) = 1.0
		_Layer5_Slope_Fade0 ("", Range(0,1)) = 0.0
		_Layer5_Slope_Fade1 ("", Range(0,1)) = 0.0
		_Layer5_Slope_Fade2 ("", Range(0,1)) = 1.0
		_Layer5_Slope_Fade3 ("", Range(0,1)) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		#include "noiseSimplex.cginc"

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		float _Prop_MinHeight;
		float _Prop_MaxHeight;
		bool _Prop_ClipWarning;
		uint _Prop_DebugMode;

		#define DEFINE_LAYER(n) float	  _Layer##n##_Enabled;\
								sampler2D _Layer##n##_Albedo;\
								sampler2D _Layer##n##_Normal;\
								float	  _Layer##n##_Glossiness;\
								float	  _Layer##n##_GlossFlip;\
								float	  _Layer##n##_Metallic;\
								float	  _Layer##n##_UV_ScaleX;\
								float	  _Layer##n##_UV_ScaleY;\
								float	  _Layer##n##_UV_Rotation;\
								float	  _Layer##n##_UV2;\
								float	  _Layer##n##_UV2_ScaleX;\
								float	  _Layer##n##_UV2_ScaleY;\
								float	  _Layer##n##_UV2_Rotation;\
								float	  _Layer##n##_UV2_NoiseFactor;\
								float	  _Layer##n##_UV2_NoiseScale;\
								float4	  _Layer##n##_Color;\
								float	  _Layer##n##_Blend_NoiseFactor;\
								float	  _Layer##n##_Blend_Bias;\
								float	  _Layer##n##_Blend_NoiseScale;\
								float	  _Layer##n##_Height;\
								float	  _Layer##n##_Height_Alpha0;\
								float	  _Layer##n##_Height_Alpha1;\
								float	  _Layer##n##_Height_Fade0;\
								float	  _Layer##n##_Height_Fade1;\
								float	  _Layer##n##_Height_Fade2;\
								float	  _Layer##n##_Height_Fade3;\
								float	  _Layer##n##_Slope;\
								float	  _Layer##n##_Slope_Alpha0;\
								float	  _Layer##n##_Slope_Alpha1;\
								float	  _Layer##n##_Slope_Fade0;\
								float	  _Layer##n##_Slope_Fade1;\
								float	  _Layer##n##_Slope_Fade2;\
								float	  _Layer##n##_Slope_Fade3;\

		#define BLEND_LAYER(n)	if (_Layer##n##_Enabled) {\
									float blend = 1.0f;\
									if (_Layer##n##_Height)\
									{\
										Blend(blend, height, _Layer##n##_Height_Alpha0, _Layer##n##_Height_Alpha1,\
											_Layer##n##_Height_Fade0, _Layer##n##_Height_Fade1, _Layer##n##_Height_Fade2, _Layer##n##_Height_Fade3);\
									}\
									if (_Layer##n##_Slope)\
									{\
										Blend(blend, slope, _Layer##n##_Slope_Alpha0, _Layer##n##_Slope_Alpha1,\
											_Layer##n##_Slope_Fade0, _Layer##n##_Slope_Fade1, _Layer##n##_Slope_Fade2, _Layer##n##_Slope_Fade3);\
									}\
									blend *= _Layer##n##_Color.a;\
									float noiseValue = 0;\
									if (_Layer##n##_Blend_NoiseScale != 0.0f)\
									{\
										noiseValue = snoise(IN.worldPos / _Layer##n##_Blend_NoiseScale);\
										noiseValue += (blend * 2 - 1);\
										noiseValue *= _Layer##n##_Blend_NoiseFactor;\
									}\
									blend = saturate(blend + _Layer##n##_Blend_Bias + noiseValue);\
									if (blend > 0.0f)\
									{\
										if (_Prop_DebugMode != 4)\
										{\
											float2 uv = IN.worldPos.xz;\
											float sinTheta = sin(_Layer##n##_UV_Rotation * 0.01745329251f);\
											float cosTheta = cos(_Layer##n##_UV_Rotation * 0.01745329251f);\
											uv = float2(uv.x * cosTheta - uv.y * sinTheta, uv.x * sinTheta + uv.y * cosTheta);\
											uv /= float2(_Layer##n##_UV_ScaleX, _Layer##n##_UV_ScaleY);\
											float4 texSample = tex2D(_Layer##n##_Albedo, uv);\
											float4 nrmSample = tex2D(_Layer##n##_Normal, uv);\
											if (_Layer##n##_UV2)\
											{\
												uv = IN.worldPos.xz;\
												sinTheta = sin((_Layer##n##_UV_Rotation + _Layer##n##_UV2_Rotation) * 0.01745329251f);\
												cosTheta = cos((_Layer##n##_UV_Rotation + _Layer##n##_UV2_Rotation) * 0.01745329251f);\
												uv = float2(uv.x * cosTheta - uv.y * sinTheta, uv.x * sinTheta + uv.y * cosTheta);\
												uv /= float2(_Layer##n##_UV_ScaleX * _Layer##n##_UV2_ScaleX, _Layer##n##_UV_ScaleY * _Layer##n##_UV2_ScaleY);\
												float noiseValue = 0;\
												if (_Layer##n##_UV2_NoiseScale != 0.0f)\
												{\
													noiseValue = snoise(IN.worldPos / _Layer##n##_UV2_NoiseScale);\
													noiseValue *= _Layer##n##_UV2_NoiseFactor;\
												}\
												texSample = lerp(texSample, tex2D(_Layer##n##_Albedo, uv), saturate(0.5f + noiseValue));\
												nrmSample = lerp(nrmSample, tex2D(_Layer##n##_Normal, uv), saturate(0.5f + noiseValue));\
											}\
											o.Albedo = lerp(o.Albedo, texSample.rgb * _Layer##n##_Color.rgb, blend);\
											o.Smoothness = lerp(o.Smoothness, _Layer##n##_Glossiness * (_Layer##n##_GlossFlip ? 1 - texSample.a : texSample.a), blend);\
											o.Metallic = lerp(o.Metallic, _Layer##n##_Metallic, blend);\
											o.Normal = lerp(o.Normal, UnpackNormal(nrmSample), blend);\
										}\
										else\
										{\
											o.Albedo = lerp(o.Albedo, IdentifyColor(n), blend);\
										}\
									}\
								}

		DEFINE_LAYER(0)
		DEFINE_LAYER(1)
		DEFINE_LAYER(2)
		DEFINE_LAYER(3)
		DEFINE_LAYER(4)
		DEFINE_LAYER(5)

		float3 IdentifyColor(int n)
		{
			if (n == 0) return float3(1, 0, 0);
			if (n == 1) return float3(0, 1, 0);
			if (n == 2) return float3(0, 0, 1);
			if (n == 3) return float3(0, 1, 1);
			if (n == 4) return float3(1, 0, 1);
			if (n == 5) return float3(1, 1, 0);
			return float3(0, 0, 0);
		}

		void Blend(inout float blend, float input, float alpha0, float alpha1,
			float fade0, float fade1, float fade2, float fade3)
		{
			const float epsilon = 0.00001f;

			fade1 = max(fade1, fade0);
			fade2 = min(fade2, fade3);

			if (input >= fade0 && input <= fade3)
			{
				if (input < fade1)
				{
					blend *= (input - fade0) / max(fade1 - fade0, epsilon);
					blend *= alpha0;
				}
				else if (input > fade2)
				{
					blend *= 1.0f - ((input - fade2) / max(fade3 - fade2, epsilon));
					blend *= alpha1;
				}
				else
				{
					blend *= lerp(alpha0, alpha1, (input - fade1) / max(fade2 - fade1, epsilon));
				}
			}
			else
			{
				blend = 0;
			}
		}

		float3 Hue(float H)
		{
			half R = abs(H * 6 - 3) - 1;
			half G = 2 - abs(H * 6 - 2);
			half B = 2 - abs(H * 6 - 4);
			return saturate(half3(R,G,B));
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float height = saturate((IN.worldPos.y - _Prop_MinHeight) / (_Prop_MaxHeight - _Prop_MinHeight));
			float slope = length(WorldNormalVector(IN, o.Normal).xz);

			o.Metallic = 0.0f;
			o.Smoothness = 0.0f;
			o.Alpha = 1.0f;
			o.Albedo = float3(0.8f, 0.8f, 0.8f);

			const float epsilon = 1.0f / 8192.0f;
			if (_Prop_ClipWarning && (height < epsilon || height > 1.0f - epsilon))
			{
				o.Albedo = 0;
				o.Emission = float3(1,0,1);
				return;
			}

			switch (_Prop_DebugMode)
			{
				case 0:
				case 4:
					BLEND_LAYER(0)
					BLEND_LAYER(1)
					BLEND_LAYER(2)
					BLEND_LAYER(3)
					BLEND_LAYER(4)
					BLEND_LAYER(5)
					break;
				case 1:
					o.Emission = Hue(lerp(2.0f / 3.0f, 0.0f, height));
					o.Albedo = 0;
					break;
				case 2:
					o.Emission = WorldNormalVector(IN, o.Normal) * 0.5f + 0.5f;
					o.Albedo = 0;
					break;
				case 3:
					o.Emission = slope;
					o.Albedo = 0;
					break;
			}
		}
		ENDCG
	}
	CustomEditor "ThreeEyedGames.Mercator.MercatorLayeredTerrainShaderEditor"
	FallBack "Diffuse"
}
