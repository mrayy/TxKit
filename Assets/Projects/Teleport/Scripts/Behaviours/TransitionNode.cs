using UnityEngine;
using System.Collections;

public class TransitionNode : MonoBehaviour {

	public MultipleTeleportManager Manager;
	public int RobotID;

	bool _isHit=false;

	TransitionNodesManager _transManager;

	// Use this for initialization
	void Start () {
		GetComponent<Collider> ().isTrigger = true;
		gameObject.tag = "TransitionNode";
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void OnTransition()
	{
		if (!_isHit) {
			Manager.SetActiveRobot (RobotID);
			if (_transManager)
				_transManager.OnTransition (this);
		}
	}

	public void SetTransitionManager(TransitionNodesManager m)
	{
		_transManager= m;
	}
}
