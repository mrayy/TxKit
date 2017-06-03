using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System;

public class RTPDataCommunicator:IRobotDataCommunicator
{
	
	
	public enum Messages
	{
		DepthData = 1,
		DepthSize = 2,
		IsStereo = 3,
		CameraConfig = 4,
		CalibrationDone = 5,
		ReportMessage = 6,
		IRSensorMessage = 7,
		BumpSensorMessage = 8,
		BatteryLevel = 9,
		ClockSync = 10,
		ReinitializeRobot = 11,
		RobotStatus=12,
		JointValues=13,
		Detect = 14,
		Presence = 15,
		NetValue = 16,
		AudioConfig = 17,
		AudioPlayerConfig = 18,
		CustomMessage = 19,
	}



	IDataChannelReceiver _receiver;

	int _port=0;

	public int Port{
		get{
			if (_receiver as RTPDataChannelReceiver!=null) {
				return (_receiver as RTPDataChannelReceiver).Port;
			}
			return 0;
		}
		set{
			_port = value;
		}
	}
	public override void SetPort(int port)
	{
		this.Port = port;
	}
	public override int GetPort()
	{
		return Port;
	}
	public RTPDataCommunicator()
	{
	}

	void _OnDataReceived(byte[] data,IPEndPoint ep)
	{
		_Process (data, ep);
	}

	public override void Start()
	{

		RTPDataChannelReceiver  r=new RTPDataChannelReceiver ();
		Debug.Log ("Starting RTPDataChannel using port: " + _port);
		r.Open(_port);

		_receiver = r;
		_receiver.OnDataReceived += _OnDataReceived;
	}

	public override void Stop()
	{
		_receiver.Close ();
		_receiver = null;
	}

	int _Process(byte[] data,IPEndPoint ep)
	{
		var reader = new BinaryReader (new MemoryStream (data));
		int msg = reader.ReadInt32 ();
		switch (msg) {
		case (int)Messages.Presence:
		{
			RobotInfo ifo=new RobotInfo();
			ifo.Read(reader);
			if(ep!=null)
				ifo.IP = ep.Address.ToString ();
			_OnRobotInfoDetected (ifo);
		}break;
		case (int)Messages.DepthData:
			break;
		case (int)Messages.DepthSize:
			break;
		case (int)Messages.IsStereo:
			break;
		case (int)Messages.CameraConfig:
			_OnCameraConfig(reader.ReadStringNative());
			break;
		case (int)Messages.AudioConfig:
			_OnAudioConfig(reader.ReadStringNative());
			break;
		case (int)Messages.AudioPlayerConfig:
			
			_OnAudioPlayerConfig(reader.ReadStringNative());
			break;
		case (int)Messages.ReportMessage:
			{
				int code=reader.ReadInt32();
				string str=reader.ReadStringNative();
				_OnReportMessage(code,str);
			}
			break;
		case (int)Messages.BumpSensorMessage:
			{
				int count=reader.ReadInt32();
				float[] values=new float[count];
				for(int i=0;i<count;++i)
				{
					values[i]=reader.ReadSingle();
				}
				_OnBumpSensor(values);
			}
			break;
		case (int)Messages.IRSensorMessage:
			{
				int count=reader.ReadInt32();
				float[] values=new float[count];
				for(int i=0;i<count;++i)
				{
					values[i]=reader.ReadSingle();
				}
				_OnIRSensor(values);
			}
			break;
		case (int)Messages.BatteryLevel:
			_OnBatteryLevel(reader.ReadInt32());
			break;
		case (int)Messages.ClockSync:
			break;
		case (int)Messages.JointValues:
			{
				int count=reader.ReadInt32();
				float[] values=new float[count];
				for(int i=0;i<count;++i)
				{
					values[i]=reader.ReadSingle();
				}
//				Debug.Log ("Received joints values: " + count);
				_OnJointValues(values);
			}
			break;
		case (int)Messages.RobotStatus:
			{
				int val=reader.ReadInt32();
				_OnRobotStatus((ERobotControllerStatus)val);
			}
			break;
		case (int)Messages.NetValue:
			{
				string name=reader.ReadStringNative();
				int port=reader.ReadInt32();
				_OnServiceNetValue(name,port);
			}
			break;

		default:
			_OnMessage(msg,reader);
			break;
		}
		return 0;
	}
}
