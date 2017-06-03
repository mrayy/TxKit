using UnityEngine;
using System.Collections;

public class URExperimentManager : ExperimentManager {
	
	public int SamplingRate=30;
	public Transform[] Entities;

	// Use this for initialization
	protected override void Start () {
		Experiments = new Data.DataSourceManager[1];
		Experiments [0] = new URDataManagerExp1 (SamplingRate, Entities);
		base.Start ();

	}

	protected override void OnNewSubject()
	{
		base.OnNewSubject ();
		Experiments [0].Path = Application.dataPath+ "\\"+BaseExperimentsFolder+ExpPrefix +"_ExpA_"+Counter+".xls";
	}

	public override void StartCapture()
	{
		//reset rotations
		foreach(var e in Entities)
			e.localRotation = Quaternion.Inverse (e.parent.localRotation);
		base.StartCapture ();
	}


	public override void PauseCapture()
	{
		base.PauseCapture ();
	}


	// Update is called once per frame
	protected override void Update () {
		base.Update ();
	}
}
