using UnityEngine;
using System.Collections;

public class TestCirclePointGenerator : MonoBehaviour {

	public int Count;
	public Vector3[] points;
	// Use this for initialization
	void Start () {
		points = new Vector3[Count];
		float angle = 0;
		float step = Mathf.Deg2Rad* 360.0f / Count;

		for (int i = 0; i < Count; ++i) {
			points [i] = new Vector3 (Mathf.Cos (angle), 0, Mathf.Sin (angle));
			angle += step;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
