﻿Shader "Image/FlipCoord" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		TextureRect ("Texture Clip Rect",Vector) = (0,0,1,1)
	}
	SubShader {
		Pass{
			ZTest Always Cull Off ZWrite Off
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
			};		




			v2f vert(appdata_img  IN) {
				
				v2f Out;
				Out.position.xy=2*sign(IN.vertex.xy)-1;
				Out.position.z = 1.0;
				Out.position.w = 1.0;
			  	Out.texCoord.xy =IN.texcoord.xy;
			  //	Out.texCoord.x=1-Out.texCoord.x;
			    //Out.texCoord.y=1-Out.texCoord.y;

			   return Out;
			}

			float4 frag(v2f IN) :COLOR  {
				return tex2D(_MainTex, IN.texCoord.xy);
			}

			ENDCG
		}
	} 
}
