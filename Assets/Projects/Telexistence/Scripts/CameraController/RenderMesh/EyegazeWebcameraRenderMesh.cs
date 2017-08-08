using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

public class EyegazeWebcameraRenderMesh : ICameraRenderMesh {
	
	//public Vector2 PixelShift;
	public float fovScaler;
	public RenderTexture _RenderedTexture;
	public Texture _CorrectedTexture;
	public RenderTexture _CombinedTexture;
	public Texture _SceneTexture;
	public Texture OriginalTexture;
	public Texture[] _GazeTexture=new Texture[1];

	public Vector4[] EyeGaze;
	public Vector2 SrcEyeGaze;

	OffscreenProcessor _Correction;
	OffscreenProcessor _SrcBlitter;
	Shader _GazeBlitterRect;
	Shader _GazeBlitterCircle;
	OffscreenProcessor _GazeBlitter;
	OffscreenProcessor _SceneBlitter;

	OffscreenProcessor _SceneBlur;

	BlurImageGenerator _blurGenerator;
	Material _gazeBlitMtrl;

	ulong _lastFrame;
	public string frames;

	public Vector2 gazeSize;

	bool m_dirty=false;

	bool m_foveatedStreaming=false;

	public EyegazeDataStreamer DataStreamer;

	public bool IsFoveatedStreaming()
	{
		return m_foveatedStreaming;
	}

	// Use this for initialization
	void Start () {
		
		_Correction=new OffscreenProcessor();
		_Correction.ShaderName = "Image/DistortionCorrection";
		_Correction.TargetFormat = RenderTextureFormat.ARGB32;

		_SrcBlitter=new OffscreenProcessor();
		_SrcBlitter.ShaderName = "Image/Blitter";
		_SrcBlitter.TargetFormat = RenderTextureFormat.Default;
		_SrcBlitter.TargetFormat = RenderTextureFormat.ARGB32;

		_GazeBlitterCircle=Shader.Find("Image/GazeBlit_Circle");

		_GazeBlitterRect=Shader.Find("Image/GazeBlit_Rect");

		_GazeBlitter=new OffscreenProcessor();
		_GazeBlitter.ShaderName = "Image/Blitter";
		_GazeBlitter.TargetFormat = RenderTextureFormat.Default;
		_GazeBlitter.TargetFormat = RenderTextureFormat.ARGB32;
		
		_SceneBlitter=new OffscreenProcessor();
		_SceneBlitter.ShaderName = "Image/Blitter";
		_SceneBlitter.TargetFormat = RenderTextureFormat.Default;
		_SceneBlitter.TargetFormat = RenderTextureFormat.ARGB32;

		_CombinedTexture = new RenderTexture ((int)1, (int)1, 16, RenderTextureFormat.ARGB32);

		_blurGenerator = new BlurImageGenerator ();
		_blurGenerator.DownScaler = 0;
		_blurGenerator.Iterations = 1;

		CamSource.GetBaseTexture ().OnFrameBlitted+= OnFrameGrabbed;

		_GazeTexture = new Texture[0];
	}

	void OnFrameGrabbed(GstBaseTexture src,int index)
	{
		
		//Debug.Log ("A");
		m_dirty = true;
	}
	
	// Update is called once per frame
	void Update () {
	}
	public override void RequestDestroy ()
	{
		Destroy (this);
	}

	void OnDestroy()
	{
		Destroy (_RenderPlane);
	}

	public override void ApplyMaterial(Material m)
	{
		
		MeshRenderer mr = _RenderPlane.GetComponent<MeshRenderer> ();
		if (mr != null) {
			Mat=mr.material=Instantiate(m);
		}else Mat = m;

	}

	public override void Enable()
	{
		if (_RenderPlane == null)
			return;
		MeshRenderer mr = _RenderPlane.GetComponent<MeshRenderer> ();
		if (mr != null) {
			mr.enabled=true;
		}
		this.enabled = true;
	}
	public override void Disable()
	{
		if (_RenderPlane == null)
			return;
		MeshRenderer mr = _RenderPlane.GetComponent<MeshRenderer> ();
		if (mr != null) {
			mr.enabled=false;
		}
		this.enabled = false;
	}

