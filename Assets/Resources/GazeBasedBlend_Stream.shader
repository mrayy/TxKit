Shader "GazeBased/Blend_Stream" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TargetMask ("Mask (RGB)", 2D) = "white" {}
		//_Strength ("Masking amount",Range(0,1))= 0.5 
	}
	SubShader {
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		Pass{
			Lighting Off
			ZWrite Off
			ZTest Off
			Cull Off
			Fog { Mode off }
			//Blend SrcAlpha DstAlpha

			CGPROGRAM
			
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _TargetMask;

			float _MinAlpha=0;
			float _MaxAlpha=1;
			float _Strength=0;


			half4 frag(v2f_img IN) : SV_Target {
				float4 c;
				c=tex2D(_MainTex,IN.uv);
				float w=tex2D(_TargetMask,IN.uv).r;//*0.7+0.3;
				//c.rgb=lerp(b.rgb,c.rgb,tex2D(_TargetMask,IN.uv).r*0.7+0.3);
				//return float4(c.rgb,min(max(_MinAlpha,_Strength),_MaxAlpha));
				//float minBlur=lerp(0,0.1,w);
				//float blurW=lerp(minBlur,1.0,_Strength);
				//blurW*=blurW;
				//c.rgb=lerp(b.rgb,c.rgb,blurW);
				//c.a=lerp(0.3,0.9,_Strength);//
				//the minimum alpha value is determined by the weight of the pixel
				float alphaW=(_MinAlpha+(1-_MinAlpha)*_Strength);//lerp(w,_MaxAlpha,(_MinAlpha+(1-_MinAlpha)*_Strength));
				//float alphaW=max(w,(0.5+0.5*_Strength));
			//	alphaW=alphaW*alphaW;
			//	alphaW=min(0.6,alphaW);
				c.a=lerp(alphaW,1,_Strength);//
				c.a=min(max(_MinAlpha,c.a),_MaxAlpha);
				//c.a=max(w,_Strength);
				//c.a=alphaW;//
				return c;
			}

	        ENDCG
		}
	} 
}
 