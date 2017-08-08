// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LeapMotion/Passthrough/Foreground" {
  Properties {
    _ColorSpaceGamma ("Color Space Gamma", Float) = 1.0
  }

  SubShader {
    Tags {"Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent"}

    Lighting Off
    Cull Off
    Zwrite Off
    ZTest Off

    Blend SrcAlpha OneMinusSrcAlpha
    //BlendOp Max

    Pass{
    CGPROGRAM
    #pragma multi_compile LEAP_FORMAT_IR LEAP_FORMAT_RGB
    #include "LeapCG.cginc"
    #include "UnityCG.cginc"
    
    #pragma vertex vert
    #pragma fragment frag
    
    uniform float _ColorSpaceGamma;

    struct frag_in{
      float4 position : SV_POSITION;
      float4 screenPos  : TEXCOORD1;
    };

    frag_in vert(appdata_img v){
      frag_in o;
      o.position = UnityObjectToClipPos(v.vertex);
      o.screenPos = ComputeScreenPos(o.position);
      return o;
    }

    float4 frag (frag_in i) : COLOR {
       float4 colorBrightness = LeapRawColorBrightness(i.screenPos);
       float alpha = colorBrightness.a;
      
    	if(alpha<0.15)
    		alpha=0;
    	else alpha=1;
      	return float4(pow(colorBrightness.rrr, 1/_ColorSpaceGamma)*alpha, alpha)*float4(0.96,0.725,0.62,1);
    }

    ENDCG
    }
  } 
  Fallback off
}
