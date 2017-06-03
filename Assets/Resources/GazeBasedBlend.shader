Shader "GazeBased/Blend" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TargetMask ("Base (RGB)", 2D) = "white" {}
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
			sampler2D _TargetMask;


			struct v2f {
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			};
			v2f vert(appdata_base  v) {
			    v2f o;
			    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			    o.uv = v.texcoord;
			    return o;
			}
			half4 frag(v2f IN) : SV_Target {
				float4 c;

				c=tex2D(_MainTex,IN.uv);
				c.a=lerp(tex2D(_TargetMask,IN.uv).r,1,0.7);
				return c;
			}

	        ENDCG
		}
	} 
}
 