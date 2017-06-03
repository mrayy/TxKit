
using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;

public class TxKitEyes : MonoBehaviour,IDependencyNode  {

	public RobotConnectionComponent RobotConnector;

	public OVRCameraRig OculusCamera;
	public Material TargetMaterial;
	public int TargetFrameRate=80;

	public DisplayConfigurations Display;


	public TELUBeeConfiguration Configuration;
	bool _customConfigurations;

//	RobotConnectionComponent _Connection;

	public DebugInterface Debugger;

	public ICameraRenderMesh TargetEyeLeft;
	public ICameraRenderMesh TargetEyeRight;

	NetValueObject _videoValues;

	public VideoParametersController ParameterController;

	ICameraSource _cameraSource;

	public ICameraSource CameraSource
	{
		get{ return _cameraSource; }
	}
	/*
	public bool AudioSupport=false;
	*/

	ICameraRenderMesh[] _camRenderer=new ICameraRenderMesh[2];
	GameObject[] _camRendererParents=new GameObject[2];

	DebugCameraCaptureElement _cameraDebugElem;


	public IImageProcessor[] Effects;

	RobotInfo _robotIfo;

	string _cameraProfile="";
	int m_grabbedFrames=0;


	bool _stream=false;

	bool _camsInited=false;


	public delegate void OnCameraSourceCreated_deleg(TxKitEyes creator,ICameraSource src);
	public OnCameraSourceCreated_deleg OnCameraSourceCreated;

	public delegate void OnCameraRendererCreated_deleg(TxKitEyes creator,ICameraRenderMesh[] renderers);
	public OnCameraRendererCreated_deleg OnCameraRendererCreated;

	public delegate void OnImageArrived_deleg(TxKitEyes src,int eye);
	public OnImageArrived_deleg OnImageArrived;

	public Vector2 WebRTCSize=new Vector2(1280,480);

	Vector3 _camsImageOffset;

	public int GrabbedFrames
	{
		get{
			return m_grabbedFrames;
		}
	}

	public string RemoteHostIP
	{
		get{
			if (_robotIfo == null)
				return "";
			return _robotIfo.IP;
		}
	}

	public ICameraRenderMesh[] CamRenderer {
		get {
			return _camRenderer;
		}
	}
	public GameObject[] CamRendererParent
	{
		get{ return _camRendererParents; }
	}
	public  void OnDependencyStart(DependencyRoot root)
	{
		if (root == RobotConnector) {
			RobotConnector.OnRobotConnected += OnRobotConnected;
			RobotConnector.OnRobotDisconnected+=OnRobotDisconnected;
			RobotConnector.Connector.DataCommunicator.OnCameraConfig += OnCameraConfig;
			RobotConnector.OnServiceNetValue+=OnServiceNetValue;
		}
	}
	// Use this for initialization
	void Start () {

		Application.targetFrameRate = TargetFrameRate;
		if(OculusCamera==null)	//Try to find OVRCameraRig component
			OculusCamera = GameObject.FindObjectOfType<OVRCameraRig> ();

		if (RobotConnector == null)
			RobotConnector=gameObject.GetComponent<RobotConnectionComponent> ();

		if (Configuration == null) {
			_customConfigurations = true;
			Configuration = gameObject.AddComponent<TELUBeeConfiguration> ();
		} else {
			_customConfigurations = false;
		}

		Init ();

		if (Debugger != null) {
	//		Debugger.AddDebugElement(new DebugCameraSettings(Configuration));
		}


		RobotConnector.AddDependencyNode (this);

	//	GStreamerCore.Ref ();
	}

	void OnDestroy()
	{
		//GStreamerCore.Unref ();
		if(_videoValues!=null)
			_videoValues.Dispose ();
	}

	public void ApplyMaterial(Material m)
	{
		TargetMaterial = m;
		if(_camRenderer [0]!=null)
			_camRenderer [0].ApplyMaterial (m);
		
		if(_camRenderer [1]!=null)
			_camRenderer [1].ApplyMaterial (m);
	}

	public Material GetMaterial(EyeName eye)
	{
		if (_camRenderer [(int)eye] != null)
			return _camRenderer [(int)eye].Mat;
		return null;
	}
	
