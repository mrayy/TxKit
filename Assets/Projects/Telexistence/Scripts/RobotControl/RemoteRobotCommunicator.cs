using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System;
using System.IO;
using System.Xml;
using System.Threading;
using System.Text;
using MsgPack;
using MsgPack.Serialization;
using System.IO.Compression;

public class RemoteRobotCommunicator : IRobotCommunicator,IDisposable {


	class CommunicationThread:ThreadJob
	{
		RemoteRobotCommunicator owner;
		public CommunicationThread(RemoteRobotCommunicator o)
		{
			owner=o;
		}
		protected override void ThreadFunction() 
		{
			while (!this.IsDone) 
			{
				if(owner._connected || owner.Broadcast)
				{
					owner.Update(true);
				}
				if(owner._userInfo.RobotConnected && owner._connected)
				{
					Thread.Sleep(50);
				}else
				{
					Thread.Sleep(100);
				}
			}
		}
		
		protected override void OnFinished() { 
		}
	}


	struct UserInfo
	{
		public string UserName;
		public bool UserConnected;
		public bool RobotConnected;
		public string RobotAddr;
		public string RobotLocation;
	};


	IDataChannelSender _dataChannel=new RTPDataChannelSender();
	bool _connected=false;
	CommunicationThread _thread;
	UserInfo _userInfo=new UserInfo();

	IDataSerializer _serializer=new XMLDataSerializer();
	string _outputValues="";

	bool _broadCast=false;

	public bool Broadcast
	{
		set{
			_broadCast = value;
		}
		get{
			return _broadCast;
		}
	}

	void _SendUpdate()
	{
		_UpdateData (true);
	}

	void _UpdateData(bool SendNow)
	{
		lock (_serializer) {
			_outputValues = _serializer.SerializeData ();
		}
		if(SendNow)
		{
			_SendData(true);
		}

	}
	void _CleanData(bool statusValues)
	{
		lock (_serializer) {
			_serializer.CleanData (statusValues);
		}
		_UpdateData (false);
	}

	public RemoteRobotCommunicator()
	{
		_thread = new CommunicationThread (this);
		_thread.Start ();
	}

	public  void Dispose()
	{
		_dataChannel.Close ();

		if (_thread != null) {
			_thread.Abort();
		}
	}
	public override void SetBroadcastNext(bool set){
		Broadcast = set;
	}
	public override void BroadcastMessage(int port)
	{
		string d;
		lock(_serializer)
		{
			_UpdateData (false);
			d = _outputValues;
			ClearData (false);
		}
		if (d.Length == 0)
			return;
		bool removeClient = false;
		if (!_dataChannel.IsOpen() ) {
			removeClient=true;
			RobotInfo ifo = new RobotInfo ();
			ifo.communicationPort = port;
			ifo.IP = IPAddress.Broadcast.ToString ();
			_dataChannel.Open(ifo);
		}
		_dataChannel.Broadcast (d);
		if (removeClient) {
			_dataChannel.Close ();
		}
	}
	public override bool Connect (RobotInfo _ifo)
	{
		if (_connected)
			Disconnect ();
		if (_dataChannel.Open (_ifo)) {
			
			_connected = true;
		} else
			_connected = false;
		return _connected;
	}
	public override void Disconnect()
	{
		if (!_connected)
			return;
		_connected = false;
		_dataChannel.Close ();
	}
	public override bool IsConnected()
	{
		return _connected;
	}
	
	public override void SetUserID(string userID)
	{
		_userInfo.UserName = userID;
	}
	public override void ConnectUser(bool c)
	{
		_userInfo.UserConnected = c;
		_SendUpdate ();
	}
	public override void ConnectRobot(bool c)
	{
		_userInfo.RobotConnected = c;
		lock (_serializer) {
			SetData ("RobotConnect", c.ToString (), true,false);
			_SendUpdate ();
		}
	}

	public override string GetData(string key)
	{
		return _serializer.GetData(key);
	}
	
	public override void SetData(string key, string value, bool statusData,bool immediate) 
	{
		lock(_serializer)
		{
			_serializer.SetData (key, value, statusData);
			_UpdateData(immediate);
		}
	}
	public override void RemoveData(string key) 
	{
		lock(_serializer)
		{
			_serializer.RemoveData (key);
			_UpdateData (false);
		}
	}
	public override void ClearData(bool statusValues)
	{
		_CleanData (statusValues);
	}
	void _SendData(bool force)
	{
		if (!_connected && !Broadcast && !force)
			return;
		
		string d;
		lock(_serializer)
		{
			d=_outputValues;
			ClearData (false);
		}
		if (d.Length == 0)
			return;
		try
		{
			var serializer= MessagePackSerializer.Get<string>();
			Stream ss=new MemoryStream();
			try{
			//	serializer.Pack(ss,_outputValues);
			//	Debug.Log("Data Size: "+d.Length.ToString()+"/"+ss.Length.ToString());
			}catch(Exception e)
			{
				Debug.Log(e.ToString());
			}
			if(Broadcast)
			{
				Broadcast=false;//only broadcast once
				_dataChannel.Broadcast(d);
			}else
			{
				_dataChannel.SendData(d);
			}
		}catch(Exception e)
		{
			LogSystem.Instance.Log("RemoteRobotCommunicator::Update() - "+e.Message,LogSystem.LogType.Warning);
		}
	}
	public override void Update(bool send)
	{
		if(send)
			_SendData (false);
	}

}
