using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotHelper {

	int m_counter=0;
	TextureWrapper m_wrapper=new TextureWrapper();
	public string prefix="image";

	public void TakeScreenshot(Texture tex,string path)
	{
		if (tex == null)
			return;
		Texture2D t=m_wrapper.ConvertTexture (tex);

		System.IO.Directory.CreateDirectory (path);


		byte[] data = t.EncodeToPNG ();
		System.IO.File.WriteAllBytes (path + prefix +m_counter.ToString()+ ".png", data);
		m_counter++;
	}
}
