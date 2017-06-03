using UnityEngine;
using System.Collections;
using UnityEngine.VR;

public class OVRTransformationRemoval : MonoBehaviour {
	public OVRCameraRig Camera;
	public bool Position=false;
	public bool Rotation=false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Rotation)
			transform.localRotation = Quaternion.Inverse(InputTracking.GetLocalRotation (VRNode.Head));
		if(Position)
			transform.localPosition = -Camera.centerEyeAnchor.localPosition;
	}
}
