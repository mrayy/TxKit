using UnityEngine;
using System.Collections;

public class DebugCameraSettings : DebugInterface.IDebugElement {

	public TELUBeeConfiguration Config;
	public DebugCameraSettings (TELUBeeConfiguration r)
	{
		Config = r;
	}
	
	public string GetDebugString()
	{
		CameraConfigurations settings = Config.CamSettings;

		string str = "";
		str += "Camera Profile: " + settings.Name + "\n";
		str+="Image Offsets: \n";
		str += "\tLeft: " + settings.PixelShiftLeft.ToString ("F0")+"\n";
		str += "\tRight: " + settings.PixelShiftRight.ToString ("F0")+"\n";
		
		str += "Camera FOV: " + ((int)settings.FoV).ToString ()+"\n";
		return str;
	}
}
