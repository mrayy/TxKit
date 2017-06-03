using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;

public class IRobotInfoManagerListener
{

    public virtual void OnRobotAdded(RobotInfoManager mngr, RobotInfo ifo) { }
	public virtual void OnRobotStatusUpdated(RobotInfoManager mngr,RobotInfo ifo){}
};
public class RobotInfoManager:MonoBehaviour  {

    protected List<RobotInfo> _robots=new List<RobotInfo>();

    public delegate void RobotDelegate(RobotInfoManager m, RobotInfo r);
    public event RobotDelegate OnRobotAdded;
    public event RobotDelegate OnRobotUpdated;


	DebugInterface _debugger;
	class RobotInfoDebug:DebugInterface.IDebugElement
	{
		RobotInfoManager manager;
		public RobotInfoDebug(RobotInfoManager r)
		{
			manager=r;
		}
		public string GetDebugString()
		{
			string msg="Detected Robots:\n";
			foreach(var r in manager._robots){
				msg += String.Format ("\t{0}:{1}\n", r.Name, r.IP);
			}
			return msg;
		}
	}

	void Start()
	{
		_debugger = GameObject.FindObjectOfType<DebugInterface> ();
		if (_debugger) {
			_debugger.AddDebugElement (new RobotInfoDebug (this));
		}
	}
    public bool AddRobotInfo(RobotInfo ifo)
    {
		for (int i = 0; i < _robots.Count; ++i)
			if (_robots [i].Name == ifo.Name) {
				_robots [i] = ifo;
				return false;
			}
        _robots.Add(ifo);

        if (OnRobotAdded != null)
            OnRobotAdded(this, ifo);
		return true;
    }
    public RobotInfo GetRobotInfo(int index)
    {
        return _robots[index];
    }
    public RobotInfo GetRobotInfoByID(int id)
    {
        foreach (RobotInfo i in _robots)
        {
            if (i.ID == id)
                return i;
        }
        return null;
	}
	public RobotInfo GetRobotInfoByName(string name)
	{
		foreach (RobotInfo i in _robots)
		{
			if ( i.Name==name)
				return i;
		}
		return null;
	}
    public List<RobotInfo> GetRobots()
    {
        return _robots;
    }
    public void ClearRobots()
    {
        _robots.Clear();
    }

	public void LoadRobots(string path)
    {
		XmlReader reader=XmlReader.Create(path); 
		if (reader == null) {
			LogSystem.Instance.Log("RobotInfoManager::LoadRobots()- Failed to load Robots File:"+path,LogSystem.LogType.Error);
			return;
		}
		long ID = 0;
		while (reader.Read()) {
			if(reader.Name=="Robot")
			{
				RobotInfo r=new RobotInfo();
				r.LoadXML(reader);
				r.ID=ID++;
				_robots.Add(r);
			}
		}
    }
}
