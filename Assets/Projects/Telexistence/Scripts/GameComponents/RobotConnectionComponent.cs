using UnityEngine;
using System.Collections;
using System.IO;

public class RobotConnectionComponent : DependencyRoot {

	RobotConnector _connector;

	//public TxKitEyes VideoStream;
	public DebugInterface Debugger;

	bool _connected=false;
	public int RobotIndex;
	public string RobotName;

	//incase we using PLC lock
	public PLCDriverObject PLCDriverObject;

	RobotInfo _RobotIP;
	public RobotInfo RobotIP
	{
		get{
			return _RobotIP;
		}
	}

	float[] _RobotJointValues;


	public delegate void Delg_OnRobotConnected(RobotInfo ifo, RobotConnector.TargetPorts ports);
	public event Delg_OnRobotConnected OnRobotConnected;
	
	public delegate void Delg_OnRobotDisconnected();
	public event Delg_OnRobotDisconnected OnRobotDisconnected;


	public delegate void Delg_OnRobotStartUpdate();
	public event Delg_OnRobotStartUpdate OnRobotStartUpdate;

	public delegate void Delg_OnRobotStopUpdate();
	public event Delg_OnRobotStopUpdate OnRobotStopUpdate;

	public delegate void Delg_OnRobotUpdate(RobotConnector connector);
	public event Delg_OnRobotUpdate OnRobotUpdate;

	public delegate void Delg_OnServiceNetValue(string serviceName,int port);
	public event Delg_OnServiceNetValue OnServiceNetValue;

	IRobotDataCommunicator.ERobotControllerStatus _robotStatus=IRobotDataCommunicator.ERobotControllerStatus.EStopped;
	public IRobotDataCommunicator.ERobotControllerStatus RobotStatus
	{
		get{
			return _robotStatus;
		}
	}
	
	public bool IsConnected
	{
		get
		{
			return _connected;
		}
	}
	public bool IsRobotConnected
	{
		get
		{
			return Connector.IsRobotConnected;
		}
	}
	
	public RobotConnector Connector
	{
		get{
			return _connector;
		}
	}
	public float[] RobotJointValues {
		get {
			return _RobotJointValues;
		}
	}


	void OnHMDUnmounted()
	{
		EndUpdate ();
	}
	// Use this for initialization
	void Start () {
		AppManager.Instance.Init ();
		Settings.Instance.Init ();
		_connector = new RobotConnector ();
		_connector.DataCommunicator.OnMessage += OnMessage;
		_connector.DataCommunicator.OnRobotInfoDetected += OnRobotInfoDetected;
		_connector.DataCommunicator.OnRobotStatus += OnRobotStatus;
		_connector.DataCommunicator.OnJointValues += OnJointValues;
		_connector.DataCommunicator.OnBumpSensor += OnBumpSensor;
		_connector.DataCommunicator.OnServiceNetValue += _OnServiceNetValue;

		_connector.StartDataCommunicator ();

		OVRManager.HMDUnmounted += OnHMDUnmounted;

		if(RobotIndex>=0)
			_RobotIP=AppManager.Instance.RobotManager.GetRobotInfo(RobotIndex);



		//Send Detect Message to scan network for the available robots
		/*
		_connector.RobotCommunicator.SetData ("detect", _connector.DataCommunicator.GetPort().ToString(), true,false);
		_connector.RobotCommunicator.BroadcastMessage (Settings.Instance.GetPortValue("CommPort",6000));
		_connector.RobotCommunicator.RemoveData ("detect");
*/
		if (Debugger == null)
			Debugger = GameObject.FindObjectOfType<DebugInterface> ();
		if (Debugger != null) {
			Debugger.AddDebugElement(new DebugRobotStatus(this));
		}

		//VideoStream.SetConnectionComponent (this);

		_OnStarted ();
	}
	void OnMessage(int message,BinaryReader reader)
	{
	//	Debug.Log ("Message Arrived: " + message.ToString());
	}

	void OnJointValues(float[] values)
	{
		_RobotJointValues = values;
		/*
		if (PLCDriverObject != null) {
			PLCDriverObject.OnTorsoJointValues(values);
		}*/
	}

	void OnRobotStatus(IRobotDataCommunicator.ERobotControllerStatus status)
	{
		_robotStatus = status;

		if (PLCDriverObject != null) {
			PLCDriverObject.OnRobotStatus(status);
		}
	}
	void OnRobotInfoDetected(RobotInfo ifo)
	{
		Debug.Log ("Robot detected: " + ifo.IP);
		if(_RobotIP==null)
			_RobotIP = ifo;
	}
	void OnBumpSensor(float[] v)
	{
	}
	
