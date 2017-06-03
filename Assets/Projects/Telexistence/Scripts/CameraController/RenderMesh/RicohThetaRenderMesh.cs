using UnityEngine;
using System.Collections;

public class RicohThetaRenderMesh : ICameraRenderMesh {



	Texture _RenderedTexture;
	ThetaEquirectangulerConverter _equirectanguler;

	public Vector4 uvs=new Vector4(0,0,0,0);
	public float radius=0.445f;
	float Scaler=50;
	// Use this for initialization
	void Start () {
		_equirectanguler = new ThetaEquirectangulerConverter();

		/*OVRTransformationRemoval tr= _RenderPlane.AddComponent<OVRTransformationRemoval> ();
		tr.Camera = Src.OculusCamera;
		tr.Rotation = true;*/


	}

	void OnDestroy()
	{
		Destroy (_RenderPlane);
	}
	public override void RequestDestroy ()
	{
		Destroy (this);
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
			_RenderPlane = new GameObject("RicohThetaMesh_" + eye.ToString ());
		MeshFilter mf = _RenderPlane.GetComponent<MeshFilter> ();
		if (mf == null) {
			mf = _RenderPlane.AddComponent<MeshFilter> ();
		}
		MeshRenderer mr = _RenderPlane.GetComponent<MeshRenderer> ();
		if (mr == null) {
			mr = _RenderPlane.AddComponent<MeshRenderer> ();
		}
		mf.mesh = MeshGenerator.GenerateSphere (1, 40, 40);

		mr.material = Mat;
		transform.localPosition =new Vector3 (0, 0, 0);
		transform.localRotation =Quaternion.identity;
		transform.localScale = new Vector3 (Scaler, Scaler, Scaler);
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
		/*if (eye == EyeName.LeftEye)
			_RenderPlane.transform.localPosition = new Vector3 (-0.03f, 0, 0);
		else 
			_RenderPlane.transform.localPosition = new Vector3 (0.03f, 0, 0);*/
	}


	public override void OnPreRender()
	{
		_RenderedTexture = CamSource.GetEyeTexture (0);
		if(_RenderedTexture!=null && Mat!=null)
		{
			/*if(_RenderedTexture!=null && (_RenderedTexture as Texture2D)!=null && (_RenderedTexture as Texture2D).format==TextureFormat.Alpha8)
				_RenderedTexture=_Processor.ProcessTexture(_RenderedTexture);//CamTexture;//*/
			_equirectanguler.uvs = uvs;
			_equirectanguler.radius = radius;
			_RenderedTexture = _equirectanguler.ProcessImage (_RenderedTexture,Src.Configuration.CamSettings.FlipYAxis);

			Mat.mainTexture=_RenderedTexture;

		}


		Quaternion qp = Quaternion.Euler (Src.Configuration.CamSettings.OffsetAngle);
		_RenderPlane.transform.localRotation = Quaternion.Inverse(qp)* Quaternion.Inverse (DisplayCamera.transform.localRotation)*qp;
	}

	public override Texture GetTexture()
	{
		return _RenderedTexture;
	}

	public override Texture GetRawTexture()
	{
		return  CamSource.GetEyeTexture (0);
	}
}
