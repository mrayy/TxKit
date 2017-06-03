using UnityEngine;
using System.Collections;

public class ThetaEquirectangularImageGenerator:MonoBehaviour  {


	OffscreenProcessor _processor;


	WebCamTexture _srcTexture;

	public Texture Result;
	// Use this for initialization
	void Start () {
		_srcTexture = new WebCamTexture ();
		_srcTexture.deviceName = WebCamTexture.devices [0].name;
		_srcTexture.Play ();

		_processor = new OffscreenProcessor ();
		_processor.ShaderName = "Theta/RealtimeEquirectangular";

	}
	
	// Update is called once per frame
	void Update () {

		Result=_processor.ProcessTexture (_srcTexture);

		this.GetComponent<MeshRenderer> ().material.mainTexture = _srcTexture;//Result;
	}
}
