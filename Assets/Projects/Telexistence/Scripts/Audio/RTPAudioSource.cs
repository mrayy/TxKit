using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class RTPAudioSource:IAudioSource
{


	public RobotConnectionComponent RobotConnector;
	public GameObject TargetNode;

	List<GstNetworkAudioPlayer> _audioPlayer=new List<GstNetworkAudioPlayer>();
	List<GstAudioPacketGrabber> _audioGrabbers=new List<GstAudioPacketGrabber>();
	uint _audioPort=0;
	bool _configReceived=false;

	public List<AudioSource> AudioObjects = new List<AudioSource> ();
	GstAudioPlayer _player;

	public bool AudioStream = false;
	RobotInfo _ifo;
	int _audioSourceCount = 1; 
	bool _isSpatialAudio=true;
	bool _audioCreated=false;
	List<Vector3> AudioLocation=new List<Vector3>();

	public GstAudioPlayer AudioProcessor
	{
		get {
			return _player;
		}
	}

	public List<GstNetworkAudioPlayer> AudioPlayer
	{
		get{
			return _audioPlayer;
		}
	}

	void OnAudioConfig(string config)
	{
		if (_audioCreated)
			return;
		AudioLocation.Clear ();

		//XmlReader reader = XmlReader.Create (new StringReader (config));
		XmlDocument d = new XmlDocument ();
		d.Load(new StringReader (config));
		int.TryParse (d.DocumentElement.GetAttribute ("StreamsCount"), out _audioSourceCount);
		if (d.DocumentElement.GetAttribute ("SpatialAudio") == "1" || 
			d.DocumentElement.GetAttribute ("SpatialAudio") == "True")
			_isSpatialAudio = true;
		else
			_isSpatialAudio = false;

		XmlNodeList elems= d.DocumentElement.GetElementsByTagName ("Pos");
		foreach (XmlNode e in elems) {
			Vector3 v = new Vector3 ();
			string[] comps= e.Attributes.GetNamedItem ("Val").Value.Split(",".ToCharArray());
			v.x = float.Parse (comps [0]);
			v.y = float.Parse (comps [1]);
			v.z = float.Parse (comps [2]);
			AudioLocation.Add (v);
		}
		_configReceived = true;
	}

	void _initAudioPlayers()
	{
		_audioCreated = true;
		_configReceived = false;
		//Create audio playback
		if(AudioStream)
		{
			string audioPorts = "";
			GstAudioPlayer.SourceChannel[] channels;
			if (!_isSpatialAudio) {
				channels = new GstAudioPlayer.SourceChannel[1]{ GstAudioPlayer.SourceChannel.Both };
				for (int i = AudioLocation.Count; i < _audioSourceCount; ++i) {
					AudioLocation.Add(Vector3.zero);
				}

			} else {
				//check number of audio locations
				for (int i = AudioLocation.Count; i < 2*_audioSourceCount; ++i) {
					AudioLocation.Add (Vector3.zero);
				}

				channels = new GstAudioPlayer.SourceChannel[2]{ GstAudioPlayer.SourceChannel.Right,GstAudioPlayer.SourceChannel.Left};/*
			//	AudioLocation = new Vector3[_audioSourceCount * 2];
				float angle = 0;
				float step = Mathf.Deg2Rad* 360.0f / (float)AudioLocation.Count;
				float r = 0.1f;
				for (int i = 0; i < AudioLocation.Count; ++i) {
					AudioLocation [i] = new Vector3 (Mathf.Cos (angle)*r, 0, Mathf.Sin (angle)*r);
					angle += step;
				}*/
			}

			int idx = 0;
			for (int i = 0; i < _audioSourceCount; ++i) {
				GstNetworkAudioPlayer aplayer;
				GstAudioPacketGrabber grabber;


				aplayer = new GstNetworkAudioPlayer ();
				aplayer.SetSampleRate (AudioSettings.outputSampleRate);
				aplayer.SetUseCustomOutput (true);

				int audioPort = Settings.Instance.GetPortValue ("AudioPort", 0);
				string ip = Settings.Instance.GetValue("Ports","ReceiveHost",_ifo.IP);
				aplayer.SetIP (ip, audioPort, false);
				aplayer.CreateStream ();
				aplayer.Play ();
				_audioPort = aplayer.GetAudioPort ();
				Debug.Log ("Playing audio from port:" + _audioPort.ToString ());
				audioPorts += _audioPort.ToString ();
				if (i != _audioSourceCount - 1)
					audioPorts += ",";

				// next create the audio grabber to encapsulate the audio player
				grabber = new GstAudioPacketGrabber ();
				grabber.Player = aplayer.AudioWrapper;
				_audioGrabbers.Add (grabber);

				//finally create sound object(s)--
				for (int j = 0; j < channels.Length; ++j) {
					//Create Sound Object
					GameObject audioObj = new GameObject ("AudioObject"+i.ToString()+"_" + TargetNode.name+"_"+channels[j].ToString());
					audioObj.transform.parent = TargetNode.transform;
					audioObj.transform.position = AudioLocation[idx];
					AudioSource asrc = audioObj.AddComponent<AudioSource> ();
					asrc.loop = true;
					_player = audioObj.AddComponent<GstAudioPlayer> ();
					_player.Player = aplayer.AudioWrapper;
					_player.TargetSrc = asrc;
					_player.grabber = grabber;
					_player.SupportSpatialAudio = _isSpatialAudio;
					_player.Channel = channels [j];

					grabber.AttachedPlayers.Add (_player);
					AudioObjects.Add (asrc);
					++idx;
				}
				_audioPlayer.Add (aplayer);

			}
			RobotConnector.Connector.SendData (TxKitEars.ServiceName,"AudioPort", audioPorts, true);
		}
	}
	public void Init(RobotInfo ifo)
	{
		_ifo = ifo;
		_audioCreated = false;
		RobotConnector.Connector.DataCommunicator.OnAudioConfig += OnAudioConfig;

		RobotConnector.Connector.SendData(TxKitEars.ServiceName,"AudioParameters","",false,true);

	}

	public void Close()
	{
		if (_audioPlayer != null) {
			for(int i=0;i<_audioPlayer.Count;++i)
				_audioPlayer[i].Close ();
			_audioPlayer.Clear ();
		}
		foreach (var g in _audioGrabbers) {
			g.Close ();
		}
		_audioGrabbers.Clear ();

		for(int i=0;i<AudioObjects.Count;++i)
		{
			AudioObjects[i].Stop ();
			GameObject.Destroy (AudioObjects[i].gameObject);
		}
		AudioObjects.Clear ();

		AudioLocation.Clear ();

		_configReceived = false;
	}

	public void Pause()
	{
		if (_audioPlayer != null) {
			
			for (int i = 0; i < _audioGrabbers.Count; ++i) {
				foreach (var p in _audioGrabbers[i].AttachedPlayers) {
					p.PauseAudio ();
				}
			}

			/*foreach (var p in _audioPlayer)
				p.Close ();*/
		}
			
	}


	public void Resume()
	{
		if (_audioPlayer != null) {
		/*	foreach (var p in _audioPlayer) {
				p.CreateStream ();
				p.Play ();
			}*/
			for (int i = 0; i < _audioGrabbers.Count; ++i) {
				foreach (var p in _audioGrabbers[i].AttachedPlayers) {
					p.ResumeAudio ();
				}
			}
		}
	}

	public void Update()
	{
		if (_configReceived) {
			_initAudioPlayers ();
			_configReceived = false;
		}
	}

	public float GetAverageAudioLevel ()
	{
		if(AudioProcessor!=null)
			return AudioProcessor.averageAudio;
		return 0;
	}
	public void SetAudioVolume (float vol)
	{
		if (AudioProcessor != null)
			AudioProcessor.Volume = vol;
	}
}
