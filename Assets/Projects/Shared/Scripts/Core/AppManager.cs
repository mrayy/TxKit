using UnityEngine;
using System.Collections;
//using UnityEditor;


public class AppManager : MonoBehaviour
{
	static AppManager _instance=null;

	public static bool Exists
	{
		get{ return _instance != null; }
	}
	public static AppManager Instance
	{
		get{
			if(_instance==null)
			{
				GameObject obj = new GameObject ("_AppManager");
				_instance=obj.AddComponent<AppManager>();
				_instance._appManager = obj;
				Debug.Log ("AppManager Instance Created");
			}
			_instance.Init ();
			return _instance;
		}
	}

	public enum HeadControllerType
	{
		None,
		Keyboard,
		Oculus,
		SteamVR,
		OptiTrack,
		SceneNode,
		Custom
	}
	
	public enum BaseControllerType
	{
		None,
		Keyboard,
		Joystick,
		Oculus,
		Wiiboard,
		Custom
	}

    public RobotInfoManager RobotManager;
    public CameraConfigurationsManager CamConfigManager;

	public IRobotScanner RobotScanner;
	bool _inited=false;

	GameObject _appManager;

    public AppManager()
	{
		if (_instance == null) {
			_instance = this;
			_inited = false;
		}

	}

	void OnApplicationQuit()
	{
		_instance=null;
	}
	void OnDestroy()
	{
		if (RobotScanner != null)
			RobotScanner.Destroy ();
		RobotScanner = null;
		_instance = null;
	}

	public void Init()
	{
		if (_inited)
			return;
		if(_appManager==null)
			_appManager = gameObject;

		RobotManager= _appManager.AddComponent<RobotInfoManager> ();
		CamConfigManager = new CameraConfigurationsManager ();
		RobotScanner = new RTPRobotScanner ();

		RobotManager.LoadRobots(Application.dataPath + "\\Data\\RobotsMap.xml");
		CamConfigManager.LoadConfigurations(Application.dataPath + "\\Data\\CameraConfigurations.xml");
		RobotScanner.ScanRobots ();

		_inited = true;
	}

	public bool IsDebugging;




}
