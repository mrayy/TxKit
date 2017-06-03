using UnityEngine;
using System.Collections;

public interface IDataChannelSender {


	bool Open (RobotInfo ifo);
	bool IsOpen ();
	void Close ();
	int SendData (string data);
	int Broadcast (string data);

}

public abstract class IDataChannelReceiver {

	public delegate void OnDataReceived_Delg(byte[] data,System.Net.IPEndPoint ep);
	public OnDataReceived_Delg OnDataReceived;

	public abstract bool IsOpen ();
	public abstract void Close ();

}