	void _internalCreateMesh(EyeName eye)
	{
		int i = (int)eye;
		if(_RenderPlane==null)
			_RenderPlane = new GameObject("EyesRenderPlane_"+eye.ToString());
		MeshFilter mf = _RenderPlane.GetComponent<MeshFilter> ();
		if (mf == null) {
			mf = _RenderPlane.AddComponent<MeshFilter> ();
		}
		MeshRenderer mr = _RenderPlane.GetComponent<MeshRenderer> ();
		if (mr == null) {
			mr = _RenderPlane.AddComponent<MeshRenderer> ();
		}
		
		mr.material = Mat;
		mf.mesh.vertices = new Vector3[]{
			new Vector3(-1,-1,1),
			new Vector3( 1,-1,1),
			new Vector3( 1, 1,1),
			new Vector3(-1, 1,1)
		};
		Rect r = new Rect(0,0,1,1);// CamSource.GetEyeTextureCoords (eye);
		Vector2[] uv = new Vector2[]{
			new Vector2(r.x,r.y),
			new Vector2(r.x+r.width,r.y),
			new Vector2(r.x+r.width,r.y+r.height),
			new Vector2(r.x,r.y+r.height),
		};
		Matrix4x4 rotMat = Matrix4x4.identity;
		if (Src.Configuration.CamSettings.Rotation [i] == CameraConfigurations.ECameraRotation.Flipped) {
			rotMat = Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (0, 0, 180), Vector3.one);
		} else if (Src.Configuration.CamSettings.Rotation [i] == CameraConfigurations.ECameraRotation.CW) {
			rotMat = Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (0, 0, 90), Vector3.one);
		} else if (Src.Configuration.CamSettings.Rotation [i] == CameraConfigurations.ECameraRotation.CCW) {
			rotMat = Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (0, 0, -90), Vector3.one);
		}
		for(int v=0;v<4;++v)
		{
			Vector3 res=rotMat*(2*uv[v]-Vector2.one);
			uv[v]=(new Vector2(res.x,res.y)+Vector2.one)*0.5f;//Vector2.one-uv[v];
			if(Src.Configuration.CamSettings.FlipXAxis)
			{
				uv[v].x=1-uv[v].x;
			}
			if(Src.Configuration.CamSettings.FlipYAxis)
			{
				uv[v].y=1-uv[v].y;
			}
		//	uv[v].y=1-uv[v].y;
		}
		mf.mesh.uv = uv;
		mf.mesh.triangles = new int[]
		{
			0,2,1,0,3,2
		};
		
		_RenderPlane.transform.localPosition =new Vector3 (0, 0, 0);
		if (Eye == EyeName.LeftEye)
			_RenderPlane.transform.localPosition = new Vector3 (-0.032f, 0, 0);
		else 
			_RenderPlane.transform.localPosition = new Vector3 (0.032f, 0, 0);
		_RenderPlane.transform.localRotation =Quaternion.identity;

		if(DataStreamer!=null)
			DataStreamer.Renderer [(int)eye] = this;
	}
	public override void CreateMesh(EyeName eye )
	{
		Eye = eye;
		MeshRenderer mr = GetComponent<MeshRenderer> ();
		if (mr == null) {
			_internalCreateMesh (eye);
		} else {
			CameraPostRenderer r=DisplayCamera.GetComponent<CameraPostRenderer>();
			if(r==null)
			{
				r=DisplayCamera.gameObject.AddComponent<CameraPostRenderer>();
			}
			r.AddRenderer(this);
			_RenderPlane=gameObject;
			mr.material = Mat;
		}
	}
	Color[] ColorWheel = new Color[] {
		Color.red,
		Color.blue,
		Color.green,
		Color.cyan
	};

	void BlitImage()
	{
		if (CamSource == null || !m_dirty || !DataStreamer)
			return;
		_blurGenerator.Size = DataStreamer.BlurSize;
		m_dirty = false;
		Texture src = CamSource.GetEyeTexture ((int)Eye);
		OriginalTexture = src;
		ulong frame = CamSource.GetGrabbedBufferID ((int)Eye);
		//if (m_dirty) 
		{
			m_dirty = false;
		}
		_lastFrame = frame;

		int levels = CamSource.GetBaseTexture ().GetEyeGazeLevels ();
		m_foveatedStreaming = (levels > 0);
		if (EyeGaze == null || EyeGaze.Length != levels)
			EyeGaze = new Vector4[levels];
		for (int i = 0; i < levels; ++i) {
			EyeGaze[i] = CamSource.GetBaseTexture ().GetEyeGaze (0, i);

		}
		if (DataStreamer.frameSize.x != _CombinedTexture.width ||
		   DataStreamer.frameSize.y != _CombinedTexture.height) {
			_CombinedTexture = new RenderTexture ((int)DataStreamer.frameSize.x, (int)DataStreamer.frameSize.y, 16, RenderTextureFormat.Default);
		}

		ulong frame2 = CamSource.GetGrabbedBufferID ((int)Eye);
		if (frame != frame)
			Debug.LogWarning ("Image frames changed!");

		Rect texRect = CamSource.GetEyeTextureCoords ((int)Eye);
		if(src!=null && Mat!=null)
		{/*
			if (_RenderedTexture != null && (_RenderedTexture as Texture2D) != null && (_RenderedTexture as Texture2D).format == TextureFormat.Alpha8) {
				_RenderedTexture = _Processor.ProcessTexture (_RenderedTexture);//CamTexture;//
				texRect = new Rect (0, 0, 1, 1);
			}*/
			//if (texRect.x != 0 || texRect.y != 0 ||
			//	texRect.width != 1 || texRect.height != 1) 
			{
				_SrcBlitter.ProcessingMaterial.SetVector ("TextureRect", new Vector4 (texRect.x, texRect.y, texRect.width, texRect.height));

				src = _SrcBlitter.ProcessTextureSized (src,(int)(texRect.width*src.width),(int)(texRect.height*src.height));//CamTexture;//

				texRect = new Rect (0, 0, 1, 1);
			} 
			/*
			//blit scene texture
			_SceneBlitter.ProcessingMaterial.SetVector ("TextureRect", new Vector4 (0, 0, texRect.width, ((float)src.height-256.0f)/(float)src.height));
			_RenderedTexture = _SceneBlitter.ProcessTextureSized (src,256,src.height-256);//CamTexture;//

			//blit gaze texture
			_GazeBlitter.ProcessingMaterial.SetVector ("TextureRect", new Vector4 (0, ((float)src.height-256.0f)/(float)src.height, texRect.width, 256.0f/(float)src.height));
			_GazeTexture = _GazeBlitter.ProcessTextureSized (src,256,256);//CamTexture;//
			*/

			gazeSize.x = src.height;
			gazeSize.y = src.height;

			//Get Foveation levels
			if (levels > 0) {
				//blit scene texture
				_SceneBlitter.ProcessingMaterial.SetVector ("TextureRect", new Vector4 ((gazeSize.x * levels) / (float)src.width, 0, ((float)src.width - gazeSize.x * levels) / (float)src.width, texRect.height));
				_SceneTexture = _SceneBlitter.ProcessTextureSized (src, (int)(src.width - gazeSize.x * levels), src.height);//CamTexture;//
				if (true) {
					RenderTexture.active = _CombinedTexture;
					GL.Clear (true, true, Color.black);
					GL.PushMatrix ();
					GL.LoadPixelMatrix (0, _CombinedTexture.width, _CombinedTexture.height, 0);

					Graphics.DrawTexture (new Rect (0, 0, DataStreamer.frameSize.x, DataStreamer.frameSize.y), _SceneTexture);


					if (_gazeBlitMtrl == null) {
						_gazeBlitMtrl = new Material (_GazeBlitterCircle);
						_gazeBlitMtrl.hideFlags = HideFlags.DontSave;
					}

					var blitter = _GazeBlitterCircle;

					switch (DataStreamer.GazeBlitMethod) {
					case EyegazeDataStreamer.GazeBlitType.Circular:
						blitter = _GazeBlitterCircle;
						break;
					case EyegazeDataStreamer.GazeBlitType.Rectangular:
						blitter = _GazeBlitterRect;
						break;
					}
					_gazeBlitMtrl.shader = blitter;

					if (_GazeTexture.Length != levels)
						_GazeTexture = new Texture[levels];
					//blit all gaze levels
					for (int i = levels - 1; i >= 0; --i) {
						if (DataStreamer.DisabledFoveations [i])
							continue;
						//blit gaze texture
						_GazeBlitter.ProcessingMaterial.SetVector ("TextureRect", new Vector4 (gazeSize.x * i / (float)src.width, 0, gazeSize.x / (float)src.width, texRect.height));
						_GazeTexture [i] = _GazeBlitter.ProcessTextureSized (src, (int)gazeSize.x, (int)gazeSize.y);//CamTexture;//

						var clr=ColorWheel [i % ColorWheel.Length];

						_gazeBlitMtrl.SetFloat ("DebugBlitArea", DataStreamer.DebugBlitArea ? 1 : 0);
						_gazeBlitMtrl.SetVector ("_DebugColor", new Vector4 (clr.r,clr.g,clr.b,clr.a));
						_gazeBlitMtrl.SetVector ("_Parameters", new Vector4 (0.5f, 0.5f, DataStreamer.GazeArea.x, DataStreamer.GazeArea.y));
				
						Graphics.DrawTexture (new Rect (EyeGaze[i].x, EyeGaze[i].y, EyeGaze[i].z, EyeGaze[i].w), _GazeTexture [i], _gazeBlitMtrl);

					}

					if (DataStreamer.DrawRectangle) {
						for (int i = 0; i < levels; ++i) {

							if (DataStreamer.DisabledFoveations [i])
								continue;
							var r=new Rect (EyeGaze [i].x, EyeGaze [i].y, EyeGaze [i].z, EyeGaze [i].w);
							if(DataStreamer.GazeBlitMethod== EyegazeDataStreamer.GazeBlitType.Rectangular)
								GUITools.GraphicsDrawScreenRectBorder (r, 4, ColorWheel [i % ColorWheel.Length]);
							else 
								GUITools.DrawCircle (r.center,r.height/2, 90, ColorWheel [i % ColorWheel.Length],3);
						}
					}
					GL.PopMatrix ();
					RenderTexture.active = null;
				}else
					_CombinedTexture=_SceneTexture as RenderTexture;
			} else {
				_SceneBlitter.ProcessingMaterial.SetVector ("TextureRect", new Vector4 (0, 0,1,1));
				_SceneTexture = _SceneBlitter.ProcessTextureSized (src, src.width , src.height);//CamTexture;//

				_CombinedTexture=_SceneTexture as RenderTexture;
			}

			_Correction.ProcessingMaterial.SetVector("TextureSize",new Vector2(_CombinedTexture.width,_CombinedTexture.height));
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
				float camfov=Camera.current.fieldOfView;
				float w1 = 2 * focal*Mathf.Tan(Mathf.Deg2Rad*(camfov*0.5f));
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

			_CorrectedTexture = _Correction.ProcessTexture (_CombinedTexture);

			if ( Src.Effects != null) {
				Texture tex = _CorrectedTexture;
				foreach (var e in Src.Effects) {
					e.ProcessTexture (tex, ref _RenderedTexture);
					tex = _RenderedTexture;
					//	_GazeTexture = e.ProcessTexture (_GazeTexture);
				}
			} else {
				_RenderedTexture = _CorrectedTexture as RenderTexture;
			}

			Mat.mainTexture = _RenderedTexture;

		}
	}
	public override void OnPreRender()
	{
		BlitImage ();

		if (_RenderedTexture != null ) {
			float aspect = (float)_RenderedTexture.width / (float)_RenderedTexture.height;
			Vector2 sfactor = CamSource.GetEyeScalingFactor ((int)Eye);
			//sfactor.x = sfactor.x - (float)(_RenderedTexture.width)/(float)src.width;
			//aspect *= sfactor.x / sfactor.y;
			if (aspect == 0 || float.IsNaN (aspect))
				aspect = 1;
			_RenderPlane.transform.localScale = new Vector3 (fovScaler, fovScaler / aspect, 1);
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
	/*
	void OnPostRender()
	{
		if (_RenderedTexture != null) {
			GL.PushMatrix();
			GL.LoadOrtho();
			Graphics.DrawTexture(new Rect(0,0,0.5f,0.5f),_RenderedTexture);
			GL.PopMatrix();
		}
	}*/
}
