using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultipleTeleportManager : MonoBehaviour {

	public List<TxKitEyes> Robots;
	public int ActiveRobotID;
	int _activeID=-1;

	public Material ActiveMaterial;

	bool _IsPaused=true;

	public RobotConnectionComponent ActiveRobot
	{
		get
		{
			return Robots [ActiveRobotID].RobotConnector;
		}
	}

	// Use this for initialization
	void Start () {
	}


	public void SetActiveRobot(int ID)
	{
		if (_activeID == ID)
			return;
		_activeID = ID;
		ActiveRobotID = ID;
		for (int i = 0; i < Robots.Count; ++i) {
			if (i == ID) {

				Robots [i].RobotConnector.StartUpdate (false);
				Robots [i].ResumeVideo ();
				Robots [i].enabled = true;
			} else {
			//	Robots [i].ApplyMaterial (RemoteMaterial);

				Robots [i].RobotConnector.EndUpdate();
				Robots [i].PauseVideo();
				Robots [i].enabled = false;
			}
		}
	}

	void Connect(RobotConnectionComponent r)
	{
		if(!r.IsConnected)
			r.ConnectRobot();
		else r.DisconnectRobot();
	}

	void StartRobot(RobotConnectionComponent r)
	{
		if(!r.IsRobotStarted())
			r.StartUpdate();
		else
			r.EndUpdate();
	}

	// Update is called once per frame
	void Update () {

		if (Input.GetButtonDown ("ConnectRobot")) {
			for(int i=0;i<Robots.Count;++i)
				Connect (Robots[i].RobotConnector);
			SetActiveRobot (ActiveRobotID);
		}
		if ( Input.GetButtonDown ("StartRobot")) {

			if (_IsPaused) {
				ActiveRobot.StartUpdate (false);
			} else {
				ActiveRobot.EndUpdate();
			}

			_IsPaused = !_IsPaused;
			//StartRobot (Robot1.RobotConnector);
			//StartRobot (Robot2.RobotConnector);

		}
		if (Input.GetButtonDown ("CalibrateRobot"))  {
			for (int i = 0; i < Robots.Count; ++i) {
				var b = Robots [i].gameObject.GetComponent<TxKitBody> ();
				if (b)
					b.Recalibrate ();
			}
		}


		if (Input.GetKeyDown (KeyCode.T)) {
			SetActiveRobot ((ActiveRobotID + 1) % Robots.Count);
		}
	}

}
