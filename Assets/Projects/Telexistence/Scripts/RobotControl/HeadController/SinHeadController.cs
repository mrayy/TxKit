using UnityEngine;
using System.Collections;

public class SinHeadController : MonoBehaviour,IDependencyNode {

	public class ImplSinHeadController : IRobotHeadControl {

		public Quaternion _orientation;
		Vector3 _position;
		Vector3 _time=Vector3.zero;
		public Vector3 angles;
		public float speedAcc=0;
		public ImplSinHeadController()
		{
		}

		public bool GetHeadOrientation(out Quaternion q, bool abs)
		{
			q = _orientation;
			return true;
		}
		public bool GetHeadPosition(out Vector3 v,bool abs)
		{
			v = _position;
			return true;
		}

		public void Recalibrate()
		{
			_position = Vector3.zero;
			_time = Vector3.zero;
			speedAcc = 0;
		}
		Vector3 limits=new Vector3(70,90,30);
		public void Update(Vector3 freq,float accel)
		{
			_time += speedAcc*freq*Time.deltaTime;
			speedAcc += accel * Time.deltaTime;
			if (speedAcc > 1)
				speedAcc = 1;
			angles.x = limits.x*Mathf.Sin (Mathf.Deg2Rad* _time.x);
			angles.y = limits.y * Mathf.Sin (Mathf.Deg2Rad* _time.y);
			angles.z = limits.z * Mathf.Sin (Mathf.Deg2Rad* _time.z);
			_orientation=Quaternion.Euler(angles);
		}
	}

	ImplSinHeadController _impl;
	RobotConnectionComponent Robot;
	TxKitBody Body;
	public Vector3 Frequency=new Vector3(3,3,3);
	public float TargetAccel=0.15f;
	public float acceleration=0.1f;
	public float speedAcc=0;
	public Vector3 angles;
	public Transform User;
	// Use this for initialization
	void Start () {
		_impl = new ImplSinHeadController ();	
		Robot = GetComponent<RobotConnectionComponent> ();
		Body = GetComponent<TxKitBody> ();
		Robot.AddDependencyNode (this);
	}
	public void OnDependencyStart(DependencyRoot root)
	{
		Body.HeadController = _impl;
	}
	// Update is called once per frame
	void Update () {
		_impl.Update (Frequency,acceleration);
		angles = _impl.angles;
		speedAcc = _impl.speedAcc;
		if(User!=null)
			User.localRotation = _impl._orientation;
		if (Input.GetKeyDown (KeyCode.F5)) {
			if (acceleration == 0)
				acceleration = TargetAccel;
			else {
				acceleration = -0.1f;
			}
		} 

		if (acceleration < 0 && speedAcc < 0.05) {
			acceleration = 0;
			_impl.speedAcc=0;
		}
	}
}
