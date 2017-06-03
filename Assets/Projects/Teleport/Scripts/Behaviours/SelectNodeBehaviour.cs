using UnityEngine;
using System.Collections;

public class SelectNodeBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit ifo;
		if (Physics.Raycast (transform.position, transform.rotation * Vector3.forward, out ifo)) {
			if (ifo.collider.gameObject.tag == "TransitionNode") {
				TransitionNode node = ifo.collider.gameObject.GetComponent<TransitionNode> ();
				Debug.Log (ifo.collider.gameObject.name);
				node.OnTransition ();
			}
		}
	}
}
