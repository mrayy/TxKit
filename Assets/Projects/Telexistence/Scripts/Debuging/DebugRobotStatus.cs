using UnityEngine;
using System.Collections;

public class DebugRobotStatus : DebugInterface.IDebugElement {

	public RobotConnectionComponent Robot;
	TxKitBody Body;
	public DebugRobotStatus (RobotConnectionComponent r)
	{
		Robot = r;
		Body = r.GetComponent<TxKitBody> ();
	}
	
	public string GetDebugString()
	{
		RobotConnector _connector = Robot.Connector;

		string str = "";
		str+="Is  Connected: "+Robot.IsConnected+"\n";
		if (Robot.IsConnected) {
			str += "Is Robot Connected: " + Robot.IsRobotConnected + "\n";
			str += "Robot Status: " + Robot.RobotStatus + "\n";
		}
		if (Body != null) {
			str += "Head Position= " + Body.HeadPosition.ToString ("F3") + "\n";
			str += "Head Rotation= " + Body.HeadOrientation.eulerAngles.ToString ("F2") + "\n";
		
			str += "Rotation Speed= " + Body.BaseRotation.ToString () + "\n";
			str += "Motion Speed= " + Body.BaseSpeed.ToString () + "\n";
		}
		float[] jv = Robot.RobotJointValues;
		if (jv != null) {
			str+="Robot Joint Values: \n";
			for(int i=0;i<jv.Length;)
			{
				str+=jv[i].ToString() + "/ " +jv[i+1];
				if(i!=jv.Length-2)
					str+="\n";
				i+=2;
			}
		}
		return str;
	}
}
