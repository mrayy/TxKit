using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CameraConfigurationsManager  {

	Dictionary<string,CameraConfigurations> _cams=new Dictionary<string, CameraConfigurations>();


	public Dictionary<string,CameraConfigurations> Configurations
	{
		get{
			return _cams;
		}
	}

	public void Clear()
	{
		_cams.Clear ();
	}

	public CameraConfigurations GetCamera(string name)
	{
		if (_cams.ContainsKey (name))
			return _cams [name];
		return null;
	}

	public void LoadConfigurations(string path)
	{
		XmlReader reader=XmlReader.Create(path); 
		if (reader == null) {
			LogSystem.Instance.Log("CameraConfigurationsManager::LoadConfigurations()- Failed to load Configurations File:"+path,LogSystem.LogType.Error);
			return;
		}
		long ID = 0;
		while (reader.Read()) {
			if(reader.Name=="CameraConfiguration")
			{
				CameraConfigurations r=new CameraConfigurations();
				r.LoadXML(reader);
				_cams[r.Name]=r;

//				Debug.Log("Camera Configuration Loaded: "+r.Name);
			}
		}
		reader.Close ();
	}

}
