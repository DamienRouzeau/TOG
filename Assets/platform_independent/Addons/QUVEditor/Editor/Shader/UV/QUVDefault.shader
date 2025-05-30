// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/QTools/QUVDefault" 
{
	Properties 
	{
	}
	
	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200		
		Lighting off
		Cull off
		Blend SrcAlpha OneMinusSrcAlpha		
		//ZWrite off
		
		PASS 
		{
			CGPROGRAM
			#pragma vertex vert
        	#pragma fragment frag
			
			struct appdata 
	        {	
	            float4 vertex: POSITION;	
	            float4 color : COLOR;		
	        };
			
			struct v2f 
			{				
	            float4 pos   : SV_POSITION;	
	            float4 color : COLOR;		
	        };		 	         
		        	
	        v2f vert(appdata v)	
	        {	
	            v2f o;	
	            o.pos = UnityObjectToClipPos(v.vertex);	
	            o.color = v.color; 
	            return o;	
	        }	

	        float4 frag(v2f IN): COLOR	
	        {		        	        	
	            return IN.color;
	        }
			
			ENDCG
		}
	}
}
