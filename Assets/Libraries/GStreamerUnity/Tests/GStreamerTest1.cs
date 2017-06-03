using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class GStreamerTest1 : MonoBehaviour {

	GstCustomTexture m_Texture;

	public Texture2D BlittedImage;

	public Rect BlitRect=new Rect(100,100,100,100);
	public RawImage TargetRenderObject;
	GstImageInfo _img;

	public long position;
	public long duration;
	// Use this for initialization
	void Start () {
		m_Texture = gameObject.GetComponent<GstCustomTexture>();
		return;
		m_Texture.OnFrameGrabbed += OnFrameGrabbed;
		_img = new GstImageInfo ();
		_img.Create (1, 1, GstImageInfo.EPixelFormat.EPixel_R8G8B8);

		BlittedImage = new Texture2D (1, 1);
		BlittedImage.filterMode = FilterMode.Bilinear;
		BlittedImage.anisoLevel=0;
		BlittedImage.wrapMode=TextureWrapMode.Clamp;

		TargetRenderObject.material.mainTexture = BlittedImage;
	}
	void OnFrameGrabbed(GstBaseTexture src,int index)
	{
		//m_Texture.Player.CopyFrame (_img);
		m_Texture.Player.CopyFrameCropped(_img,(int)BlitRect.x,(int)BlitRect.y,(int)BlitRect.width,(int)BlitRect.height);
		_img.BlitToTexture (BlittedImage);
//		Debug.Log(String.Format("Frame Copied {0}x{1}",_img.Width,_img.Height));
	}
	public void OnGUI()
	{
	}
	void OnDestroy()
	{
		if(_img!=null)
			_img.Destory ();
//		Debug.Log ("Destorying Image");
	}
	
	// Update is called once per frame
	void Update () {

		position=m_Texture.Player.GetPosition ()/1000;
		duration=m_Texture.Player.GetDuration ()/1000;

		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			var p = (position - 5000) ;
			if (p < 0)
				p = 0;
			m_Texture.Player.Seek (p * 1000);
		}
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			var p = (position + 5000);
			if (p >=duration)
				p = duration;
			m_Texture.Player.Seek (p * 1000);
		}
		if (Input.GetKeyDown (KeyCode.S))
			m_Texture.Stop ();

		if (Input.GetKeyDown (KeyCode.P))
			m_Texture.Play ();
	}
}
