Shader "Image/DistortionCorrection" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Pass{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
		
			CGPROGRAM
			
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "TelexistenceCG.cginc"

			sampler2D _MainTex;


			float2 PixelShift=float2(0,0);
			float2 TextureSize=float2(1,1);

			half4 frag(v2f_img IN) : SV_Target {
				IN.uv+=PixelShift/TextureSize;
				float4 c;
				float2 tc=_CorrectDistortion(IN.uv);
				if (any(clamp(tc, float2(0.0,0.0), float2(1.0, 1.0)) - tc))    
					c=0;
				else			
					c = tex2D (_MainTex, tc);

				c.a=1;
				return c;
			}

	        ENDCG
		}
	} 
}
 