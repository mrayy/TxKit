using UnityEngine;
using System.Collections;
using System.IO;
using System;
//using UnityEditor;

public class LogSystem {

	//[InitializeOnLoad]
	class Startup
	{
		static Startup()
		{
		//	LogSystem.Instance.OpenLog("log.txt");
		}
	}
	static LogSystem _instance=null;
	public static LogSystem Instance
	{
		get
		{
			if(_instance==null)
			{
				_instance=new LogSystem();
			}
			return _instance;
		}
	}

	string _logPath="log.txt";
	StreamWriter _logFile=null;
	bool _isEditor;

	public enum LogType
	{
		Info,
		Success,
		Warning,
		Error
	}

	LogSystem()
	{
		_logPath = Application.dataPath + "\\log.txt";
		_isEditor = Application.isEditor;
	}
	public void OpenLog(string path)
	{
		if (!_isEditor) {
			_logPath = path;
			try {

				_logFile = File.CreateText (_logPath);
				_logFile.Close ();
			} catch (Exception e) {
				_logPath = Application.dataPath + "\\log.txt";
				Debug.LogError ("Failed to open log file:" + e.Message);
			}
		}
	}

	public void Log(string msg,LogType type)
	{
		if (!_isEditor) {
			_logFile = File.AppendText (_logPath);
			_logFile.WriteLine (type.ToString () + ":" + msg);
			_logFile.Close ();
		} else {
			switch (type) {
			case LogType.Info:
				Debug.Log (msg);
				break;
			case LogType.Success:
				Debug.Log (msg);
				break;
			case LogType.Warning:
				Debug.LogWarning (msg);
				break;
			case LogType.Error:
				Debug.LogError (msg);
				break;
			}
		}
	}
}
