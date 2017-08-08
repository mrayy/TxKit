using UnityEngine;
using System.Collections;
using System.Threading;
using System;


public interface IRobotScanner
{
	void ScanRobots ();
	void Destroy();
}

public class RTPRobotScanner:IRobotScanner {

	RTPRobotCommunicator _robotCommunicator;
	RTPDataCommunicator _dataCommunicator;

	int ScanPeriod=5000;

	ThreadJob _thread;

	public RTPRobotScanner()
	{
		_robotCommunicator = new RTPRobotCommunicator ();
		_dataCommunicator = new RTPDataCommunicator ();
		_dataCommunicator.Start ();
		_dataCommunicator.OnRobotInfoDetected+=OnRobotInfoDetected;

		_thread = new ThreadJob ();
		_thread.ThreadHandler += ThreadHandler;
		_thread.Start ();
	}

	void ThreadHandler(ThreadJob t)
	{
		Thread.Sleep (5000);//wait until everything initialized to avoid multithreaded access violations
		while (!t.IsDone) {
			ScanRobots ();
			Thread.Sleep (ScanPeriod);
		}
	}

	void OnRobotInfoDetected(RobotInfo ifo)
	{
		if (AppManager.Exists == false)
			return;
		if (AppManager.Instance.RobotManager.AddRobotInfo (ifo)) {
			Debug.Log ("Robot Added: " + ifo.Name + ":" + ifo.IP);
		}
	}

	public void ScanRobots()
	{
		try
		{
			_robotCommunicator.SetData ("","detect", _dataCommunicator.GetPort().ToString(), true,false);
			_robotCommunicator.BroadcastMessage (Settings.Instance.GetPortValue("CommPort",6000));
			_robotCommunicator.RemoveData ("","detect");
		}catch(Exception e) {
		}
	}
	public void Destroy()
	{
		_thread.Abort ();
		_robotCommunicator.Disconnect ();
		_robotCommunicator.Dispose ();
		_dataCommunicator.Stop ();

		Thread.Sleep (100);
	}
}
