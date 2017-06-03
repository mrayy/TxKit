using UnityEngine;
using System.Collections;

public class CameraBrightnessController : MonoBehaviour {

	public VideoParametersController VideoParams;

	public float Value=0.5f;
	public float MaxExposure=1;

	protected float _value=0;

	// Use this for initialization
	protected virtual void Start () {
	
		if (VideoParams == null)
			VideoParams=gameObject.GetComponent<VideoParametersController> ();
	}
	
	// Update is called once per frame
	protected virtual void Update () {

		if (Input.GetButtonDown ("BrightnessUp"))
			Value += 0.1f;
		if (Input.GetButtonDown ("BrightnessDown"))
			Value -= 0.1f;

		if (Value != _value) {
			Debug.Log (Value);
			SetBrightness(Value);
		}

	}

	public virtual void SetBrightness(float v)
	{
		if (v > 1)
			v = 1;
		if (v < 0)
			v = -1;
		//Value = v;
		_value = v;
		if (VideoParams != null) {
			VideoParams.ExposureValue = _value>0?(int)(_value*MaxExposure):(int)_value;//*0.6f+0.1f;
		}


	}
	public float GetBrigtness(){
		return Value;
	}
}
