using UnityEngine;
using System.Collections;

public class DisableVRCameraTracking : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.localRotation = Quaternion.identity;
	}

	void OnPostRender()
	{
		transform.localRotation = Quaternion.identity;
	}
}
