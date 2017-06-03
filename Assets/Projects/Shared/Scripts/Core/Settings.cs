using UnityEngine;
using System.Collections;

public class Settings  {

	INIParser _robotSettings;
	//public RobotConnector.TargetPorts TargetPorts;

	 Settings()
	{
		/*
		TargetPorts.VideoPort = _robotSettings.ReadValue ("Ports", "VideoPort", 7000);
		TargetPorts.AudioPort = _robotSettings.ReadValue ("Ports", "AudioPort", 7005);
		TargetPorts.HandsPort = _robotSettings.ReadValue ("Ports", "HandsPort", 7010);*/
		//TargetPorts.CommPort = _robotSettings.ReadValue ("Ports", "CommPort", 6000);
	}

	bool _inited=false;

	public void Init()
	{
		if (_inited)
			return;
		_inited = true;
		_robotSettings = new INIParser ();
		_robotSettings.Open (Application.dataPath + "\\Data\\RobotSettings.ini");

		Debug.Log ("Settings File loaded");
	}

	public INIParser RobotSettings
	{
		get{
			return _robotSettings;
		}
	}

	public int GetPortValue(string name,int def)
	{
		return _robotSettings.ReadValue ("Ports", name, def);
	}

	public string GetValue(string cat,string name,string def)
	{
		return _robotSettings.ReadValue (cat, name,def);
	}
	/*
	public string GetValue(string category,string name,string defaultValue)
	{
		return _robotSettings.ReadValue (category, name, defaultValue);
	}*/



	static Settings _instance;

	public static Settings Instance
	{
		get
		{
			if (_instance == null) {
				_instance = new Settings ();
			}
			return _instance;
		}
	}
}