	public void OnCameraFPS(int c0,int c1)
	{
		if (PLCDriverObject != null) {
			PLCDriverObject.OnCameraFPS(c0,c1);
		}
	}
	public void _OnServiceNetValue(string serviceName,int port)
	{
		Debug.Log (serviceName + ":" + port);
		if(OnServiceNetValue!=null)
			OnServiceNetValue(serviceName,port);
	}
	void OnDestroy()
	{
		DisconnectRobot ();
		if(_connector!=null)
			_connector.Dispose();
		_connector = null;
	}

	// Update is called once per frame
	void Update () {
		if(_connector!=null)
		if (IsConnected) {
			_connector.SendData("query","",false);
			_connector.SendData("jointVals","",false);

			if (_connector.IsRobotConnected) {
				if (OnRobotUpdate != null)
					OnRobotUpdate (_connector);
			}
		}
	}

	void FixedUpdate()
	{
		if(_connector!=null)
			_connector.Update ();
	}
	
	public void ConnectRobot()
	{
		if (_RobotIP == null || _RobotIP.IP=="") {
			if (RobotIndex != -1)
				_RobotIP = AppManager.Instance.RobotManager.GetRobotInfo (RobotIndex);
			else if(RobotName != "")
				_RobotIP = AppManager.Instance.RobotManager.GetRobotInfoByName (RobotName);
		}
		RobotInfo ifo=_RobotIP;
		ConnectRobot(ifo);
	}
	public void ConnectRobot(RobotInfo r)
	{
		if (_connected) {
			if(r.IP==_RobotIP.IP)
				return;
			DisconnectRobot ();
		}
		_RobotIP = r;

		if (_RobotIP == null) {
			Debug.Log ("No robot is set to connect to!");
			return;
		}

		if (_connector.RobotCommunicator != null) {
			_connector.RobotCommunicator.Disconnect ();
		}

		if (r.ConnectionType == RobotInfo.EConnectionType.WebRTC) {
			_connector.RobotCommunicator = new WebRTCRobotCommunicator ();
			_connector.RobotCommunicator._ownerConnection = this;
		} else if (r.ConnectionType == RobotInfo.EConnectionType.RTP ||
			r.ConnectionType == RobotInfo.EConnectionType.Ovrvision) {
			_connector.RobotCommunicator = new RTPRobotCommunicator ();
			_connector.RobotCommunicator._ownerConnection = this;
		}

		RobotConnector.TargetPorts ports = new RobotConnector.TargetPorts ();

		_RobotIP.communicationPort= ports.CommPort = Settings.Instance.GetPortValue ("CommPort", 6000);//_RobotIP.communicationPort;//
		ports.RobotIP = _RobotIP.IP;
		_connector.ConnectRobot (_RobotIP);
		_connected=true;
/*
		if (VideoStream != null) {
			VideoStream.SetRemoteHost(_RobotIP.IP,ports);
		}*/

		if (OnRobotConnected!=null) {
			OnRobotConnected(r,ports);
		}

		

		LogSystem.Instance.Log ("Connecting to Robot:" + _RobotIP.Name, LogSystem.LogType.Info);
	}

	public void DisconnectRobot()
	{
		if (!_connected)
			return;
		_connector.DisconnectRobot ();
		if (OnRobotDisconnected != null) {
			OnRobotDisconnected();
		}

		if (_connector.RobotCommunicator!=null) {
			_connector.RobotCommunicator.Disconnect ();
			_connector.RobotCommunicator = null;
		}
		_connected = false;
		LogSystem.Instance.Log ("Disconnecting Robot:" + _RobotIP.Name, LogSystem.LogType.Info);
	}

	public void StartUpdate(bool recalibrate=true)
	{
		if (!_connected) 
			return;
		_connector.StartUpdate();
		if (OnRobotStartUpdate != null)
			OnRobotStartUpdate ();
		LogSystem.Instance.Log ("Start Updating Robot:" + _RobotIP.Name, LogSystem.LogType.Info);
	}
	
	public void EndUpdate()
	{
		if (!_connected) 
			return;
		_connector.EndUpdate();
		if (OnRobotStopUpdate != null)
			OnRobotStopUpdate ();
		LogSystem.Instance.Log ("End Updating Robot:" + _RobotIP.Name, LogSystem.LogType.Info);
	}
	public bool IsRobotStarted()
	{
		return _connected && _connector.IsRobotConnected;
	}

}
