using UnityEngine;
using System.Collections;

public class VideoParametersController : MonoBehaviour {

	public int ExposureValue;
	public int GainValue;
	public int GammaValue;
	public int BrightnessValue;
	public int ContrastValue;
	public int SaturationValue;
	public int WhiteBalanceValue;

	// Use this for initialization
	void Start () {
		GainValue=PlayerPrefs.GetInt ("Robot.Gain",-1);
		ExposureValue=PlayerPrefs.GetInt ("Robot.Exposure",0);
		GammaValue=PlayerPrefs.GetInt ("Robot.Gamma",0);
		BrightnessValue=PlayerPrefs.GetInt ("Robot.Brightness",0);
		ContrastValue=PlayerPrefs.GetInt ("Robot.Contrast",0);
		SaturationValue=PlayerPrefs.GetInt ("Robot.Saturation",0);
		WhiteBalanceValue=PlayerPrefs.GetInt ("Robot.WhiteBalance",0);
	}

	void OnDestroy()
	{
		PlayerPrefs.SetInt ("Robot.Gain", GainValue);
		PlayerPrefs.SetInt ("Robot.Exposure", ExposureValue);
		PlayerPrefs.SetInt ("Robot.Gamma", GammaValue);
		PlayerPrefs.SetInt ("Robot.Brightness", BrightnessValue);
		PlayerPrefs.SetInt ("Robot.Contrast", ContrastValue);
		PlayerPrefs.SetInt ("Robot.Saturation", SaturationValue);
		PlayerPrefs.SetInt ("Robot.WhiteBalance", WhiteBalanceValue);
	}

	void SetValue(NetValueObject obj,string name,float val)
	{
		NetValueObject.ValueControllerCtl c=obj.GetValue (name);
		string value = val.ToString ();
		if (val < 0)
			value = "auto";
		if (c != null)
			c.value = value;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.PageDown)) {
			GainValue -= 1;
			if (GainValue < 0)
				GainValue = -1;
		}
		if (Input.GetKeyDown (KeyCode.PageUp)) {
			if (GainValue < 0)
				GainValue = 0;
			GainValue += 1;
	//		if (GainValue > 10)
	//			GainValue = 10;
		}

	}
	public void UpdateValuesObject(NetValueObject obj)
	{
		SetValue (obj,"Camera.Gain", GainValue);
		SetValue (obj,"Camera.Brightness", BrightnessValue);
		SetValue (obj,"Camera.Gamma", GammaValue);
		SetValue (obj,"Camera.Exposure", ExposureValue);
		SetValue (obj,"Camera.Contrast", ContrastValue);
		SetValue (obj,"Camera.Saturation", SaturationValue);
		SetValue (obj,"Camera.WhiteBalance", WhiteBalanceValue);

	}
}
