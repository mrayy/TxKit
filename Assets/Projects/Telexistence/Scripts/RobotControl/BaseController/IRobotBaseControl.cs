using UnityEngine;
using System.Collections;

public interface IRobotBaseControl  {

	
	Vector2 GetSpeed();
	 float GetRotation();
	
	 void Recalibrate();
}
