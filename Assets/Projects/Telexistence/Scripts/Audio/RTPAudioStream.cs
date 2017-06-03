using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class RTPAudioStream:IAudioStream
{


	public RobotConnectionComponent RobotConnector;
	public GameObject TargetNode;

	GstNetworkAudioStreamer _audioStreamer;
	public int AudioStreamPort=0;
	bool _audioCreated;
	RobotInfo _ifo;

	public void Init(RobotInfo ifo)
	{
		_ifo = ifo;
		_audioCreated = false;
		//Create audio streaming
		_audioStreamer = TargetNode.AddComponent<GstNetworkAudioStreamer> ();
		_audioStreamer.SetChannels(1);

		AudioStreamPort = Settings.Instance.GetPortValue ("AudioStreamPort", AudioStreamPort);
		string ip = Settings.Instance.GetValue("Ports","ReceiveHost",_ifo.IP);
		Debug.Log ("Streaming audio to:" + AudioStreamPort.ToString ());
		_audioStreamer.AddClient (ip, AudioStreamPort);
		_audioStreamer.CreateStream();
		_audioStreamer.Stream ();
		_audioCreated = true;
		RobotConnector.Connector.SendData("AudioParameters","",false,true);

	}

	public void Close()
	{
		if (_audioStreamer != null) {
			_audioStreamer.Close();
			Object.Destroy (_audioStreamer);
			_audioStreamer = null;
		}
	}

	public void Pause()
	{
		if (_audioStreamer != null) {
			_audioStreamer.SetClientVolume(0,0);
		}
			
	}


	public void Resume()
	{
		if (_audioStreamer != null) {
			_audioStreamer.SetClientVolume(0,2);
		//	_audioStreamer.CreateStream ();
		//	_audioStreamer.Stream ();
		}
	}

	public void Update()
	{
	}

	public void SetAudioVolume (float vol)
	{
		_audioStreamer.SetClientVolume(0,vol);
	}
}
