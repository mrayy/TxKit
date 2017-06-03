using UnityEngine;
using System.Collections;

public class UINEDOInterfaceControls : MonoBehaviour {


	public PLCDriverObject PLCDriver;

	public UINEDOBattery CrawlerBatteryLevel;
	public UINEDOBattery OtherBatteryLevel;
	public UINEDOSpeed SpeedMeter;
	public UINEDOWifi UserRSSI;
	public UINEDOWifi RobotRSSI;
	public UINEDOMessage MessageHandler;
	public UINEDOVehicleStatus VehicleStatus;
	public UINEDOConnectScreen ConnectScreen;
	public UINEDOPower PowerIcon;

	public GameObject ElementHandler;

	bool _isUIActive=true;

	// Use this for initialization
	void Start () {
		if (PLCDriver == null) {
			PLCDriver=GameObject.FindObjectOfType<PLCDriverObject>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (PLCDriver!=null) {
			SetCrawlerBatteryLevel(PLCDriver.GetCrawlerBatteryLevel());
			SetOthersBatteryLevel(PLCDriver.GetOthersBatteryLevel());

			
			SetUserRSSI(PLCDriver.GetUserRSSI());
			SetRobotRSSI(PLCDriver.GetRobotRSSI());
			SetSpeed(PLCDriver.GetSpeed());
		}

		if (Input.GetButtonDown ("UISwitch")) {
			_isUIActive=!_isUIActive;
			ElementHandler.SetActive(_isUIActive);
		}
	}

	public void SetCrawlerBatteryLevel(float level)
	{
		CrawlerBatteryLevel.SetLevel( level);
	}
	public void SetOthersBatteryLevel(float level)
	{
		OtherBatteryLevel.SetLevel( level);
	}

	public void SetUserRSSI(int level)
	{
		UserRSSI.SetLevel (level);
	}
	public void SetRobotRSSI(int level)
	{
		RobotRSSI.SetLevel (level);
	}

	public void SetSpeed(float speed)
	{
		SpeedMeter.SetSpeed (speed);
	}

	public void SetMessage(string msg,UINEDOMessage.MessageLevel level)
	{
		MessageHandler.SetMessage (msg, level);
	}

	public void SetPowered(bool p)
	{
		PowerIcon.SetPowered (p);
	}

	public void SetCondition(int condition)
	{
	}

	public void SetErrorMessage(ushort message)
	{
		MessageHandler.SetMessage (message);
	}
}
