using UnityEngine;
using System.Collections;

public class LeapMotionRenderer : MonoBehaviour {

	public Material HandsMaterial;

	LeapImageRetriever[] _retrivals=new LeapImageRetriever[2];

	GameObject _Hands;
	public bool IsActive=true;
	public float SizeFactor=1.0f;
	public bool IsStereo = true;
	HandController _handController;

	public LeapImageRetriever[] LeapRetrival
	{
		get
		{
			return _retrivals;
		}
	}
	// Use this for initialization
	void Start () {

		OVRCameraRig OculusCamera = gameObject.GetComponent<OVRCameraRig>();

		_handController= gameObject.AddComponent<HandController> ();
		_handController.isHeadMounted = true;
		_handController.destroyHands = true;


		LeapImageRetriever.EYE[] eyes = new LeapImageRetriever.EYE[]{LeapImageRetriever.EYE.RIGHT,IsStereo? LeapImageRetriever.EYE.LEFT:LeapImageRetriever.EYE.RIGHT};
		if (OculusCamera != null) {
			Camera[] cams = new Camera[] { OculusCamera.rightEyeCamera, OculusCamera.leftEyeCamera };
			
			for (int i = 0; i < cams.Length; ++i)
			{
				LeapImageRetriever lret=cams[i].gameObject.AddComponent<LeapImageRetriever>();
				lret.eye=eyes[i];
				lret.syncMode=LeapImageRetriever.SYNC_MODE.LOW_LATENCY;
				lret.gammaCorrection=1.0f;

				_retrivals[i]=lret;

				cams[i].gameObject.AddComponent<EnableDepthBuffer>();
			}

			//OculusCamera.centerEyeAnchor.gameObject;

			GameObject HandsRenderer=GameObject.CreatePrimitive(PrimitiveType.Quad);
			HandsRenderer.name="LeapMotionHandsRenderer";
			HandsRenderer.transform.parent=OculusCamera.centerEyeAnchor.transform;
			HandsRenderer.transform.localPosition=new Vector3(0,0,0.137f);
			HandsRenderer.transform.localRotation=Quaternion.identity;
			LeapImageBasedMaterial lmat=HandsRenderer.AddComponent<LeapImageBasedMaterial>();
			lmat.imageMode=IsStereo?LeapImageBasedMaterial.ImageMode.STEREO:LeapImageBasedMaterial.ImageMode.RIGHT_ONLY;
			HandsRenderer.GetComponent<MeshRenderer>().material=HandsMaterial;
			_Hands=HandsRenderer;
			_Hands.GetComponent<MeshRenderer>().enabled=IsActive;
		}


	}
	
	// Update is called once per frame
	void Update () {
		if (_Hands == null)
			return;
		if (_handController == null) {
			_Hands.GetComponent<MeshRenderer>().enabled=false;
			return;
		}

		_Hands.transform.localScale = new Vector3 (SizeFactor, SizeFactor, SizeFactor);
		if (_handController.IsConnected() == false) {
			_Hands.GetComponent<MeshRenderer>().enabled=false;
			return;
		}
		if (Input.GetButtonDown ("Hands")) {
			IsActive=!IsActive;
		}
		if (_Hands.GetComponent<MeshRenderer> ().enabled != IsActive) {
			_Hands.GetComponent<MeshRenderer> ().enabled = IsActive;
		}
		if (Input.GetKeyDown (KeyCode.F5)) {
			System.IO.File.WriteAllBytes (Application.dataPath + "\\screenshots\\hands.png", _retrivals [0].MainTexture.EncodeToPNG());
		}
	}
}
