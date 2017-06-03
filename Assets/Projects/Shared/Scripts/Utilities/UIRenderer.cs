using UnityEngine;
using System.Collections;

public class UIRenderer  {

	RenderTexture _RT;
	public Texture ResultTexture
	{
		get{
			return _RT;
		}
	}
	public UIRenderer()
	{
	}

	void _Resize(int w,int h)
	{
		if (_RT == null)
			_RT = new RenderTexture (w,h, 24,RenderTextureFormat.Default);

		if (_RT.width != w || _RT.height != h) {
			_RT = new RenderTexture (w,h, 24,RenderTextureFormat.Default);
		}
	}
	public void Begin(Vector2 size)
	{
		_Resize ((int)size.x,(int)size.y);
		RenderTexture.active = _RT;
		Graphics.SetRenderTarget (_RT);
		GL.Clear (true, true, new Color(0,0,0,0));
	}

	public Texture End()
	{
		RenderTexture.active = null;
		Graphics.SetRenderTarget (null);
		return _RT;
	}
}
