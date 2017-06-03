using UnityEngine;
using System.Collections;
using UnityEngine.VR;

public class KeyboardHeadController : IRobotHeadControl {

	Quaternion _orientation;
	Vector3 _position;

	public KeyboardHeadController()
	{
	}

	public bool GetHeadOrientation(out Quaternion q, bool abs)
    {
		q = _orientation;
		return true;
	}
	public bool GetHeadPosition(out Vector3 v,bool abs)
	{
		_position.x += ((Input.GetKey (KeyCode.RightArrow)?1:0) - (Input.GetKey (KeyCode.LeftArrow)?1:0)) * Time.deltaTime*0.1f;
		_position.z += ((Input.GetKey (KeyCode.UpArrow)?1:0) - (Input.GetKey (KeyCode.DownArrow)?1:0)) * Time.deltaTime*0.1f;
		v = _position;
		return true;
	}
	
	public void Recalibrate()
	{
		_position = Vector3.zero;
	}
}
