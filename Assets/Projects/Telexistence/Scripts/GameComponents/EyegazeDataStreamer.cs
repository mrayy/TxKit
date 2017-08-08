using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EyegazeDataStreamer : MonoBehaviour {
	public RobotConnectionComponent Robot;
	public PupilGazeTracker Tracker;

	public TxKitEyes TargetCamera;

	public Image Eyegaze;

	public RawImage Image;

	public Vector2 gaze;

	public bool UseMouse = false;

	public DebugInterface Debugger;

	public bool UseFoveation=true;
	public int FoveaSize = 18;//in degrees
	public int FoveaLevels = 0 ;

	public Vector2 frameSize=new Vector2(640,480);
	//public Vector2 frameSize=new Vector2(1280,720);
	//	public Vector2 frameSize=new Vector2(1920,1080);
	//public Vector2 frameSize=new Vector2(4224,3156);

	Vector2[] _GazeList=new Vector2[2]{new Vector2(),new Vector2()};

	public Vector2 GazeArea = new Vector2 (0.3f, 0.5f);
	public bool DebugBlitArea=false;
	public float BlurSize=0.1f;
	public bool DrawRectangle=false;

	public EyegazeWebcameraRenderMesh[] Renderer;

	public enum GazeBlitType
	{
		Circular,
		Rectangular
	}

	public bool[] DisabledFoveations=new bool[10];


	public GazeBlitType GazeBlitMethod=GazeBlitType.Circular;

	ScreenshotHelper _imageshot;
	ScreenshotHelper _imageshot2;


	class EyegazeDSDebug:DebugInterface.IDebugElement
	{
		public EyegazeDataStreamer ds;
		public EyegazeDSDebug(EyegazeDataStreamer d)
		{
			ds=d;
		}

		public string GetDebugString()
		{
			var mesh = ds.TargetCamera.CamRenderer [0] as EyegazeWebcameraRenderMesh;
			if (mesh == null)
				return "";
			string str = "Foveal rendering settings:\n";
		//	str += "\t FOV (degrees)     :"+(90*mesh.gazeSize.y/mesh.frameSize.y).ToString()+"\n";
			str += "\t Size (pixels)     :"+(mesh.gazeSize).ToString()+"\n";
		//	str += "\t Framesize (pixels):"+(mesh.frameSize).ToString()+"\n";
			str += "\t Blit type         :"+ds.GazeBlitMethod.ToString()+"\n";
			str += "\t Blit parameters   :"+ds.GazeArea.ToString()+"\n";

			return str;
		}
	}

	// Use this for initialization
	void Start () {
		if(Tracker!=null)
			Tracker.OnEyeGaze += OnEyeGaze;
		TargetCamera.OnCameraRendererCreated += OnCameraRendererCreated;
		TargetCamera.OnImageArrived += OnImageArrived;
		Debugger.AddDebugElement(new EyegazeDSDebug (this));

		Renderer = new EyegazeWebcameraRenderMesh[2];

		_imageshot = new ScreenshotHelper ();
		_imageshot.prefix="gaze";
		_imageshot2 = new ScreenshotHelper ();
		_imageshot2.prefix = "source";
	}
	ScreenshotHelper screenshot=new ScreenshotHelper();
	ScreenshotHelper screenshot2=new ScreenshotHelper();
	bool screen=false;

	EyegazeWebcameraRenderMesh[] _renderers;

	void OnImageArrived(TxKitEyes src,int eye)
	{
		return;
		if (eye == 0) {
			screen = true;
		}
	}
	void OnCameraRendererCreated(TxKitEyes creator,ICameraRenderMesh[] renderers)
	{
		_renderers=new EyegazeWebcameraRenderMesh[renderers.Length];
		for (int i = 0; i < renderers.Length; ++i) {
			var mesh = renderers [i] as EyegazeWebcameraRenderMesh;
			if (mesh != null) {
				_renderers [i] = mesh;
				mesh.DataStreamer = this;
			}
		}
	}
	void OnEyeGaze(PupilGazeTracker manager,int index)
	{
		if (UseMouse)
			return;
	}

	public float speed=1.0f;
	float dir=1;

	public bool IsFoveatedStreaming()
	{
		return _renderers!=null && _renderers [0]!=null && _renderers [0].IsFoveatedStreaming ();
	}

	Vector2 ScreenToField(Vector2 pos)
	{
		//convert position from screen space (0-->1) to camera field of view
		if (TargetCamera.Configuration == null)
			return pos;

		float fov=TargetCamera.Configuration.CamSettings.FoV;
		Vector2 sz=frameSize;

		float aspect = (float)sz.x / (float)sz.y;
		aspect *= TargetCamera.CameraSource.GetEyeScalingFactor (0).x / TargetCamera.CameraSource.GetEyeScalingFactor (0).y;

		float camfov=TargetCamera.CamRenderer[0].DisplayCamera.fieldOfView;
		float camaspect = TargetCamera.CamRenderer [0].DisplayCamera.aspect;
		float w1 = Mathf.Tan(Mathf.Deg2Rad*camfov*0.5f);
		float w2 = Mathf.Tan(Mathf.Deg2Rad*fov*0.5f);

		if(w1==0)
			w1=1;
		float ratio = w2 / w1;


		sz.x = ratio/camaspect;
		sz.y = ratio / aspect;
		pos.x=Utilities.Map (pos.x, 0.5f-sz.x / 2, 0.5f+sz.x / 2,0, 1);
		pos.y=Utilities.Map (pos.y, 0.5f-sz.y / 2, 0.5f+sz.y / 2,0, 1);

		return pos;
	}

	// Update is called once per frame
	void FixedUpdate () {
		//Vector2 gaze = new Vector2 (Tracker.EyePos.x , Tracker.EyePos.y);
		if (TargetCamera.CamRenderer.Length == 0 || TargetCamera.CamRenderer [0] as EyegazeWebcameraRenderMesh == null)
			return;
		string gazeStr = "";
		if (UseMouse) {
			gaze = new Vector2 ((float)Input.mousePosition.x / (float)Screen.width, 1 - (float)Input.mousePosition.y / (float)Screen.height);

			//gaze = ScreenToField (gaze);
			/*
			gaze.x +=dir*speed ;
			if (gaze.x < 0 || gaze.x > 1) {
				dir = -dir;
				Mathf.Clamp01 (gaze.x);
			}
			gaze.y = 0.5f;*/

			for (int i = 0; i < _GazeList.Length; ++i) {
				gazeStr += (gaze.x).ToString () + "," + (gaze.y).ToString ();
				if (i != _GazeList.Length - 1)
					gazeStr += ",";
			}
		} else {

			_GazeList [0] = Tracker.GetEyeGaze (PupilGazeTracker.GazeSource.BothEyes);

			gaze = new Vector2 (_GazeList [0].x, 1 - (_GazeList [0].y));
			//must convert to screenspace first
			var g = gaze;//ScreenToField (gaze);
			for (int i = 0; i < _GazeList.Length; ++i) {
				gazeStr += (g.x).ToString () + "," + (g.y).ToString ();
				if (i != _GazeList.Length - 1)
					gazeStr += ",";
			}


		}
		Robot.Connector.SendData (TxKitEyes.ServiceName, "Gaze", gazeStr, false);
		for (int i = 0; i < TargetCamera.CamRenderer.Length; ++i) {
			var mesh = TargetCamera.CamRenderer [i] as EyegazeWebcameraRenderMesh;
			if (mesh != null)
				mesh.SrcEyeGaze = gaze;
		}

		if (screen) {
			_imageshot.TakeScreenshot ((TargetCamera.CamRenderer[0] as EyegazeWebcameraRenderMesh)._RenderedTexture, Application.dataPath + "\\.screenShots\\");
			_imageshot.TakeScreenshot ((TargetCamera.CamRenderer[0] as EyegazeWebcameraRenderMesh).OriginalTexture, Application.dataPath + "\\.screenShots\\");
			screen = false;
		}
	}
	void Update(){

	//	if(FoveaLevels>0)
	//		Image.texture=(TargetCamera.CamRenderer[0] as EyegazeWebcameraRenderMesh)._GazeTexture[0];
		//Image.rectTransform.localPosition = new Vector3 (Tracker.EyePos.x , Tracker.EyePos.y, 0);


		if (Input.GetKeyDown (KeyCode.A))
			DebugBlitArea = !DebugBlitArea;

		if (Input.GetKeyDown (KeyCode.S))
			DrawRectangle = !DrawRectangle;
		
		if (Input.GetKeyDown (KeyCode.G))
			Eyegaze.enabled = !Eyegaze.enabled;

		if (Input.GetKeyDown (KeyCode.M))
			UseMouse = !UseMouse;

		if (Input.GetKeyDown (KeyCode.O))
			GazeBlitMethod = GazeBlitType.Circular;
		if (Input.GetKeyDown (KeyCode.I))
			GazeBlitMethod = GazeBlitType.Rectangular;
		
		for (int i = 0; i < 9; ++i) {
		
			if (Input.GetKeyDown ((KeyCode)((int)KeyCode.Alpha0 + i + 1))) {
			
				DisabledFoveations [i] = !DisabledFoveations [i];
			}
		}



		if (Input.GetKeyDown (KeyCode.P)) {
			screen = true;
		}
	}

	public void OnGUI()
	{
	}
}
