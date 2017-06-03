using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Threading;

public class PLCDriverObject : MonoBehaviour {


	PLCDriver _driver;

	public string IP;
	public int Port;

	public int SleepInterval = 100;
	public DebugInterface DebugText;

	class PLCDebugElement:DebugInterface.IDebugElement
	{
		PLCDriverObject obj;
		public PLCDebugElement(PLCDriverObject o)
		{
			obj=o;
		}
		public string GetDebugString()
		{
			return obj.GenerateDebugString ();
		}
	}

	public class XyrisJointValues
	{
		public float FrontRightRail;
		public float BackRightRail;
		public float FrontLefttRail;
		public float BackLeftRail;
		public float LeftRail;
		public float RightRail;
		public float YBMJointPitch;
		public float BasePitch;
		public float YBMRod;
		public float YBMBaseRoll;


		public float LeftCrawlerRPM;
		public float RightCrawlerRPM;

		public int XyrisOperation;
		public int XyrisTraverse;
		
		public int YBMRodStarted;
		public int YBMEmgStop;
	}

	class JobHandler:ThreadJob
	{
		PLCDriverObject _o;
		public JobHandler(PLCDriverObject o)
		{
			_o=o;
		}
		protected override void ThreadFunction() 
		{
			while (!IsDone) {
				_o._RunJob();

			}
		}
		
		protected override void OnFinished() 
		{
		}
	}

	JobHandler _jobHandler;

	public bool Connected
	{
		get{
			return _driver.IsConnected;
		}
	}

	void _RunJob()
	{
		_driver.ReadData ();
		//_driver.WriteData ();

		Thread.Sleep (SleepInterval);
	}


	public string ObjectName="PLC";

	public string GenerateDebugString()
	{
		string str = "";

		str+="PLC Status: " + (_driver.IsConnected? "Connected":"Disconnected")+ "\n";
		return str;
	}

	// Use this for initialization
	void Start () {
		_driver = new PLCDriver ();
		
		IP=Settings.Instance.GetValue ("PLC", "IP",IP);
		int.TryParse (Settings.Instance.GetValue (ObjectName, "PLCPort",Port.ToString()), out Port);

		_driver.Connect (IP, Port);

		_jobHandler = new JobHandler (this);
		_jobHandler.Start ();

		if (DebugText != null)
			DebugText.AddDebugElement (new PLCDebugElement (this));


	}
	 void OnDestroy()
	{
		_jobHandler.Abort ();
		_driver.Destroy ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnTorsoJointValues(float[] values)
	{
		for (int i=0; i<6; ++i) {
			_driver.SetTorsoUInt ((PLCDriver.ETorsoDataField)(((int)PLCDriver.ETorsoDataField.J1_ik_angle)+i), (uint)(values [2*i+0]*100));
			_driver.SetTorsoUInt ((PLCDriver.ETorsoDataField)(((int)PLCDriver.ETorsoDataField.J1_rt_angle)+i), (uint)(values [2*i+1]*100));
		}
	}

	public float[] GetTorsoJointValues()
	{
		float[] ret = new float[6];
		for (int i=0; i<6; ++i) {
			ret[i]=(float)_driver.GetTorsoInt ((PLCDriver.ETorsoDataField)(((int)PLCDriver.ETorsoDataField.J1_rt_angle)+i));
			ret[i]*=0.01f;
		}
		return ret;
	}

	public XyrisJointValues GetXyrisJointValues()
	{
		XyrisJointValues v = new XyrisJointValues ();
		v.LeftCrawlerRPM = (float)(_driver.GetXyrisUInt (PLCDriver.EXyrisDataField.mainCrwlLeftRPM));
		v.RightCrawlerRPM = (float)(_driver.GetXyrisUInt (PLCDriver.EXyrisDataField.mainCrwlRightRPM));
		v.FrontLefttRail=(float)(_driver.GetXyrisShort (PLCDriver.EXyrisDataField.subCrwlFrontLeft));
		v.BackLeftRail=(float)(_driver.GetXyrisShort (PLCDriver.EXyrisDataField.subCrwlBackLeft));

		v.FrontRightRail=(float)(_driver.GetXyrisShort (PLCDriver.EXyrisDataField.subCrwlFrontRight));
		v.BackRightRail=(float)(_driver.GetXyrisShort (PLCDriver.EXyrisDataField.subCrwlBackRight));


		v.LeftRail=(float)(_driver.GetXyrisShort (PLCDriver.EXyrisDataField.traverseLeft));
		v.RightRail=(float)(_driver.GetXyrisShort (PLCDriver.EXyrisDataField.traverseRight));

		v.YBMJointPitch=(float)(_driver.GetYBMUShort (PLCDriver.EYbmDataField.rodPitch))/10.0f;
		v.BasePitch=(float)(_driver.GetYBMUShort (PLCDriver.EYbmDataField.basePitch))/10.0f;
		v.YBMRod = 0;//(float)(_driver.GetYBMUShort (PLCDriver.EYbmDataField.rodRaised));

		v.YBMBaseRoll=(float)(_driver.GetYBMUShort (PLCDriver.EYbmDataField.baseRoll))/10.0f;
		return v;
	}


	public float GetSpeed()
	{
		return _driver.PLCGetGnssUShort (PLCDriver.EGNSSDataField.ground_speed)/100.0f;
	}

	public void OnRobotStatus(IRobotDataCommunicator.ERobotControllerStatus status)
	{
	}
	public void OnCameraFPS(int c0,int c1)
	{
	}

	public float GetCrawlerBatteryLevel()
	{
		return _driver.GetXyrisUInt (PLCDriver.EXyrisDataField.battVoltage)/100.0f;
	}

	
	public float GetOthersBatteryLevel()
	{
		return _driver.GetCommonUShort(PLCDriver.ECommonDataField.battertVoltage)/100.0f;
	}
	public int GetUserRSSI()
	{
		return _driver.GetCommonShort(PLCDriver.ECommonDataField.rssi_base);
	}
	public int GetRobotRSSI()
	{
		return _driver.GetCommonShort(PLCDriver.ECommonDataField.rssi_robot);
	}


	public double[] GetGNSSLocation()
	{
		double[] ret = new double[2];
		ret [0] = ((double)_driver.PLCGetGnssUInt64 (PLCDriver.EGNSSDataField.latitude)) /1000000.0;
		ret [1] = ((double)_driver.PLCGetGnssUInt64 (PLCDriver.EGNSSDataField.longitude))  /1000000.0;
		ret [0] /= 100.0;
		ret [1] /= 100.0;
		ret [0] = (int)(ret [0]) + (ret [0] - (int)(ret [0]))*100.0 / 60.0;
		ret [1] = (int)(ret [1]) + (ret [1] - (int)(ret [1]))*100.0 / 60.0;

		return ret;
	}

}
