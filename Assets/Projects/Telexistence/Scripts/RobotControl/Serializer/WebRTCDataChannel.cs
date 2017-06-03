using UnityEngine;
using System.Collections;
using System.Text;

public class ChannelWebRTCDataSender : IDataChannelSender {


	RTCSocketSender _rtcSocket=new RTCSocketSender();

	public void Init(CoherentUIView view)
	{
		_rtcSocket.Start (view);
	}

	public bool Open (RobotInfo ifo)
	{
		_rtcSocket.ConnectTo (ifo);
		return _rtcSocket.IsOpen ();
	}
	public bool IsOpen ()
	{
		return _rtcSocket.IsOpen ();
	}
	public void Close ()
	{
		_rtcSocket.Close ();
	}
	public int SendData (string data)
	{
		if (!_rtcSocket.IsOpen ())
			return 0;
		_rtcSocket.Send (data);
		return data.Length;
	}
	public int Broadcast (string data)
	{
		return 0;
	}
}

public class ChannelWebRTCDataReceiver : IDataChannelReceiver,RTCCallbackObserver
{


	RTCSocketReceiver _rtcSocket;
	string _msg;

	public ChannelWebRTCDataReceiver()
	{
		RTCSocketReceiver _rtcSocket=new RTCSocketReceiver();
		_rtcSocket.SetObserver (this);
	}

	public void Init(CoherentUIView view)
	{
		_rtcSocket.Start (view);
	}

	public bool Open (RobotInfo ifo)
	{
		_rtcSocket.ConnectTo (ifo);
		return _rtcSocket.IsOpen ();
	}
	public override  bool IsOpen ()
	{
		return _rtcSocket.IsOpen ();
	}
	public override void Close ()
	{
		_rtcSocket.Close ();
	}
	public void RecvMessage(string message)
	{
		if (OnDataReceived!=null) {
			byte[] d;

			d=Encoding.UTF8.GetBytes(message);
			OnDataReceived (d,null);
		}
	}
}