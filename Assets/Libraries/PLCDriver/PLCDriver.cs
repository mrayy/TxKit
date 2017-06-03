using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class PLCDriver  {
	
	public enum ETorsoDataField
	{
		status,
		userConnected,
		robotConnected,
		overCurrent,
		cameraFPS,
		oculusFPS,
		J1_rt_angle,
		J2_rt_angle,
		J3_rt_disp,
		J4_rt_angle,
		J5_rt_angle,
		J6_rt_angle,
		J1_ik_angle,
		J2_ik_angle,
		J3_ik_disp,
		J4_ik_angle,
		J5_ik_angle,
		J6_ik_angle,
		J1_current,
		J2_current,
		J3_current,
		J4_current,
		J5_current,
		J6_current,
		J1_torque,
		J2_torque,
		J3_torque,
		J4_torque,
		J5_torque,
		J6_torque,
		unused,
		collisionYBM
	};
	
	
	public enum EXyrisDataField
	{
		terminal,
		offline,
		inOperation,
		inTraverse,
		subcrawler,
		offroad,
		mainCrwlRightRPM,
		mainCrwlLeftRPM,
		mainCrwlRightRotDir,
		mainCrwlLeftRotDir,
		subCrwlFrontRight,
		subCrwlFrontLeft,
		subCrwlBackRight,
		subCrwlBackLeft,
		pitch,
		roll,
		traverseRight,
		traverseLeft,
		battVoltage,
		battCurrent,
		forwardControlMode,
		traverseControlMode,
		mainCrwlSpeedLimit,
		subCrwlSpeedLimit,
		maxTempLeft,
		maxTempRight,
		unused0,
		HealthCheckCount,
	};
	
	public enum EYbmDataField
	{
		currentDepth,
		currentLoad,
		currentHtCount,
		currentSoil,
		currentTime,
		comments,
		water_pressure,
		penitrationSound,
		Nvalue,
		surveyNo,
		measurementNo,
		unused1,
		remainRecData,
		ybmBattVoltage,
		ybmBattCurrent,
		rotMotInsCurrent,
		feedMotInsCurrent,
		operatingTime,
		baseRoll,
		basePitch,
		rodPitch,
		rodRoll,
		rodRaised,
		communicationCount,
		feedRise,
		feedModeSwitching,
		feedModeManual,
		FeedModeAuto,
		feedDown,
		inManualMode,
		unused2,
		load25kgDropped,
		load50kgDropped,
		load75kDropped,
		load100kgDropped,
		rotation,
		rotationSwitching,
		rodModeManual,
		rodHalfRotation,
		unused3,
		testContRotation,
		testHalfTurn,
		rodStarted,
		rodStopped,
		inverterON,
		chuckOpen,
		chuckClose,
		clampOpen,
		clampClose,
		graspSortButton,
		rodCuttingOp1,
		rodCuttingOp2,
		rodCuttingOp3,
		rodCuttingOp4,
		emgStop,
		testStarted
	};

	
	public enum ECommonDataField {		
		batteryCurrent,
		battertVoltage,
		commonUnused,
		rssi_base,
		rssi_robot
	};
	public enum EGNSSDataField
	{
		gps_locked,
		latitude,
		longitude,
		altitude,
		true_heading,
		magnetic_heading,
		ground_speed,
		gnss_pitch,
		gnss_roll
	};

	internal const string DllName = "PLCUnityPlugin";
	
	
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private IntPtr CreatePLCDriver(  );
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private bool PLCDriverConnect(IntPtr driver, [MarshalAs(UnmanagedType.LPStr)]string ip, int port );
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private bool PLCDriverIsConnected(IntPtr driver );
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private bool PLCDriverDisconnect(IntPtr driver);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void PLCDriverDestroy(IntPtr driver);
	
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void PLCDriverRead(IntPtr driver);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void PLCDriverWrite(IntPtr driver);
	
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void PLCSetTorsoUInt(IntPtr driver,int data,uint v);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void PLCSetTorsoUShort(IntPtr driver,int data,ushort v);

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private uint PLCGetTorsoUInt(IntPtr driver,int data);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private int PLCGetTorsoInt(IntPtr driver,int data);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private ushort PLCGetTorsoUShort(IntPtr driver,int data);
	
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void PLCSetXyrisUInt(IntPtr driver,int data,uint v);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void PLCSetXyrisUShort(IntPtr driver,int data,ushort v);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private uint PLCGetXyrisUInt(IntPtr driver,int data);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private ushort PLCGetXyrisUShort(IntPtr driver,int data);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private short PLCGetXyrisShort(IntPtr driver,int data);

	
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private short PLCGetCommonShort(IntPtr driver,int data);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private ushort PLCGetCommonUShort(IntPtr driver,int data);

	
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private  byte PLCGetYbmUChar(IntPtr driver,int data);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private ushort PLCGetYbmUShort(IntPtr driver,int data);
	
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private ulong PLCGetGnssUInt64(IntPtr driver,int data);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private ushort PLCGetGnssUShort(IntPtr driver,int data);
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private int PLCGetGnssInt(IntPtr driver,int data);

	IntPtr _instance;

	public bool IsConnected {
		get {
			return PLCDriverIsConnected (_instance);
		}
	}

	public System.IntPtr NativePtr
	{
		get
		{
			return _instance;
		}
	}


	public PLCDriver()
	{
		_instance = CreatePLCDriver ();
	}
	
	public void Destroy()
	{
		PLCDriverDestroy (_instance);
	}
	public bool Connect(string ip,int port)
	{
		return PLCDriverConnect (_instance,ip, port);
	}
	public void Disconnect()
	{
		PLCDriverDisconnect (_instance);
	}
	
	public void SetTorsoUInt(ETorsoDataField data,uint v)
	{
		PLCSetTorsoUInt (_instance, (int)data,v);
	}
	public void SetTorsoUShort(ETorsoDataField data,ushort v)
	{
		PLCSetTorsoUShort (_instance, (int)data,v);
	}
	public uint GetTorsoUInt(ETorsoDataField data)
	{
		return PLCGetTorsoUInt (_instance, (int)data);
	}
	public int GetTorsoInt(ETorsoDataField data)
	{
		return PLCGetTorsoInt (_instance, (int)data);
	}
	public ushort GetTorsoUShort(ETorsoDataField data)
	{
		return PLCGetTorsoUShort (_instance, (int)data);
	}


	public uint GetXyrisUInt(EXyrisDataField data)
	{
		return PLCGetXyrisUInt (_instance, (int)data);
	}
	public ushort GetXyrisUShort(EXyrisDataField data)
	{
		return PLCGetXyrisUShort (_instance, (int)data);
	}
	public short GetXyrisShort(EXyrisDataField data)
	{
		return PLCGetXyrisShort (_instance, (int)data);
	}
	
	public ushort GetYBMUShort(EYbmDataField data)
	{
		return PLCGetYbmUShort (_instance, (int)data);
	}
	public byte GetYBMUChar(EYbmDataField data)
	{
		return PLCGetYbmUChar (_instance, (int)data);
	}

	
	public ushort GetCommonUShort(ECommonDataField data)
	{
		return PLCGetCommonUShort (_instance, (int)data);
	}
	public short GetCommonShort(ECommonDataField data)
	{
		return PLCGetCommonShort (_instance, (int)data);
	}

	public ulong PLCGetGnssUInt64(EGNSSDataField data)
	{
		return PLCGetGnssUInt64 (_instance, (int)data);
	}
	public ushort PLCGetGnssUShort(EGNSSDataField data)
	{
		return PLCGetGnssUShort (_instance, (int)data);
	}
	public int PLCGetGnssInt(EGNSSDataField data)
	{
		return PLCGetGnssInt (_instance, (int)data);
	}

	public void ReadData()
	{
		PLCDriverRead (_instance);
	}
	public void WriteData()
	{
		PLCDriverWrite (_instance);
	}
}
