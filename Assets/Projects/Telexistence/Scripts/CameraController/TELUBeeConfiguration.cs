using UnityEngine;
using System.Collections;
using System;

public class TELUBeeConfiguration : MonoBehaviour {
	/*
	[Serializable]
	public class CameraSettings
	{
		public string Name;
		public float FoV;
		public float CameraOffset=0;
		public Vector2 LensCenter;
		public Vector2 FocalLength;
		public float K1,K2,P1,P2;
		public Vector2 PixelShiftLeft= Vector2.zero;
		public Vector2 PixelShiftRight= Vector2.zero;
		public float Focal=1;
		public bool Flipped=false;

	}*/

	public CameraConfigurations CamSettings=new CameraConfigurations();

	// Use this for initialization
	void Start () {
	//	if (AppManager.Instance.CamConfigManager.GetCamera (CamSettings.Name) != null)
	//		CamSettings = AppManager.Instance.CamConfigManager.GetCamera (CamSettings.Name);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
