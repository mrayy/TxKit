using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class TxKitEars : MonoBehaviour,IDependencyNode {
	public RobotConnectionComponent RobotConnector;

	RobotInfo _robotIfo;

	IAudioSource _audioSource;

	bool _audioInited=false;

	string _audioProfile;
	public IAudioSource AudioSource
	{
		get{ return _audioSource; }
	}

	public delegate void OnAudioSourceCreated_deleg(TxKitEars creator,IAudioSource src);
	public OnAudioSourceCreated_deleg OnAudioSourceCreated;


	public  void OnDependencyStart(DependencyRoot root)
	{
		if (root == RobotConnector) {
			RobotConnector.OnRobotConnected += OnRobotConnected;
			RobotConnector.OnRobotDisconnected+=OnRobotDisconnected;
			RobotConnector.Connector.DataCommunicator.OnAudioConfig += OnAudioConfig;
			RobotConnector.OnServiceNetValue+=OnServiceNetValue;
		}
	}

	// Use this for initialization
	void Start () {

		if (RobotConnector == null)
			RobotConnector=gameObject.GetComponent<RobotConnectionComponent> ();
		
		RobotConnector.AddDependencyNode (this);

	}
	void OnDestroy()
	{
	}
	// Update is called once per frame
	void Update () {

		if (!_audioInited && RobotConnector.IsConnected)
			_initAudio ();
		
		if (_audioSource != null)
			_audioSource.Update ();
		
	}

	void OnEnable()
	{
	}


	void OnDisable()
	{
	}

	void OnAudioConfig(string audioProfile)
	{
		if (!RobotConnector.IsConnected)
			return;
		if (_audioProfile != audioProfile) {

			_audioInited = false;
			_audioProfile = audioProfile;

			XmlReader reader = XmlReader.Create (new StringReader (_audioProfile));
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element) {
				}
			}
		}

		//Debug.Log (cameraProfile);
	}
	public void OnServiceNetValue(string serviceName,int port)
	{
		if (serviceName == "AVStreamServiceModule") {
		}
	}
	void OnRobotConnected(RobotInfo ifo,RobotConnector.TargetPorts ports)
	{
		SetRobotInfo (ifo, ports);
	}
	void OnRobotDisconnected()
	{
		if (_audioSource!=null) {
			_audioSource.Close();
			_audioSource=null;
		}
		_audioInited = false;
	}

	public void PauseAudio()
	{
		if (!RobotConnector.IsConnected)
			return;
		//if (_audioSource != null)
		//	_audioSource.SetAudioVolume (0);
		if (_audioSource != null)
			_audioSource.Pause ();
		//if(RobotConnector.Connector!=null)
		//	RobotConnector.Connector.SendData("PauseAudio","",false,true);
	}
	public void ResumeAudio()
	{
		//if (_audioSource != null)
		//	_audioSource.SetAudioVolume (1);

		if (!RobotConnector.IsConnected)
			return;
		if (_audioSource != null)
			_audioSource.Resume ();
		/*RobotConnector.Connector.SendData("PauseAudio","",false,true);*/
	}

	void _initAudio()
	{
		if (_robotIfo.ConnectionType == RobotInfo.EConnectionType.RTP) {
			_CreateRTPAudio ();
		}
		else if (_robotIfo.ConnectionType == RobotInfo.EConnectionType.WebRTC) {
		}else if(_robotIfo.ConnectionType == RobotInfo.EConnectionType.Local) {
		}else if(_robotIfo.ConnectionType == RobotInfo.EConnectionType.Ovrvision) {
		}else if(_robotIfo.ConnectionType == RobotInfo.EConnectionType.Movie) {
		}
		_audioInited = true;
	}
	void _CreateRTPAudio()
	{
		RTPAudioSource a; 
		if (_audioSource != null) {
			_audioSource.Close ();
		}
		_audioSource = (a = new RTPAudioSource());


		a.AudioStream = true;
		a.TargetNode = gameObject;
		a.RobotConnector = RobotConnector;
		a.Init (_robotIfo);

		if (OnAudioSourceCreated != null)
			OnAudioSourceCreated (this, _audioSource);
	}
	public void SetRobotInfo(RobotInfo ifo,RobotConnector.TargetPorts ports)
	{
		_robotIfo = ifo;
		RobotConnector.Connector.SendData("AudioParameters","",false,true);
	}
}
