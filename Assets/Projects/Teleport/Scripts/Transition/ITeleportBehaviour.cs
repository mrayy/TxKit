using UnityEngine;
using System.Collections;

public abstract class ITeleportBehaviour : MonoBehaviour {


	public delegate void Deleg_OnEntered(ITeleportBehaviour t);
	public delegate void Deleg_OnExitted(ITeleportBehaviour t);

	public  Deleg_OnEntered OnEntered;
	public  Deleg_OnExitted OnExitted;
	protected AnimationCurve _curve;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public virtual void Update (float t) {
	
	}

	public void SetAnimationCurve (AnimationCurve c){
		_curve = c;
	}
	public abstract void OnEnter (TxKitEyes camera);
	public abstract void OnExit ();
	public abstract bool IsActive ();
}