	public void OnServiceNetValue(string serviceName,int port)
	{
		if (serviceName == "AVStreamServiceModule") {
			_videoValues.Connect(RobotConnector.RobotIP.IP,port);
			Debug.Log("Net Value Port: "+port);
		}
	}

	public void PauseVideo()
	{
		if (!RobotConnector.IsConnected)
			return;
		if (_cameraSource != null)
			_cameraSource.Pause ();
		if(RobotConnector.Connector!=null)
			RobotConnector.Connector.SendData("PauseVideo","",false,true);
	}
	public void ResumeVideo()
	{
		if (!RobotConnector.IsConnected)
			return;
		if (_cameraSource != null)
			_cameraSource.Resume();
		RobotConnector.Connector.SendData("ResumeVideo","",false,true);
	}

	void OnFrameGrabbed(GstBaseTexture texture,int index)
	{
	//	Debug.Log ("Frame Grabbed: "+index);
		m_grabbedFrames++;
		if (m_grabbedFrames > 10) {
		//	_camRenderer[0].Enable();
		//	_camRenderer[1].Enable();
		}

		if (OnImageArrived!=null)
			OnImageArrived (this, index);

		if (RobotConnector != null) {
			RobotConnector.OnCameraFPS(_cameraSource.GetCaptureRate((int)EyeName.LeftEye),_cameraSource.GetCaptureRate((int)EyeName.RightEye));
		}
	}
	void OnFrameGrabbed_Local(LocalWebcameraSource texture,int index)
	{
		//	Debug.Log ("Frame Grabbed: "+index);
		m_grabbedFrames++;
		if (m_grabbedFrames > 10) {
			//	_camRenderer[0].Enable();
			//	_camRenderer[1].Enable();
		}

		if (OnImageArrived!=null)
			OnImageArrived (this, index);

		if (RobotConnector != null) {
			RobotConnector.OnCameraFPS(_cameraSource.GetCaptureRate((int)EyeName.LeftEye),_cameraSource.GetCaptureRate((int)EyeName.RightEye));
		}
	}

	void OnEnable()
	{
		if(_camRenderer[0]!=null)
			_camRenderer [0].Enable ();
		if(_camRenderer[1]!=null)
			_camRenderer [1].Enable ();
	}


	void OnDisable()
	{
		if(_camRenderer[0]!=null)
			_camRenderer [0].Disable();
		if(_camRenderer[1]!=null)
			_camRenderer [1].Disable();
	}

    void Init()
    {		
		_videoValues=new NetValueObject();
	//	if (ParameterController != null)
	//		ParameterController.TargetValueObject = _videoValues;
    }


	// Update is called once per frame
	void Update () {
		GStreamerCore.Time = Time.time;


		if (_cameraProfile != "" && !_camsInited && RobotConnector.IsConnected) {

			_initCameras ();
			//_cameraProfile="";
		}


		//Offset Cameras using Keyboard
		if (Input.GetKey (KeyCode.LeftControl)) {
			Configuration.CamSettings.PixelShiftLeft.x-=(Input.GetKeyDown(KeyCode.RightArrow)?0:1)-(Input.GetKeyDown(KeyCode.LeftArrow)?0:1);
			Configuration.CamSettings.PixelShiftLeft.y-=(Input.GetKeyDown(KeyCode.UpArrow)?0:1)-(Input.GetKeyDown(KeyCode.DownArrow)?0:1);

		} else if (Input.GetKey (KeyCode.RightControl)) {
			Configuration.CamSettings.PixelShiftRight.x-=(Input.GetKeyDown(KeyCode.RightArrow)?0:1)-(Input.GetKeyDown(KeyCode.LeftArrow)?0:1);
			Configuration.CamSettings.PixelShiftRight.y-=(Input.GetKeyDown(KeyCode.UpArrow)?0:1)-(Input.GetKeyDown(KeyCode.DownArrow)?0:1);
		}
		if (false) {
			if (Input.GetKeyDown (KeyCode.V)) {
				RobotConnector.Connector.SendData ("Stream", _stream.ToString ());
				_stream = !_stream;
			}
		}

		if (_camsInited && false) {
			for (int i = 0; i < 2; ++i) {
				if (_camRendererParents [i] != null)
					_camRendererParents [i].transform.localRotation = Quaternion.Euler (Configuration.CamSettings.OffsetAngle + _camsImageOffset);
			}
		}
		if (ParameterController != null) {
			ParameterController.UpdateValuesObject (_videoValues);
			_videoValues.SendData ();
		}
	}
	/*
	public void SetConnectionComponent(RobotConnectionComponent connection)
	{
		_Connection = connection;
		_Connection.Connector.DataCommunicator.OnCameraConfig += OnCameraConfig;
	}*/
	void OnCameraConfig(string cameraProfile)
	{
		if (!RobotConnector.IsConnected)
			return;
		if (_cameraProfile != cameraProfile && _customConfigurations) {
			
			_camsInited = false;
			_cameraProfile = cameraProfile;

			XmlReader reader = XmlReader.Create (new StringReader (_cameraProfile));
			while (reader.Read()) {
				if(reader.NodeType==XmlNodeType.Element)
				{
					Configuration.CamSettings.LoadXML (reader);
					break;
				}
			}
		}

		//Debug.Log (cameraProfile);
	}

