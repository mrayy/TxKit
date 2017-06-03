using UnityEngine;
using System.Collections;
using System.Threading;

public class OVRVisionRenderMesh : ICameraRenderMesh {
	
	public RenderTexture _RenderedTexture;

	public string frames;
	ulong _lastFrame;
	OffscreenProcessor _Blitter;
	// Use this for initialization
	void Start () {

		_Blitter=new OffscreenProcessor();
		_Blitter.ShaderName = "Image/Blitter";
		_Blitter.TargetFormat = RenderTextureFormat.Default;
		_Blitter.TargetFormat = RenderTextureFormat.ARGB32;
		_Blitter.ProcessingMaterial.SetInt("flipImage", 1);
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
			_RenderPlane.transform.localPosition = new Vector3 (-0.032f, 0.0f, 0);
		else 
			_RenderPlane.transform.localPosition = new Vector3 (0.032f, -0.0f, 0);
		_RenderPlane.transform.localRotation =Quaternion.Euler(0,0,0);
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
	public override void OnPreRender()
	{
		if (CamSource == null)
			return;
		
		Texture src = CamSource.GetEyeTexture ((int)Eye);
		_lastFrame = CamSource.GetGrabbedBufferID ((int)Eye);
		if(src!=null && Mat!=null)
		{
			Vector2 offset=Vector2.zero;
			if (Eye == EyeName.LeftEye)
				offset = Src.Configuration.CamSettings.PixelShiftLeft;
			else
				offset = Src.Configuration.CamSettings.PixelShiftRight;

			//if (offset != Vector2.zero) 
			if(true)
			{
				_Blitter.ProcessingMaterial.SetVector ("PixelShift", offset);

				src = _Blitter.ProcessTexture (src);
			}

			Texture resultTex;
			if (Src.Effects != null && Src.Effects.Length>0) {
				Texture tex = src;
				foreach (var e in Src.Effects) {
					e.ProcessTexture (tex, ref _RenderedTexture);
					tex = _RenderedTexture;
					//	_GazeTexture = e.ProcessTexture (_GazeTexture);
				}
				resultTex = _RenderedTexture;
			}else
				resultTex = src;	
			Mat.mainTexture = resultTex;

			float fov=Src.Configuration.CamSettings.FoV;

			float focal = Src.Configuration.CamSettings.Focal;//1;//in meter
			float camfov=Camera.current.fieldOfView;
			float w1 = 2 * focal*Mathf.Tan(Mathf.Deg2Rad*(camfov*0.5f));
			float w2 = 2 * (focal - Src.Configuration.CamSettings.CameraOffset)*Mathf.Tan(Mathf.Deg2Rad*fov*0.5f);

			if(w1==0)
				w1=1;
			float ratio = w2 / w1;

			float aspect = (float)resultTex.width / (float)resultTex.height;
			aspect *= CamSource.GetEyeScalingFactor ((int)Eye).x / CamSource.GetEyeScalingFactor ((int)Eye).y;
			if(aspect==0 || float.IsNaN(aspect))
				aspect=1;
			_RenderPlane.transform.localScale = new Vector3 (ratio, ratio/aspect, 1);

		}
		//if (CamSource != null)
		//	frames+=CamSource.GetGrabbedBufferID ((int)Eye)+"\t";
	}
	public override void OnPostRender()
	{
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

}
