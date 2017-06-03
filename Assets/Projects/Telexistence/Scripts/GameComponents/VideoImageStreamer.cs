using UnityEngine;
using System.Collections;

public class VideoImageStreamer : MonoBehaviour,IDependencyNode {

	GstNetworkImageStreamer _streamer;
	GstUnityImageGrabber _imageGrabber;

	public Texture SrcTexture;
	public RobotConnectionComponent RobotConnector;

	public int BitRate=500;

	TextureWrapper _srcTexWrapper;
	bool _isConnected;
	int _videoPort;
	// Use this for initialization
	protected virtual void Start () {		
		_isConnected = false;
		_videoPort = 7011;
		_srcTexWrapper = new TextureWrapper ();
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

			_srcTexWrapper.ConvertTexture (SrcTexture);
			_imageGrabber.SetTexture2D (_srcTexWrapper.WrappedTexture);
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
		_imageGrabber = new GstUnityImageGrabber ();

		_srcTexWrapper.ConvertTexture (SrcTexture);
		_imageGrabber.SetTexture2D (_srcTexWrapper.WrappedTexture);
		_imageGrabber.Update();//update once

		
		_videoPort=Settings.Instance.GetPortValue("VideoStreamPort",_videoPort);

		_streamer = new GstNetworkImageStreamer ();
		_streamer.SetBitRate (BitRate);
		_streamer.SetResolution (_srcTexWrapper.WrappedTexture.width,_srcTexWrapper.WrappedTexture.height, 30);
		_streamer.SetGrabber (_imageGrabber);
		_streamer.SetIP (ports.RobotIP, _videoPort, false);
		
		_streamer.CreateStream ();
		_streamer.Stream ();
		RobotConnector.Connector.SendData("VideoWindowPorts",_videoPort.ToString(),false);
		_isConnected = true;
	}
}
