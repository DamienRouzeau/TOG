Shader "Hidden/Mercator/TerrainStamp"
{
	Properties
	{
		_HeightTex ("Texture", 2D) = "black" {}
		_MaskTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off
		Blend [_SrcFactor] [_DstFactor]
		BlendOp [_BlendOp]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 height : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 height : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			float _Opacity;
			sampler2D _HeightTex;
			float4 _HeightTex_ST;
			float _HeightPow;
			sampler2D _MaskTex;
			float _MaskPow;
			float _RangeFrom;
			float _RangeTo;
			float _PremulPow;
			float _PremulValue;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = float4(v.vertex.xy * 2 - 1, 0, 1);
				o.vertex.y *= -1;
				o.uv = v.uv;
				o.height = v.height;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float heightSample = tex2D(_HeightTex, TRANSFORM_TEX(i.uv, _HeightTex));
				float maskSample = tex2D(_MaskTex, i.uv).r;

				#if UNITY_COLORSPACE_GAMMA 
				heightSample = pow(heightSample, 2.2f);
				maskSample = pow(maskSample, 2.2f);
				#endif

				float height = i.height.x + pow(heightSample, _HeightPow) * (i.height.y - i.height.x);
				float alpha = pow(maskSample, _MaskPow) * _Opacity;
				float premul = pow(alpha, _PremulPow);
				float valueInRange = lerp(_RangeFrom, _RangeTo, saturate(height));
				return float4(lerp(_PremulValue, valueInRange, premul).rrr, alpha);
			}
			ENDCG
		}
	}
}
