using UnityEngine;
using System.Collections;

public class LocalRobotListener : MonoBehaviour {

	public RobotConnectionComponent RobotConnector;

	// Use this for initialization
	protected virtual void Start () {
		RobotConnector.OnRobotConnected += _OnRobotConnected;
		RobotConnector.OnRobotDisconnected+= _OnRobotDisconnected;
		RobotConnector.OnRobotStartUpdate += _OnRobotStarted;
		RobotConnector.OnRobotStopUpdate += _OnRobotStopped;
		RobotConnector.OnRobotUpdate += _OnRobotUpdated;
	}

	void _OnRobotConnected(RobotInfo ifo, RobotConnector.TargetPorts ports)
	{
		OnRobotConnected ();
	}
	void _OnRobotDisconnected()
	{
		OnRobotDisconnected ();
	}
	void _OnRobotStarted()
	{
		OnRobotStarted ();
	}
	void _OnRobotStopped()
	{
		OnRobotStopped ();
	}
	void _OnRobotUpdated(RobotConnector connector)
	{
		OnRobotUpdated (connector);
	}

	// Update is called once per frame
	void Update () {
	
	}

	public virtual void OnRobotConnected()
	{
	}
	public virtual void OnRobotDisconnected()
	{
	}

	public virtual void OnRobotStarted()
	{
	}
	public virtual void OnRobotStopped()
	{
	}

	public virtual void OnRobotUpdated(RobotConnector connector)
	{
	}
}
