using UnityEngine;
using System.Collections;
using System;

public class WebRTCRobotCommunicator : IRobotCommunicator,IDisposable {

	bool _connected=false;
	IDataChannelSender _dataChannel;
	WebRTCObjectHandler _rtcHandler;
	IDataSerializer _serializer=new XMLDataSerializer();
	string _outputValues="";
	public override bool Connect (RobotInfo _ifo)
	{
		if (_connected)
			Disconnect ();


		var channel=new ChannelWebRTCDataSender();

		_rtcHandler = _ownerConnection.GetComponent<WebRTCObjectHandler> ();
		if(_rtcHandler==null)
			_rtcHandler= _ownerConnection.gameObject.AddComponent<WebRTCObjectHandler> ();

		CoherentUIView _view = _rtcHandler.GetOrCreateView ();

		channel.Init (_view);
		_dataChannel = channel;

		if (_dataChannel.Open (_ifo)) {

			_connected = true;
		} else
			_connected = false;
		return _connected;
	}
	public override void SetBroadcastNext(bool set){
	}
	public override void BroadcastMessage(int port)
	{
	}
	public  void Dispose()
	{
		Disconnect ();
	}
	public override void Disconnect()
	{
		if (_rtcHandler != null) {
			_rtcHandler.Close ();
			_rtcHandler = null;
		}
		if (_dataChannel != null) {
			_dataChannel.Close ();
			_dataChannel = null;
		}
	}
	public override bool IsConnected()
	{
		return _connected;
	}

	public override void SetUserID(string userID)
	{
	}
	public override void ConnectUser(bool c)
	{
		_SendUpdate ();
	}
	public override void ConnectRobot(bool c)
	{
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
	void _SendData(bool force)
	{
		if (!_connected && !force)
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
			try{
				//	serializer.Pack(ss,_outputValues);
				//	Debug.Log("Data Size: "+d.Length.ToString()+"/"+ss.Length.ToString());
			}catch(Exception e)
			{
				Debug.Log(e.ToString());
			}
			_dataChannel.SendData(d);
		}catch(Exception e)
		{
			LogSystem.Instance.Log("RemoteRobotCommunicator::Update() - "+e.Message,LogSystem.LogType.Warning);
		}
	}
	public override void Update(bool send)
	{
		_SendData (false);
	}
}
