using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class webcamTextureTest : MonoBehaviour {

	public WebCamTexture tex;

	public int index;

	// Use this for initialization
	void Start () {

		foreach (var d in WebCamTexture.devices)
			Debug.Log (d.name);

		tex = new WebCamTexture (WebCamTexture.devices [index].name,1280,720);
		tex.Play ();

		this.GetComponent<MeshRenderer> ().material.mainTexture = tex;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
