using UnityEngine;
using System.Collections;

public class URDataManagerExp1 : Data.DataSourceManager {

	Transform[] _entities;
	int SamplingRate=30;
	public URDataManagerExp1(int samplingRate,Transform[] entities):base("Experiment 1")
	{
		_entities=entities;
		this.SamplingRate = samplingRate;
	}
	public override void Init ()
	{
		base.Init ();

		AddDataSource (new ObjAngleY(_entities[0].transform));
		AddDataSource (new ObjAngleZ(_entities[1].transform));
		AddDataSource (new ObjAngleZ(_entities[2].transform));
		/*foreach(var e in _entities)
			AddDataSource (new ObjAngleY(e));*/
		//AddDataSource (new ObjAngleZ(_robot.transform));

		AddSamplingCondition (new Data.TimeOutSamplingCondition (1000/SamplingRate));
	}

	public override bool Sample ()
	{
		return base.Sample ();
	}

}
