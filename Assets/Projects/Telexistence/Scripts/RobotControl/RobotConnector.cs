using UnityEngine;
using System.Collections;
using System;
using System.Threading;

public class RobotConnector:IDisposable{
	
	class RobotStatus
	{
		public bool Connected=false;//is connected with robot
		public bool Status=false;   //is robot connected and running
	/*	public Vector2 Speed;
		public Quaternion HeadRotation;
		public Vector3 HeadPosition;
		public float Rotation=0;*/
	}
	
	public struct TargetPorts
	{
		public TargetPorts(string ip)
		{
			RobotIP=ip;
			CommPort=0;/*
			LeftEyeVideo=0;
			RightEyeVideo=0;
			AudioPort=0;
			HandsPort=0;
			ClockPort=0;
			Rtcp=false;*/
		}
		public string RobotIP;

		public int CommPort;
		/*public int LeftEyeVideo;
		public int RightEyeVideo;
		public int AudioPort;
		public int HandsPort;
		public int ClockPort;
		public bool Rtcp;*/
	}

	public IRobotCommunicator RobotCommunicator;
	public IRobotDataCommunicator DataCommunicator;

	RobotStatus _robotStatus;

	/*

	public Vector2 Speed
	{
		get{
			return _robotStatus.Speed;
		}
	}
	public Quaternion HeadRotation
	{
		get{
			return _robotStatus.HeadRotation;
		}
	}
	public Vector3 HeadPosition
	{
		get{
			return _robotStatus.HeadPosition;
		}
	}
	public float Rotation
	{
		get{
			return _robotStatus.Rotation;
		}
	}*/

	public bool IsRobotConnected
	{
		get
		{
			return _robotStatus.Status;
		}
	}


	Thread _processingThread;
	bool _IsDone=false;
	public RobotConnector()
	{
		_robotStatus = new RobotStatus ();
		DataCommunicator = new RTPDataCommunicator ();
		_processingThread = new Thread(new ThreadStart(this.UpdateThreaded));
		//_processingThread.Start();
	}

	public int StartDataCommunicator()
	{
		DataCommunicator.SetPort (Settings.Instance.GetPortValue("DataCom",0));
		DataCommunicator.Start();
		return DataCommunicator.GetPort();
	}
	public void StopDataCommunicator()
	{
		DataCommunicator.Stop ();
	}
	public void Dispose()
	{
		_IsDone = true;
		DisconnectRobot ();
		StopDataCommunicator ();
		if(_processingThread.IsAlive)
			_processingThread.Join ();
		_processingThread = null;
		RobotCommunicator = null;
		DataCommunicator = null;
	}

	public void ConnectRobot(RobotInfo roboIfo)
	{
		if (RobotCommunicator==null)
			return;
		if (_robotStatus.Connected)
			RobotCommunicator.Disconnect();

		_robotStatus.Connected = RobotCommunicator.Connect(roboIfo);
		RobotCommunicator.ClearData(true);
		//	m_roboComm->Connect("127.0.0.1",3000);
		RobotCommunicator.SetUserID("yamens");
		RobotCommunicator.ConnectUser(true);
		string addrStr = Utilities.LocalIPAddress();
		/*
		addrStr += "," + _ports.LeftEyeVideo.ToString();
		addrStr += "," + _ports.RightEyeVideo.ToString();
		addrStr += "," + _ports.AudioPort.ToString();
		addrStr += "," + _ports.HandsPort.ToString();
		addrStr += "," + _ports.ClockPort.ToString();
		addrStr += "," + _ports.Rtcp.ToString();*/
		RobotCommunicator.SetData("","Connect", addrStr,true,false);
		
		addrStr = DataCommunicator.GetPort().ToString();//_ports.CommPort.ToString();
		RobotCommunicator.SetData("","CommPort", addrStr, true,false);
		
	}
	public void DisconnectRobot()
	{
		if (RobotCommunicator==null)
			return;

		string ipAddr = Utilities.LocalIPAddress ();
		RobotCommunicator.ConnectUser (false);
		RobotCommunicator.ConnectRobot (false);
		string addrStr = ipAddr+","+DataCommunicator.GetPort().ToString();
		_robotStatus.Connected = false;
		RobotCommunicator.SetData ("","shutdown", "", false,true);
		RobotCommunicator.SetData("","Disconnect", addrStr, true,true);
		RobotCommunicator.Update(true);//only once
		LogSystem.Instance.Log("Disconnecting Robot",LogSystem.LogType.Info);
		EndUpdate();
		RobotCommunicator.Disconnect();


	}
	public void StartUpdate()
	{
		if (RobotCommunicator==null)
			return;
		if (!_robotStatus.Connected)
			return;
		_robotStatus.Status = true;
		
		RobotCommunicator.ConnectRobot(true);
	}
	public void EndUpdate()
	{
		if (RobotCommunicator==null)
			return;
		_robotStatus.Status = false;
		RobotCommunicator.ConnectRobot(false);
		RobotCommunicator.Update(true);//only once

	}

	public void Update()
	{
		if (!_robotStatus.Connected)
			return;
		if (RobotCommunicator == null)
			return;
		
		//RobotCommunicator.Update (false);
	}

	public void UpdateThreaded()
	{/*
		while (!_IsDone) {
			 else {
				Thread.Sleep (100);
			}
		}*/
	}
	public void SendData(string target,string name,string value,bool statusData=false,bool immediate=false)
	{
		if (RobotCommunicator==null)
			return;
		RobotCommunicator.SetData (target,name,value,statusData,immediate);
	}

}



