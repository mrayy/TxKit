using UnityEngine;
using System.Collections;

public class RenderTextureWrapper:TextureWrapper {



	public Texture2D ConvertRenderTexture(RenderTexture src)
	{
		return ConvertTexture (src);
	}

}
