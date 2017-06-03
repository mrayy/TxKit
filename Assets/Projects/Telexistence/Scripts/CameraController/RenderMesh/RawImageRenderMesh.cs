using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RawImageRenderMesh : ICameraRenderMesh {
	
	//public Vector2 PixelShift;
	public float fovScaler;
	Texture _RenderedTexture;

	public Texture RenderTex;

	public RawImage Image;

	MeshRenderer _mr;

	OffscreenProcessor _Correction;
	OffscreenProcessor _Blitter;
	// Use this for initialization
	void Start () {
		
		_Correction=new OffscreenProcessor();
		_Correction.ShaderName = "Image/DistortionCorrection";
		_Correction.TargetFormat = RenderTextureFormat.ARGB32;
		_Blitter=new OffscreenProcessor();
		_Blitter.ShaderName = "Image/Blitter";
		_Blitter.TargetFormat = RenderTextureFormat.ARGB32;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void RequestDestroy ()
	{
	}
	void OnDestroy()
	{
		//	Destroy (_RenderPlane);
		CameraPostRenderer r=DisplayCamera.GetComponent<CameraPostRenderer>();
		if(r!=null)
			r.RemoveRenderer (this);
	}

	public override void ApplyMaterial(Material m)
	{
		
		_mr = _RenderPlane.GetComponent<MeshRenderer> ();
		if (_mr != null) {
			Mat=_mr.material=Instantiate(m);
		}else Mat = m;

	}

	public override void Enable()
	{
	}
	public override void Disable()
	{
	}

	public override void CreateMesh(EyeName eye )
	{
		Eye = eye;
		Image.material = Mat;

		CameraPostRenderer r=DisplayCamera.GetComponent<CameraPostRenderer>();
		if(r==null)
		{
			r=DisplayCamera.gameObject.AddComponent<CameraPostRenderer>();
		}
		r.AddRenderer(this);
		_RenderPlane=gameObject;
		Enable ();
	}
	public override void OnPreRender()
	{
		if (CamSource == null)
			return;
		_RenderedTexture = CamSource.GetEyeTexture ((int)Eye);
		Rect texRect = CamSource.GetEyeTextureCoords ((int)Eye);
		if(_RenderedTexture!=null && Mat!=null)
		{/*
			if (_RenderedTexture != null && (_RenderedTexture as Texture2D) != null && (_RenderedTexture as Texture2D).format == TextureFormat.Alpha8) {
				_RenderedTexture = _Processor.ProcessTexture (_RenderedTexture);//CamTexture;//
				texRect = new Rect (0, 0, 1, 1);
			}*/
			if (texRect.x != 0 || texRect.y != 0 ||
				texRect.width != 1 || texRect.height != 1) {
				_Blitter.ProcessingMaterial.SetVector ("TextureRect",new Vector4 (texRect.x, texRect.y, texRect.width, texRect.height));
				_RenderedTexture = _Blitter.ProcessTexture (_RenderedTexture);//CamTexture;//

				texRect = new Rect (0, 0, 1, 1);
			}

				

				
			_Correction.ProcessingMaterial.SetVector("TextureSize",new Vector2(_RenderedTexture.width,_RenderedTexture.height));
			//Vector4 tr=new Vector4 (texRect.x, texRect.y, texRect.width, texRect.height);
			//Mat.SetVector ("TextureRect",tr);


		//	float fovScaler = 1;
			if(Src.Configuration!=null)
			{
				if(Eye==EyeName.LeftEye)
					_Correction.ProcessingMaterial.SetVector("PixelShift",Src.Configuration.CamSettings.PixelShiftLeft);
				else 
					_Correction.ProcessingMaterial.SetVector("PixelShift",Src.Configuration.CamSettings.PixelShiftRight);

				float fov=Src.Configuration.CamSettings.FoV;
				
				float focal = Src.Configuration.CamSettings.Focal;//1;//in meter
				float w1 = 2 * focal*Mathf.Tan(Mathf.Deg2Rad*(Camera.current.fieldOfView*0.5f));
				float w2 = 2 * (focal - Src.Configuration.CamSettings.CameraOffset)*Mathf.Tan(Mathf.Deg2Rad*fov*0.5f);

				if(w1==0)
					w1=1;
				float ratio = w2 / w1;
				
				fovScaler=ratio;
				//				Debug.Log("Configuration Updated");
				_Correction.ProcessingMaterial.SetVector("FocalLength",Src.Configuration.CamSettings.FocalLength);
				_Correction.ProcessingMaterial.SetVector("LensCenter",Src.Configuration.CamSettings.LensCenter);
				
				//	Vector4 WrapParams=new Vector4(Configuration.CamSettings.KPCoeff.x,Configuration.CamSettings.KPCoeff.y,
				//	                               Configuration.CamSettings.KPCoeff.z,Configuration.CamSettings.KPCoeff.w);
				_Correction.ProcessingMaterial.SetVector("WrapParams",Src.Configuration.CamSettings.KPCoeff);
			}else
				_Correction.ProcessingMaterial.SetVector("PixelShift",Vector2.zero);

			_RenderedTexture = _Correction.ProcessTexture (_RenderedTexture);

			Mat.mainTexture = _RenderedTexture;
			Mat.SetTexture ("_MainTex", _RenderedTexture);
			Image.texture = _RenderedTexture;

			float aspect = (float)_RenderedTexture.width / (float)_RenderedTexture.height;
			aspect *= CamSource.GetEyeScalingFactor ((int)Eye).x / CamSource.GetEyeScalingFactor ((int)Eye).y;
			if(aspect==0 || float.IsNaN(aspect))
				aspect=1;
		//	_RenderPlane.transform.localScale = new Vector3 (fovScaler, fovScaler/aspect, 1);

		}
	}
	public override Texture GetTexture()
	{
		return _RenderedTexture;
	}
	public override Texture GetRawTexture()
	{
		if (CamSource == null)
			return null;
		return CamSource.GetRawEyeTexture ((int)Eye);
	}
	/* void OnPostRender()
	{
		
		if (_RenderedTexture != null) {
			GL.PushMatrix();
			GL.LoadOrtho();
			Graphics.DrawTexture(new Rect(0,0,0.5f,0.5f),_RenderedTexture);
			GL.PopMatrix();
		}
	}*/
}