	void OnRobotConnected(RobotInfo ifo,RobotConnector.TargetPorts ports)
	{
		SetRobotInfo (ifo, ports);
	}
	void OnRobotDisconnected()
	{
		if(_camRenderer [0]!=null)
			_camRenderer [0].Disable ();
		if(_camRenderer [1]!=null)
			_camRenderer [1].Disable ();
		

		if (_cameraSource != null) {
			_cameraSource.Close();
			_cameraSource=null;
		}

		for (int i = 0; i < _camRenderer.Length; ++i) {
			if(_camRenderer [i])
				_camRenderer [i].RequestDestroy();
		}
		for (int i = 0; i < _camRendererParents.Length; ++i) {
			if (_camRendererParents [i]!=null) {
				GameObject.Destroy (_camRendererParents [i]);
				_camRendererParents [i] = null;
			}
		}
		_cameraProfile = "";
		_camsInited = false;
	}

	public void SetImageAngleOffset(Vector3 offset)
	{
		_camsImageOffset = offset;
	}

	void _initCameras()
	{
		if (_robotIfo.ConnectionType == RobotInfo.EConnectionType.RTP) {
			_CreateRTPCamera (Configuration.CamSettings.StreamsCount);
		}
		else if (_robotIfo.ConnectionType == RobotInfo.EConnectionType.WebRTC) {
			_CreateWebRTCCamera();
		}else if(_robotIfo.ConnectionType == RobotInfo.EConnectionType.Local) {
			_CreateLocalCamera();
		}else if(_robotIfo.ConnectionType == RobotInfo.EConnectionType.Ovrvision) {
			_CreateOvrvisionCamera();
		}else if(_robotIfo.ConnectionType == RobotInfo.EConnectionType.Movie) {
			_CreateMediaCamera();
		}
		_camRenderer[0].CreateMesh(EyeName.LeftEye);
		_camRenderer[1].CreateMesh(EyeName.RightEye);
		_camsInited = true;
	}
	void _InitCameraRenderers()
	{
		EyeName[] eyes = new EyeName[]{EyeName.LeftEye,EyeName.RightEye};
		//TelubeeCameraRenderer[] Targets = new TelubeeCameraRenderer[]{TargetEyeRight,TargetEyeLeft};
		ICameraRenderMesh[] Targets = new ICameraRenderMesh[]{TargetEyeLeft,TargetEyeRight};

		float[] camsOffset=new float[]{-0.03f,0.03f};
	//	if (OculusCamera != null)
		{

			for (int i = 0; i < _camRenderer.Length; ++i) {
				if(_camRenderer [i])
					_camRenderer [i].RequestDestroy();
			}
			for (int i = 0; i < _camRendererParents.Length; ++i) {
				if (_camRendererParents [i]!=null) {
					GameObject.Destroy (_camRendererParents [i]);
					_camRendererParents [i] = null;
				}
			}

			Camera[] cams = new Camera[2];
			Transform[] Anchors = new Transform[2];
			if (OculusCamera != null) {
				cams [0] = OculusCamera.leftEyeAnchor.GetComponent<Camera>();
				cams [1] = OculusCamera.rightEyeAnchor.GetComponent<Camera>();
				Anchors [0] = OculusCamera.centerEyeAnchor;
				Anchors [1] = OculusCamera.centerEyeAnchor;

				cams [0].cullingMask = (cams [0].cullingMask & ~(1<<LayerMask.NameToLayer ("RightEye"))) | 1<<LayerMask.NameToLayer ("LeftEye");
				cams [1].cullingMask = (cams [1].cullingMask & ~(1<<LayerMask.NameToLayer ("LeftEye"))) | 1<<LayerMask.NameToLayer ("RightEye");
			} else {
				cams [0] = Camera.main;
				cams [1] = Camera.main;

				Anchors [0] = Camera.main.transform;
				Anchors [1] = Camera.main.transform;
			}
			//	Vector2[] pixelShift = new Vector2[] { Configuration.CamSettings.PixelShiftRight,Configuration.CamSettings.PixelShiftLeft};
			for (int i = 0; i < cams.Length; ++i)
			{
				//int i = (int)eyes[c];
				cams[i].backgroundColor=new Color(cams[i].backgroundColor.r,cams[i].backgroundColor.g,cams[i].backgroundColor.b,1);


				//	CreateMesh ((EyeName)i);
				//	RicohThetaRenderMesh r = Targets[i] as RicohThetaRenderMesh;
				ICameraRenderMesh r=null;

				//Check camera type used. 
				if (Configuration.CamSettings.CameraType==CameraConfigurations.ECameraType.WebCamera) {
					//Create A webcamera type renderer
					r = Targets[i] ;
					if (r == null) {
						if(Configuration.CamSettings.streamCodec==CameraConfigurations.EStreamCodec.Ovrvision)
							r = cams [i].gameObject.AddComponent<OVRVisionRenderMesh> ();
						else
							r = cams [i].gameObject.AddComponent<WebcameraRenderMesh> ();
					}
				}else 
				{
					r = Targets[i] ;
					if (r == null) {
						r = cams [i].gameObject.AddComponent<RicohThetaRenderMesh> ();
					}
				}
				r.Mat = Object.Instantiate(TargetMaterial);
				r.DisplayCamera=cams[i];
				r.Src = this;
				r.CamSource = _cameraSource;

				//r.CreateMesh(eyes[c]);
				r.CreateMesh(eyes[i]);

				_camRenderer[i]=r;

				if (eyes[i] == EyeName.RightEye)
				{
					r._RenderPlane.layer=LayerMask.NameToLayer("RightEye");
				}
				else
				{
					r._RenderPlane.layer=LayerMask.NameToLayer("LeftEye");
				}
				if(Targets[i]==null)
				{
					_camRendererParents [i] = new GameObject (this.name+"_"+eyes[i].ToString());
					//_camRendererParents[i].transform.parent=Anchors[i].transform;
					_camRendererParents[i].transform.localRotation = Quaternion.Euler(Configuration.CamSettings.OffsetAngle);
					_camRendererParents[i].transform.position=Anchors[i].transform.position;//Vector3.zero;
					var attachment=_camRendererParents[i].AddComponent<CameraTransformAttachment>();
					attachment.attachedAnchor = Anchors [i].transform;

					r._RenderPlane.transform.parent = _camRendererParents[i].transform;
					r._RenderPlane.transform.localRotation = Quaternion.identity;
					r._RenderPlane.transform.localPosition=new Vector3(camsOffset[i],0,0);


					r.transform.localRotation = Quaternion.identity;
					r.transform.localPosition=Vector3.zero;
				}
			}
		}

		for(int i=0;i<2;++i)
			_camRenderer[i].CamSource=_cameraSource;

		if (OnCameraRendererCreated != null)
			OnCameraRendererCreated (this, _camRenderer);
	}
	void _CreateMediaCamera()
	{
		FileCameraSource c = new FileCameraSource();
		_cameraSource = c;
		_InitCameraRenderers ();
		c.TargetNode = gameObject;
		c.Init (_robotIfo);
		_cameraSource.GetBaseTexture ().OnFrameGrabbed += OnFrameGrabbed;
		if(OnCameraSourceCreated!=null)
			OnCameraSourceCreated (this,_cameraSource);
	}

