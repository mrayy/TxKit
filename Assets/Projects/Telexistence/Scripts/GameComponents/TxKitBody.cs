using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TxKitBody : MonoBehaviour,IDependencyNode {
	public RobotConnectionComponent RobotConnector;
	public bool NullValues;

	public AppManager.HeadControllerType HeadControllerType;
	public AppManager.BaseControllerType BaseControllerType;

	public IRobotHeadControl HeadController;
	public IRobotBaseControl BaseController;

	IRobotCommunicator RobotCommunicator;

	public Vector3 HeadPosition;
	public Quaternion HeadOrientation;

	public Vector2 BaseSpeed;
	public float BaseRotation;

	public bool SupportBase = true;

	public  void OnDependencyStart(DependencyRoot root)
	{
		if (root == RobotConnector) {
			RobotConnector.OnRobotConnected += OnRobotConnected;
			RobotConnector.OnRobotDisconnected+=OnRobotDisconnected;
			RobotConnector.OnRobotStartUpdate+=OnRobotStartUpdate;
			RobotConnector.OnRobotStopUpdate+=OnRobotStopUpdate;
		}
	}
	// Use this for initialization
	void Start () {

		if (RobotConnector == null)
			RobotConnector=gameObject.GetComponent<RobotConnectionComponent> ();

		switch (HeadControllerType) {
		case AppManager.HeadControllerType.Oculus:
			HeadController = new OculusHeadController ();
			break;
			#if STEAMVR_ENABLED
			case AppManager.HeadControllerType.SteamVR:
			HeadController = new SteamVRHeadController ();
			break;
			#endif
		case AppManager.HeadControllerType.Keyboard:
			HeadController=new KeyboardHeadController();
			break;
		default:
			HeadController=new OculusHeadController();
			break;
		}
		switch (BaseControllerType) {
		case AppManager.BaseControllerType.None:
			BaseController = null;
			break;
		case AppManager.BaseControllerType.Oculus:
		default:
			BaseController=new OculusBaseController();
			break;
		}


		RobotConnector.AddDependencyNode (this);


	}

	void OnEnable()
	{
	}


	void OnDisable()
	{
	}

	void OnRobotConnected(RobotInfo ifo,RobotConnector.TargetPorts ports)
	{
		RobotCommunicator = RobotConnector.Connector.RobotCommunicator;
	}
	void OnRobotDisconnected()
	{
		RobotCommunicator = null;
	}

	void OnRobotStartUpdate()
	{
		Recalibrate();
	}
	void OnRobotStopUpdate()
	{
		RobotCommunicator.SetData ("HeadPosition", Vector3.zero.ToExportString (), false);
		RobotCommunicator.SetData("HeadRotation", Quaternion.identity.ToExportString(), false);
		RobotCommunicator.SetData("Speed", Quaternion.identity.ToExportString(), false);
		RobotCommunicator.SetData ("Rotation", "0", false);
	}

	// Update is called once per frame
	void FixedUpdate () {

		if (RobotConnector.IsRobotConnected) {
			
			if (NullValues) {
				HeadOrientation = Quaternion.identity;
				HeadPosition = Vector3.zero;
				BaseSpeed = Vector2.zero;
				BaseRotation = 0;
			} else {
				HandleController ();
			}
			if (HeadController != null) {
				RobotCommunicator.SetData ("HeadRotation", HeadOrientation.ToExportString (), false);
				RobotCommunicator.SetData ("HeadPosition", HeadPosition.ToExportString (), false);
			}
			if (BaseController != null) {
				RobotCommunicator.SetData ("Speed", BaseSpeed.ToExportString (), false);
				RobotCommunicator.SetData ("Rotation", BaseRotation.ToString ("f6"), false);
			}
		}
	}


	void HandleController()
	{
		if (BaseController != null) {
			BaseSpeed = BaseController.GetSpeed ();
			if (BaseSpeed.x < 0)
				BaseSpeed.x *= 0.1f;
			BaseRotation = BaseController.GetRotation ();
		}
		if (HeadController!=null) {
			HeadController.GetHeadOrientation(out HeadOrientation, false);
			HeadController.GetHeadPosition(out HeadPosition, false);
		}
	}


	public void Recalibrate()
	{
		if (BaseController != null) {
			BaseController.Recalibrate();
		}
		if (HeadController!=null) {
			HeadController.Recalibrate();
		}
	}
}
