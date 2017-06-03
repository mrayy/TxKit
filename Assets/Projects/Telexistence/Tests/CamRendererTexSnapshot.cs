using UnityEngine;
using System.Collections;

public class CamRendererTexSnapshot : MonoBehaviour {

	public TxKitEyes Source;

	RenderTextureWrapper _wrapper =new RenderTextureWrapper();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyDown (KeyCode.S)) {
			byte[] data= _wrapper.ConvertRenderTexture(Source.CamRenderer [0].GetTexture () as RenderTexture).EncodeToJPG();

			System.IO.File.WriteAllBytes (Application.dataPath + "\\screenshots\\" + Source.gameObject.name + "_SnapShot.jpg",data);
		}
	}
}
