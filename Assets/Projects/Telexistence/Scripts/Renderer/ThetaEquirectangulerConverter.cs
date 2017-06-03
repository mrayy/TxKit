using UnityEngine;
using System.Collections;

public class ThetaEquirectangulerConverter {

	Texture _resultImage;

	public Texture ProcessedImage{
		get{return _resultImage;}
	}

	OffscreenProcessor _RicohProcessor;
	OffscreenProcessor _BlitterProcessor;
	public Vector4 uvs=new Vector4(0,0,0,0);
	public float radius=0.445f;

	public ThetaEquirectangulerConverter()
	{
		_RicohProcessor = new OffscreenProcessor ();
		_RicohProcessor.ShaderName = "Theta/RealtimeEquirectangular";
		_BlitterProcessor = new OffscreenProcessor ();
		_BlitterProcessor.ShaderName = "Image/Blitter";
	}

	public Texture ProcessImage(Texture image,bool flip=true)
	{
		_RicohProcessor.ProcessingMaterial.SetVector ("_UVOffset", uvs);
		_RicohProcessor.ProcessingMaterial.SetFloat ("_Radius", radius);
		if (flip)
			_resultImage = _BlitterProcessor.ProcessTexture (image);
		else
			_resultImage = image;
		_resultImage = _RicohProcessor.ProcessTexture (_resultImage);
		return _resultImage;
	}
}
