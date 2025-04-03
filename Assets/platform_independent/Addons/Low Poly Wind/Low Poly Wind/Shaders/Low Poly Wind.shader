Shader "Low Poly/Low Poly Wind"  
{	
	Properties 
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0, 1)) = 0.5
		_Metallic("Metallic", Range(0, 1)) = 0.0

		_ColorT1 ("ColorT1", Color) = (1, 1, 1, 1)
		_ColorT2 ("ColorT2", Color) = (1, 1, 1, 1)
		_ColorT3 ("ColorT3", Color) = (1, 1, 1, 1)
		_ColorT4 ("ColorT4", Color) = (1, 1, 1, 1)
		_ColorT5 ("ColorT5", Color) = (1, 1, 1, 1)		

		_DistanceX("DistanceX", Range(0, 10)) = 0.7
		_DistanceY("DistanceY", Range(0, 10)) = 0.5
		_DistanceZ("DistanceZ", Range(0, 10)) = 0.7

		_SpeedX("SpeedX", Range(0, 5) ) = 1.3
		_SpeedY("SpeedY", Range(0, 5) ) = 0.3
		_SpeedZ("SpeedZ", Range(0, 5) ) = 1.3

		[MaterialToggle] _WorldSpace("Use World Space", Float) = 0

		[MaterialToggle] _MoveTrunk("Move Trunk", Float) = 0
		_TrunkSpeed("TrunkSpeed", Range(.1, 5)) = 3.5

		_MinHeight("MinHeight", Range(0, 10)) = 0			

		[MaterialToggle] _UseHeight("Use Height", Float) = 0
		_HeightDist("HeightDist", Range(0.1, 5)) = 3.5
		_HeightSpeed("HeightSpeed", Range(0.1, 5)) = 3.5

		_Precision("Precision", Range(0.00, 0.03)) = 0.00

	}
	SubShader
	{
		Tags 
		{ 
			"RenderType"="Opaque" 
			"DisableBatching" = "True"
		}

		LOD 200
		
		CGPROGRAM
		
		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
		#pragma target 3.0
		
		#include "UnityCg.cginc"
		
		struct Input 
		{
			float2 uv_MainTex;
			float4 color : Color;
		};

		//Standard Surface Shader Properties
		sampler2D _MainTex;

		fixed4 _Color;

		half _Glossiness;
		half _Metallic;


		//Default colors used for comparison 
		float4 _ColorT1;
		float4 _ColorT2;
		float4 _ColorT3;
		float4 _ColorT4;
		float4 _ColorT5;
		

		half _SpeedX;
		half _SpeedY;
		half _SpeedZ;
		
		half _DistanceX;
		half _DistanceY;
		half _DistanceZ;

		fixed _WorldSpace;

		half _MoveTrunk;
		half _TrunkSpeed;

		half _HeightDist;
		half _HeightSpeed;

		fixed _UseHeight;
		half _MinHeight;

		half _Precision;
		
		float randomNum(in float2 uv)
		{
     		float2 noise = (frac(sin(dot(uv, float2(12.9898,78.233)*2.0)) * 43758.5453));
     		return abs(noise.x + noise.y) * 0.1;
 		}

		bool colorCompare(in float4 c1, in float4 c2)
		{
			return  (c1.r > c2.r - _Precision && c1.r < c2.r + _Precision) &&
					(c1.g > c2.g - _Precision && c1.g < c2.g + _Precision) &&
					(c1.b > c2.b - _Precision && c1.b < c2.b + _Precision);
		}

		void vert( inout appdata_full v )
		{		
			//Texture Lookup
            float4 tex = tex2Dlod (_MainTex, float4(v.texcoord.xy, 0, 0));
			
			bool t1 = colorCompare(tex, _ColorT1);
			bool t2 = colorCompare(tex, _ColorT2);
			bool t3 = colorCompare(tex, _ColorT3);
			bool t4 = colorCompare(tex, _ColorT4);
			bool t5 = colorCompare(tex, _ColorT5);

			bool isLeaf = (t1 + t2 + t3 + t4 + t5 == 0); //Check if vertex is a leaf

			bool overMinHeight = (v.vertex.y > _MinHeight); //Min Height Threshold

			bool distanceModifier = !(_HeightDist == 5); //Check if Height Controlled Distance && Speed == 5
			bool speedModifier = !(_HeightSpeed == 5);


			//Height Control
			_DistanceX += (v.vertex.y / _HeightDist) * isLeaf * (_UseHeight * distanceModifier * overMinHeight);
			_DistanceZ += (v.vertex.y / _HeightDist) * isLeaf * (_UseHeight * distanceModifier * overMinHeight);
		

			_SpeedX += (v.vertex.y / _HeightSpeed) * isLeaf * (_UseHeight * speedModifier * overMinHeight);
			_SpeedZ += (v.vertex.y / _HeightSpeed) * isLeaf * (_UseHeight * speedModifier * overMinHeight);

			//Vector Calculation
			float x = sin(_Time.y * _SpeedX + randomNum(v.vertex.x)) * _DistanceX * randomNum(v.vertex.x);
			float y = sin(_Time.y * _SpeedY + randomNum(v.vertex.y)) * _DistanceY * randomNum(v.vertex.y);
			float z = sin(_Time.y * _SpeedZ + randomNum(v.vertex.z)) * _DistanceZ * randomNum(v.vertex.z);

			if (_WorldSpace == 1)
				v.vertex = mul(unity_ObjectToWorld, v.vertex);
			
			//Leaf Movement
			v.vertex.x += x * isLeaf * overMinHeight;
			v.vertex.y += y * isLeaf * overMinHeight;
			v.vertex.z += z * isLeaf * overMinHeight;

			//Trunk Movement
			v.vertex.x += x / _TrunkSpeed * !isLeaf * overMinHeight * _MoveTrunk;
			v.vertex.y += y / _TrunkSpeed * !isLeaf * overMinHeight * _MoveTrunk;
			v.vertex.z += z / _TrunkSpeed * !isLeaf * overMinHeight * _MoveTrunk;

			if (_WorldSpace == 1)
				v.vertex = mul(unity_WorldToObject, v.vertex);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			o.Albedo = c;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;			
		}
		ENDCG
	}
	FallBack "Diffuse"
	CustomEditor "WindEditor"
}
