using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

public class RTPDataChannelSender : IDataChannelSender {

	UdpClient _client;
	IPEndPoint _addr = new IPEndPoint ( IPAddress.Loopback,0);

	RobotInfo _ifo;

	public bool Open(RobotInfo ifo)
	{
		_ifo = ifo;
		if (IsOpen ())
			Close ();
		string ip = Settings.Instance.GetValue ("Ports", "ReceiveHost", _ifo.IP);
		_addr.Address = IPAddress.Parse (ip);
		_addr.Port = Settings.Instance.GetPortValue("CommPort", _ifo.communicationPort);
		try
		{
			_client = new UdpClient ();
			//_client.Connect (_addr);
		}catch(Exception e)
		{
			LogSystem.Instance.Log("RTPDataChannel::Open() - "+e.Message,LogSystem.LogType.Error);
			Close ();
		}
		return _client!=null;
	}

	public bool IsOpen()
	{
		return _client != null;
	}
	public void Close ()
	{
		if (_client != null) 
		{
			_client.Close();
			_client=null;
		}
	}

	public int SendData(string data)
	{
		if (!IsOpen ())
			return 0;
		byte[] d;

		d=Encoding.UTF8.GetBytes(data);
		return  _client.Send (d, d.Length, _addr);
	}

	public int Broadcast(string data)
	{
		if (!IsOpen ())
			return 0;
		byte[] d;

		d=Encoding.UTF8.GetBytes(data);
		return _client.Send(d,d.Length,new IPEndPoint(IPAddress.Broadcast,_addr.Port));
	}
}

public class RTPDataChannelReceiver : IDataChannelReceiver {
	

	class DataThread:ThreadJob
	{
		RTPDataChannelReceiver owner;
		public DataThread(RTPDataChannelReceiver o)
		{
			owner=o;
		}
		protected override void ThreadFunction() 
		{
			while (!this.IsDone) {
				if(owner._Process()!=0){
					this.IsDone=true;
				}
			}
		}

		protected override void OnFinished() 
		{
		}
	}

	UdpClient _client;
	DataThread _thread;
	int _port;
	public int Port
	{
		get{
			return _port;
		}
	}



	int _Process()
	{
		if (_client == null)
			return -1;
		IPEndPoint ep=null;
		byte[] data;
		try
		{
			data=_client.Receive (ref ep);
		}catch{
			return 0;
		}
		if (data == null || data.Length == 0)
			return 0;

		if (OnDataReceived != null)
			OnDataReceived (data,ep);
		return 0;
	}

	public bool Open(int port)
	{
		try
		{
			_client = new UdpClient (port);
			_port=(_client.Client.LocalEndPoint as IPEndPoint).Port;
		}catch(Exception e)
		{
			Debug.Log ("RTPDataChannel Warning: " + e.Message);
			return false;
		}
		_thread = new DataThread (this);
		_thread.Start ();
		return true;
	}

	public override bool IsOpen()
	{
		return _client != null;
	}
	public override void Close ()
	{
		if (_client != null) 
		{
			_client.Close();
			_client=null;
		}
	}
}
