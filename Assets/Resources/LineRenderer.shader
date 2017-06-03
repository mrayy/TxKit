Shader "Lines/Colored Blended" {
	SubShader {
		Pass{
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest Off
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha
			BindChannels { Bind "Vertex",vertex Bind "Color",color }
		}
	} 
}