	void _CreateLocalCamera()
	{
		LocalWebcameraSource c = new LocalWebcameraSource ();
		_cameraSource = c;
		_InitCameraRenderers ();
		c.Init (_robotIfo);
		c.OnFrameGrabbed += OnFrameGrabbed_Local;
		if(OnCameraSourceCreated!=null)
			OnCameraSourceCreated (this,_cameraSource);
	}
	void _CreateOvrvisionCamera()
	{
		OvrvisionSource c = new OvrvisionSource ();
		_cameraSource = c;
		_InitCameraRenderers ();
		c.Init (_robotIfo);
		c.OnFrameGrabbed += OnFrameGrabbed_Local;
		if(OnCameraSourceCreated!=null)
			OnCameraSourceCreated (this,_cameraSource);
	}
	void _CreateWebRTCCamera()
	{
		WebRTCCameraSource c = new WebRTCCameraSource (gameObject,(int)WebRTCSize.x,(int)WebRTCSize.y);
		_cameraSource = c;
		_InitCameraRenderers ();
		c.Init (_robotIfo);

		if(OnCameraSourceCreated!=null)
			OnCameraSourceCreated (this,_cameraSource);
	}
	void _CreateRTPCamera(int streamsCount)
	{
		MultipleNetworkCameraSource c;

		if (_cameraSource != null) {
			_cameraSource.Close ();
		}
		_cameraSource = (c = new MultipleNetworkCameraSource ());

		string profileType = Configuration.CamSettings.streamCodec == CameraConfigurations.EStreamCodec.Ovrvision ? "Ovrvision" : "None";

		_InitCameraRenderers ();
		/*if(Configuration.CamSettings.CameraType==CameraConfigurations.ECameraType.WebCamera)
			c.StreamsCount = 2;
		else c.StreamsCount = 1;*/
		c.SeparateStreams = Configuration.CamSettings.SeparateStreams;
		c.CameraStreams=Configuration.CamSettings.CameraStreams;
		c.StreamsCount = streamsCount;
		c.TargetNode = gameObject;
		c.port = Settings.Instance.GetPortValue("VideoPort",0);
		c.RobotConnector = RobotConnector;
		c.Init (_robotIfo,profileType);
		if(OnCameraSourceCreated!=null)
			OnCameraSourceCreated (this,_cameraSource);
		

		_cameraSource.GetBaseTexture ().OnFrameGrabbed += OnFrameGrabbed;
		m_grabbedFrames=0;

		if (Debugger) {
			Debugger.RemoveDebugElement(_cameraDebugElem);;
			_cameraDebugElem=new DebugCameraCaptureElement(_cameraSource.GetBaseTexture());
			Debugger.AddDebugElement(_cameraDebugElem);;
		}

		//request netvalue port
		if(RobotConnector.Connector.RobotCommunicator!=null)
			RobotConnector.Connector.RobotCommunicator.SetData ("NetValuePort", "AVStreamServiceModule,"+RobotConnector.Connector.DataCommunicator.GetPort().ToString(), false,false);
	}
	public void SetRobotInfo(RobotInfo ifo,RobotConnector.TargetPorts ports)
	{
		_robotIfo = ifo;

		if (ifo.ConnectionType == RobotInfo.EConnectionType.Movie ||
		   ifo.ConnectionType == RobotInfo.EConnectionType.Local ||
		   ifo.ConnectionType == RobotInfo.EConnectionType.WebRTC)
			_customConfigurations = false;
			

		_camsInited = false;

		if (!_customConfigurations) {
			_initCameras ();
		}
		else
		{
			//this should be changed to request system parameters
			//request A/V settings
			RobotConnector.Connector.SendData("CameraParameters","",false,true);
		}

	}
}
