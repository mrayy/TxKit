using UnityEngine;
using System.Collections;

public class TransitionNodesManager : MonoBehaviour {

	TransitionNode[] Nodes;
	// Use this for initialization
	void Start () {
		Nodes = GameObject.FindObjectsOfType<TransitionNode> ();
		foreach (var n in Nodes)
			n.SetTransitionManager (this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTransition(TransitionNode n)
	{
		foreach(var v in Nodes)
		{
			v.gameObject.SetActive(true);
		}
		n.gameObject.SetActive(false);
	}
}
