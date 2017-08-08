// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Telexistence/Demo/MaskedCamShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MaskTex ("Mask (Alpha)", 2D) = "white" {}
		_Strength ("",Range (0.0, 1.0))= 0.5 
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		Pass{
			LOD 200
			
			Lighting Off
			ZWrite Off
			Cull Off
			Fog { Mode off }
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			sampler2D _MainTex;
			sampler2D _MaskTex;
			half _Strength;

			struct v2f {
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			};
			v2f vert(appdata_base  v) {
			    v2f o;
			    o.pos = UnityObjectToClipPos(v.vertex);
			    o.uv = v.texcoord;
			    return o;
			}
			half4 frag(v2f IN) : SV_Target {
				float4 c;
				c = tex2D (_MainTex, IN.uv);
				c.a=lerp(1,tex2D(_MaskTex,IN.uv).r,_Strength);
				return c;
			}

	        ENDCG
		}
	} 
}
