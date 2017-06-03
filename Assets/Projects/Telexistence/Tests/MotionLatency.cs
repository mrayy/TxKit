using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MotionLatency : MonoBehaviour {

	public int latency=0;
	int _oldLatency=0;

	List<Quaternion> _frames;
	int _time=0;
	// Use this for initialization
	void Start () {
		_frames = new List<Quaternion> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (latency != _oldLatency) {
			_time = 0;
			_oldLatency = latency;
			_frames.Clear ();
		}
		_frames.Add (Quaternion.Inverse (transform.parent.localRotation));

		if (_time >= latency) {
			transform.localRotation = _frames [0];
			_frames.RemoveAt (0);
		} else
			_time++;
	}
}
