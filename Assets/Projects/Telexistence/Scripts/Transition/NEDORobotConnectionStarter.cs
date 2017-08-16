using UnityEngine;
using System.Collections;

public class NEDORobotConnectionStarter : MonoBehaviour {
	
	public RobotConnectionComponent Robot;
	public TxKitBody BodyController;
	public UINEDOInterfaceControls Interface;

	string[] captionsJP=new string[]{
		"少々お待ちください。",
		"ヘッドマウントディスプレイをかぶって\nしばらくお待ちください。",
		"接続中．．"
	};
	string[] captions=new string[]{
		"Please Wait",
		"Please put the Head Mounted Display and\nWait to be connected",
		"Connecting.."
	};
	// Use this for initialization
	void Start () {

		Robot.OnRobotConnected += OnRobotConnected;
		Robot.OnRobotDisconnected += OnRobotDisconnected;

		Robot.OnRobotStartUpdate += OnRobotStartUpdate;
		Robot.OnRobotStopUpdate += OnRobotStopUpdate;

		if(Interface!=null)
			Interface.ConnectScreen.SetMessage (captions[0]);
	}

	void OnRobotConnected(RobotInfo ifo, RobotConnector.TargetPorts ports)
	{
		if (Interface != null) {
			Interface.ConnectScreen.SetMessage (captions [1]);
			Interface.SetPowered (true);
			Interface.ConnectScreen.SetBackgroundAlpha (0.95f);
		}
	}
	void OnRobotDisconnected()
	{
		if (Interface != null) {
			Interface.ConnectScreen.SetMessage (captions [0]);
			Interface.SetPowered (false);
			Interface.ConnectScreen.SetBackgroundAlpha (0.95f);
		}
	}

	void OnRobotStartUpdate()
	{
		if (Interface != null) {
			Interface.ConnectScreen.SetMessage (captions [2]);
			Interface.ConnectScreen.SetBackgroundAlpha (1);
		}
	}
	void OnRobotStopUpdate()
	{
		if (Interface != null) {
			Interface.ConnectScreen.SetMessage (captions [1]);
			Interface.ConnectScreen.SetConnected (false);
			Interface.ConnectScreen.SetBackgroundAlpha (0.95f);
		}
	}

	// Update is called once per frame
	void Update () {

		if (Robot.IsRobotConnected && Robot.RobotStatus == IRobotDataCommunicator.ERobotControllerStatus.EConnected) {
			if (Interface != null) {
				Interface.ConnectScreen.SetConnected (true);
			}
		}else 
			if(Interface!=null)
				Interface.ConnectScreen.SetConnected(false);
		
		if (Input.GetButtonDown ("ConnectRobot")) {
			if(!Robot.IsConnected)
			{
				Robot.ConnectRobot();
			}
			else{
				Robot.DisconnectRobot();
			}

		}
		if ( Input.GetButtonDown ("StartRobot")) {
			if(!Robot.IsRobotStarted())
			{
				Robot.StartUpdate();
			}
			else
			{
				Robot.EndUpdate();
			}
		}
		if (Input.GetButtonDown ("CalibrateRobot"))  {
			BodyController.Recalibrate();
		}

	}
}
