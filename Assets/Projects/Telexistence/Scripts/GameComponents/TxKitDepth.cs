using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class TxKitDepth: MonoBehaviour,IDependencyNode {
	public const string ServiceName="TxDepthServiceModule";
	public RobotConnectionComponent RobotConnector;

	RobotInfo _robotIfo;


	bool _Inited=false;



	public  void OnDependencyStart(DependencyRoot root)
	{
		if (root == RobotConnector) {
			RobotConnector.OnRobotConnected += OnRobotConnected;
			RobotConnector.OnRobotDisconnected+=OnRobotDisconnected;
			RobotConnector.OnServiceNetValue+=OnServiceNetValue;
		}
	}

	// Use this for initialization
	void Start () {

		if (RobotConnector == null)
			RobotConnector=gameObject.GetComponent<RobotConnectionComponent> ();
		
		RobotConnector.AddDependencyNode (this);

	}
	void OnDestroy()
	{
	}
	// Update is called once per frame
	void Update () {
		
		
	}

	void OnEnable()
	{
	}


	void OnDisable()
	{
	}

	public void OnServiceNetValue(string serviceName,int port)
	{
		if (serviceName == "OpenNIServiceModule") {
		}
	}
	void OnRobotConnected(RobotInfo ifo,RobotConnector.TargetPorts ports)
	{
		SetRobotInfo (ifo, ports);
	}
	void OnRobotDisconnected()
	{
		_Inited = false;
	}

	void _CreateRTPDepth()
	{
	}
	public void SetRobotInfo(RobotInfo ifo,RobotConnector.TargetPorts ports)
	{
		_robotIfo = ifo;
	}
}
