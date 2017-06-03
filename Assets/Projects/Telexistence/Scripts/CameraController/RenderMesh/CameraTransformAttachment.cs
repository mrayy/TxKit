using UnityEngine;
using System.Collections;

public class CameraTransformAttachment : MonoBehaviour {

	public Transform attachedAnchor;
	public float speed=500;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (attachedAnchor == null)
			return;

		transform.position = attachedAnchor.position;
		float t =  (speed * Time.deltaTime);
		transform.localRotation = Quaternion.Slerp (transform.localRotation,attachedAnchor.rotation , t);
		//var diff=attachedAnchor.rotation* Quaternion.Inverse(transform.rotation);
		//transform.rotation = Quaternion.RotateTowards(transform.localRotation,attachedAnchor.rotation , t);
	}
}
