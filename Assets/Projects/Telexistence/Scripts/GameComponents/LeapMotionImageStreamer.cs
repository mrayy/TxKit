using UnityEngine;
using System.Collections;

public class LeapMotionImageStreamer : MonoBehaviour,IDependencyNode {

	GstNetworkImageStreamer _streamer;
	GstUnityImageGrabber _imageGrabber;
	
	public LeapMotionRenderer HandRenderer;
	public RobotConnectionComponent RobotConnector;
	bool _isConnected;
	int _handsPort;
	// Use this for initialization
	void Start () {		
		_isConnected = false;
		_handsPort = 7010;
		RobotConnector.AddDependencyNode (this);
	}
	public  void OnDependencyStart(DependencyRoot root)
	{
		if (root == RobotConnector) {
			RobotConnector.OnRobotConnected += OnRobotConnected;
			RobotConnector.OnRobotDisconnected += OnRobotDisconnected;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (_isConnected && _streamer.IsStreaming) {
			_imageGrabber.Update();
		}
	}

	void OnRobotDisconnected()
	{
		_isConnected = false;
		_streamer = null;
	}
	void OnRobotConnected(RobotInfo ifo,RobotConnector.TargetPorts ports)
	{
		HandController c=GameObject.FindObjectOfType<HandController> ();
		if (c == null || !c.IsConnected ()) {
			return;
		}
		_imageGrabber = new GstUnityImageGrabber ();
		_imageGrabber.SetTexture2D (HandRenderer.LeapRetrival [0].MainTextureData,HandRenderer.LeapRetrival [0].Width,HandRenderer.LeapRetrival [0].Height,TextureFormat.Alpha8);
		_imageGrabber.Update();//update once
		
		_handsPort=Settings.Instance.GetPortValue("HandsPort",0);

		_streamer = new GstNetworkImageStreamer ();
		_streamer.SetBitRate (300);
		_streamer.SetResolution (640, 240, 30);
		_streamer.SetGrabber (_imageGrabber);
		_streamer.SetIP (ports.RobotIP, _handsPort, false);
		RobotConnector.Connector.SendData("HandPorts",_handsPort.ToString(),false);
		
		_streamer.CreateStream ();
		_streamer.Stream ();
		_isConnected = true;
	}
}
