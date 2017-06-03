Shader "Image/GazeBlit_Rect" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		Pass{
			ZTest Always Cull Off ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Fog { Mode off }
			
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			sampler2D _MainTex;
 
			struct v2f 
			{
				float4 position : SV_POSITION;
				float2 texCoord  : TEXCOORD0;
			};
	 
			struct a2v
			{
				float4 vertex   : POSITION;
				float2 texCoord  : TEXCOORD0;
			};		

			float DebugBlitArea=1;
			float4 _Parameters=float4(0.5,0.5,0.3,0.5);
			
			v2f vert(a2v  IN) {
				
				v2f Out;
				Out.position = mul(UNITY_MATRIX_MVP, IN.vertex);
				Out.position.z = 1.0;
				Out.position.w = 1.0;
			  	Out.texCoord.xy =IN.texCoord.xy;
			   return Out;
			}
			float sigmoid(float x)
			{
				if(x>=1)
					return 1;
				else if(x<=-1) return 0;
				else return 0.5 + x*(1-abs(x)*0.5);
			}
			float4 frag(v2f IN) :COLOR  {
				float2 uv=IN.texCoord.xy;

				float r1=_Parameters.z;
				float r2=_Parameters.w;
				float dist=max(abs(uv.x-_Parameters.x),abs(uv.y-_Parameters.y));

				float t;
				if(dist<r1)
					t=1;
				else if(dist>r2)
					t=0;
				else 
				{
					t=1-(dist-r1)/(r2-r1);
					//t=sigmoid(2*t-1);
				}

				float4 blitClr= float4(t,t,t,1);

				return lerp( float4(tex2D(_MainTex, IN.texCoord.xy).rgb,t),blitClr,DebugBlitArea);
			}

			ENDCG
		}
	} 
}
