using UnityEngine;
using System.Collections;

public class TelubeeAudioModule : MonoBehaviour,IDependencyNode {

	public RobotConnectionComponent RobotConnector;

	// Use this for initialization
	void Start () {

		if (RobotConnector == null)
			RobotConnector=gameObject.GetComponent<RobotConnectionComponent> ();

		RobotConnector.AddDependencyNode (this);
	}
	public  void OnDependencyStart(DependencyRoot root)
	{
		if (root == RobotConnector) {
			RobotConnector.OnRobotConnected += OnRobotConnected;
			RobotConnector.OnRobotDisconnected+=OnRobotDisconnected;
			RobotConnector.OnServiceNetValue+=OnServiceNetValue;
		}
	}

	public void OnServiceNetValue(string serviceName,int port)
	{
		if (serviceName == "AudioStreamServiceModule") {
		}
	}
	void OnRobotConnected(RobotInfo ifo,RobotConnector.TargetPorts ports)
	{
	}
	void OnRobotDisconnected()
	{
	}
	void OnDestroy()
	{
	}
	// Update is called once per frame
	void Update () {
	
	}
}
