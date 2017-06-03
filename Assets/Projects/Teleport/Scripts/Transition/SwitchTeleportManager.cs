using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwitchTeleportManager : MonoBehaviour {

	public TxKitEyes Robot1;
	public TxKitEyes Robot2;

	public Material ActiveMaterial;
	public Material RemoteMaterial;

	public ITeleportBehaviour TeleporterDoor;

	public UINEDOInterfaceControls Interface;

	bool _IsPaused=true;

	public AnimationCurve Curve=AnimationCurve.EaseInOut(0,0,1,1);

	public RobotConnectionComponent ActiveRobot
	{
		get
		{
			if(TeleporterDoor.IsActive())
				return Robot2.RobotConnector;
			return Robot1.RobotConnector;
		}
	}

	// Use this for initialization
	void Start () {
		Robot1.ApplyMaterial (ActiveMaterial);
		Robot2.ApplyMaterial (RemoteMaterial);

		TeleporterDoor.OnEntered += OnEntered;
		TeleporterDoor.OnExitted += OnExitted;

		TeleporterDoor.SetAnimationCurve (Curve);

		Robot1.RobotConnector.OnRobotConnected += OnRobotConnected;
		Robot1.RobotConnector.OnRobotDisconnected += OnRobotDisconnected;

		Robot1.RobotConnector.OnRobotStartUpdate += OnRobotStartUpdate;
		Robot1.RobotConnector.OnRobotStopUpdate += OnRobotStopUpdate;
	}

	void OnRobotConnected(RobotInfo ifo, RobotConnector.TargetPorts ports)
	{
		if(Interface!=null)
			Interface.SetPowered(true);
	}
	void OnRobotDisconnected()
	{
		if(Interface!=null)
			Interface.SetPowered(false);
	}

	void OnRobotStartUpdate()
	{
		if(Interface!=null)
			Interface.ConnectScreen.SetConnected(true);
	}
	void OnRobotStopUpdate()
	{
		if(Interface!=null)
			Interface.ConnectScreen.SetConnected(false);
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
			r.StartUpdate(false);
		else
			r.EndUpdate();
	}

	void SwitchTeleport(bool on)
	{
		_IsPaused = false;
		if (on) {
			Robot2.RobotConnector.StartUpdate ();
			Robot2.ResumeVideo ();
			TeleporterDoor.OnEnter (Robot2);
		} else {
			Robot1.RobotConnector.StartUpdate();
			Robot1.ResumeVideo ();
			TeleporterDoor.OnExit();
		}
	}
	
	void OnEntered(ITeleportBehaviour t)
	{
	//	Robot1.RobotConnector.EndUpdate ();
		Robot1.PauseVideo ();
	}
	void OnExitted(ITeleportBehaviour t)
	{
	//	Robot2.RobotConnector.EndUpdate();
		Robot2.PauseVideo ();
	}
	// Update is called once per frame
	void Update () {

		if (Input.GetButtonDown ("ConnectRobot")) {
			Connect (Robot1.RobotConnector);
			Connect (Robot2.RobotConnector);
		}
		if ( Input.GetButtonDown ("StartRobot")) {

			if (_IsPaused) {
				Robot1.RobotConnector.StartUpdate ();
				Robot2.RobotConnector.StartUpdate ();
			} else {
				Robot1.RobotConnector.EndUpdate();
				Robot2.RobotConnector.EndUpdate();
			}

			_IsPaused = !_IsPaused;
			//StartRobot (Robot1.RobotConnector);
			//StartRobot (Robot2.RobotConnector);

		}
		if (Input.GetButtonDown ("CalibrateRobot"))  {
			var b=Robot1.gameObject.GetComponent<TxKitBody> ();
			if (b)
				b.Recalibrate ();
			b=Robot2.gameObject.GetComponent<TxKitBody> ();
			if (b)
				b.Recalibrate ();
		}


		if (Input.GetKeyDown (KeyCode.T)) {
			SwitchTeleport(!TeleporterDoor.IsActive());
		}

	}

}
