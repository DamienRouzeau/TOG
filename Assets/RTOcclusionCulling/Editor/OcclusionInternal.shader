Shader "Hidden/Internal-Occlusion"
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Int) = 1
	}

	SubShader
	{
		Tags { "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest [_ZTest]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 color : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			//sampler2D _MainTex;
			float4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.color = _Color;
				o.color.rgb *= 0.75 + 0.25 * dot(UnityObjectToWorldNormal(v.normal), float3(0.3f, 0.95f, 0.3f));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}