using UnityEngine;
using System.Collections;
using System;

public class Support_KeyInputDetector : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		var k = Enum.GetValues (typeof(KeyCode));
		foreach (var c in k) {
			KeyCode ck = (KeyCode)c;
			if (Input.GetKeyDown (ck))
				Debug.Log (ck);
		}
	}
}
