using UnityEngine;
using System.Collections;

public class RobotConnectionStarter : MonoBehaviour {
	
	public RobotConnectionComponent Robot;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetButtonDown ("ConnectRobot")) {
			if(!Robot.IsConnected)
				Robot.ConnectRobot();
			else Robot.DisconnectRobot();

		}
		if ( Input.GetButtonDown ("StartRobot")) {
			if(!Robot.IsRobotStarted())
				Robot.StartUpdate();
			else
				Robot.EndUpdate();
		}
		if (Input.GetButtonDown ("CalibrateRobot"))  {
			var b=Robot.GetComponent<TxKitBody> ();
			if(b)
				b.Recalibrate();
		}

	}
}
