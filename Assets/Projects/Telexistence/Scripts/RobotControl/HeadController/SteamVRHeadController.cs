using UnityEngine;
using System.Collections;
using UnityEngine.VR;

#if STEAMVR_ENABLED
public class SteamVRHeadController : IRobotHeadControl {
	Quaternion _initial=Quaternion.identity;
	Vector3 _neckOffset=Vector3.zero;
	SteamVR_Camera _camera;
	public SteamVRHeadController()
	{
		_camera = GameObject.FindObjectOfType<SteamVR_Camera> ();
        /*
		float[] neckOffset = new float[] {
			Ovr.Hmd.OVR_DEFAULT_NECK_TO_EYE_HORIZONTAL,
			Ovr.Hmd.OVR_DEFAULT_NECK_TO_EYE_VERTICAL
		};
		neckOffset= OVRManager.capiHmd.GetFloatArray (Ovr.Hmd.OVR_KEY_NECK_TO_EYE_DISTANCE, neckOffset);
		this._neckOffset = new Vector3 (0, neckOffset [1], neckOffset [0]);
		_neckOffset.y = 0;
		//_neckOffset.z = 0;*/
		Recalibrate ();
	}

	public bool GetHeadOrientation(out Quaternion q, bool abs)
    {
		if (_camera == null) {
			q = Quaternion.identity;
			return false;
		}

	//	q = ts.Orientation.ToQuaternion(false);
		q = _camera.head.localRotation;
		if (!abs) {
			q=q*_initial;
		}

		Quaternion t = q;
		q.x = t.z;
		q.y = -t.y;
		q.z = -t.x;
		return true;
	}
	public bool GetHeadPosition(out Vector3 v,bool abs)
	{

		if (_camera == null) {
			v=Vector3.zero;
			return false;
		}
//		v = OVRManager.display.GetHeadPose (0).position;

		v = _camera.head.localPosition;
		return true;
	}
	
	public void Recalibrate()
	{
		_initial = _camera.head.localRotation;
		_initial=Quaternion.Euler(0, -_initial.eulerAngles.y,0);
	}
}
#endif
