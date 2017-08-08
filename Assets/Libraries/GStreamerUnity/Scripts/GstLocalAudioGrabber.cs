using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class GstLocalAudioGrabber: GstIAudioGrabber {
	

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private System.IntPtr mray_gst_createLocalAudioGrabber();

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void mray_gst_LocalAudioGrabberInit(System.IntPtr g,[MarshalAs(UnmanagedType.LPStr)]string guid,int channels,int samplingrate);

	public GstLocalAudioGrabber()
	{
		GStreamerCore.Ref();
		m_Instance = mray_gst_createLocalAudioGrabber ();
	}

	public void Init(int channels,int samplingrate)
	{
		mray_gst_LocalAudioGrabberInit (m_Instance,"",channels,samplingrate);
	}

}
