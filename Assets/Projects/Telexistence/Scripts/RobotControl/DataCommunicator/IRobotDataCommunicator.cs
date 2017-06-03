using UnityEngine;
using System.Collections;
using System.IO;

public class IRobotDataCommunicator 
{

	public enum ERobotControllerStatus
	{
		EStopped,
		EIniting,
		EStopping,
		EDisconnected,
		EDisconnecting,
		EConnected,
		EConnecting
	};
	public delegate void Delg_OnConfig(string audioConfig);
	public delegate void Delg_OnRobotCalibrateDone();
	public delegate void Delg_OnReportMessage(int code,string msg);
	public delegate void Delg_OnBumpSensor(float[] v);
	public delegate void Delg_OnIRSensor(float[] v);
	public delegate void Delg_OnJointValues(float[] v);
	public delegate void Delg_OnBatteryLevel(int level);
	public delegate void Delg_OnMessage(int message,BinaryReader reader);
	public delegate void Delg_OnRobotInfoDetected(RobotInfo ifo);
	public delegate void Delg_OnRobotStatus(ERobotControllerStatus status);
	public delegate void Delg_OnServiceNetValue(string serviceName,int port);


	public event Delg_OnConfig OnCameraConfig;
	public event Delg_OnConfig OnAudioConfig;
	public event Delg_OnConfig OnAudioPlayerConfig;
	public event Delg_OnRobotCalibrateDone OnRobotCalibrateDone;
	public event Delg_OnReportMessage OnReportMessage;
	public event Delg_OnBumpSensor OnBumpSensor;
	public event Delg_OnIRSensor OnIRSensor;
	public event Delg_OnMessage OnMessage;
	public event Delg_OnBatteryLevel OnBatteryLevel;
	public event Delg_OnRobotInfoDetected OnRobotInfoDetected;
	public event Delg_OnRobotStatus OnRobotStatus;
	public event Delg_OnJointValues OnJointValues;
	public event Delg_OnServiceNetValue OnServiceNetValue;


	protected void _OnRobotInfoDetected(RobotInfo ifo)
	{
		if(OnRobotInfoDetected!=null)
			OnRobotInfoDetected(ifo);
	}
	protected void _OnCameraConfig(string config)
	{
		if(OnCameraConfig!=null)
			OnCameraConfig(config);
	}
	protected void _OnAudioConfig(string config)
	{
		if(OnAudioConfig!=null)
			OnAudioConfig(config);
	}
	protected void _OnAudioPlayerConfig(string config)
	{
		if(OnAudioPlayerConfig!=null)
			OnAudioPlayerConfig(config);
	}
	protected void _OnReportMessage(int code,string str)
	{
		if(OnReportMessage!=null)
		{
			OnReportMessage(code,str);
		}
	}
	protected void _OnBumpSensor(float[] values)
	{
		if(OnBumpSensor!=null)
			OnBumpSensor(values);
	}
	protected void _OnIRSensor(float[] values)
	{
		if(OnIRSensor!=null)
			OnIRSensor(values);
	}
	protected void _OnJointValues(float[] values)
	{
		if(OnJointValues!=null)
			OnJointValues(values);
	}
	protected void _OnBatteryLevel(int level)
	{
		if(OnBatteryLevel!=null)
			OnBatteryLevel(level);
	}
	protected void _OnRobotStatus(ERobotControllerStatus v)
	{
		if(OnRobotStatus!=null)
			OnRobotStatus(v);
	}
	protected void _OnServiceNetValue(string name,int port)
	{
		if(OnServiceNetValue!=null)
			OnServiceNetValue(name,port);
	}
	protected void _OnMessage(int message,BinaryReader reader)
	{
		if(OnMessage!=null)
			OnMessage(message,reader);
	}

	public virtual void Start(){}
	public virtual void Stop(){}

	public virtual void SetPort(int port){}
	public virtual int GetPort(){return 0;}
}
