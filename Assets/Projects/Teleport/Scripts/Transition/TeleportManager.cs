using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeleportManager : MonoBehaviour {

	public TxKitEyes[] Robots;
	public int ActiveRobotID;


	public Material ActiveMaterial;
	public Material RemoteMaterial;
	class RobotConnectionIfo
	{
		
		public TxKitEyes Renderer;
		public RobotConnectionComponent Robot;
		public bool IsActive=false;
	}
	List<RobotConnectionIfo> _ifo=new List<RobotConnectionIfo> ();

	public RobotConnectionComponent ActiveRobot
	{
		get
		{
			if(ActiveRobotID>=Robots.Length)
				return null;
			return _ifo[ActiveRobotID].Robot;
		}
	}

	// Use this for initialization
	void Start () {

		if (ActiveRobotID >= Robots.Length) {
			ActiveRobotID=0;
		}
		for(int i=0;i<Robots.Length;++i)
		{
			RobotConnectionIfo ifo= AddRobot(Robots[i]);
			ifo.Renderer.ApplyMaterial(RemoteMaterial);
			ifo.IsActive=true;
		}
		if (_ifo.Count > ActiveRobotID) {
			_ifo[ActiveRobotID].IsActive=true;
			_ifo[ActiveRobotID].Renderer.ApplyMaterial(ActiveMaterial);
		}
	}

	RobotConnectionIfo AddRobot(TxKitEyes r)
	{
		RobotConnectionIfo ifo=new RobotConnectionIfo();
		ifo.Robot = r.RobotConnector;
		ifo.Renderer = r;
		ifo.IsActive = false;
		_ifo.Add (ifo);
		return ifo;
	}


	void _CheckKeys()
	{
		if (Input.GetButtonDown ("ConnectRobot")) {
			for(int i=0;i<_ifo.Count;++i)
			{
				if(_ifo[i].IsActive)
				{
					if(!_ifo[i].Robot.IsConnected)
						_ifo[i].Robot.ConnectRobot();
					else _ifo[i].Robot.DisconnectRobot();
				}
			}
		}
		if ( Input.GetButtonDown ("StartRobot")) {
			for(int i=0;i<_ifo.Count;++i)
			{
				if(_ifo[i].IsActive)
				{
					if(!_ifo[i].Robot.IsRobotStarted())
						_ifo[i].Robot.StartUpdate();
					else
						_ifo[i].Robot.EndUpdate();
				}
			}
		}
		if (Input.GetButtonDown ("CalibrateRobot"))  {
			for(int i=0;i<_ifo.Count;++i)
			{
				if(_ifo[i].IsActive)
				{
					var b=_ifo[i].Robot.gameObject.GetComponent<TxKitBody> ();
					if (b)
						b.Recalibrate ();
				}
			}
		}

	}

	// Update is called once per frame
	void Update () {
		_CheckKeys ();

	}

	//triggers when the user teleports to robot [id]
	public void OnTeleported(int id)
	{
		if(id==ActiveRobotID)
		{
			return;
		}
		for(int i=0;i<_ifo.Count;++i)
		{
			if(i==id)
			{
				_ifo[i].Renderer.ApplyMaterial(ActiveMaterial);
			}else{
				_ifo[i].Renderer.ApplyMaterial(RemoteMaterial);
			}
		}
		ActiveRobotID = id;
	}
}
