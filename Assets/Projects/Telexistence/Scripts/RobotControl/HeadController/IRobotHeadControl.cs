using UnityEngine;
using System.Collections;

public interface IRobotHeadControl  {
	
	bool GetHeadOrientation(out Quaternion q, bool abs) ;
	 bool GetHeadPosition(out Vector3 v,bool abs);
	
	 void Recalibrate();
}
