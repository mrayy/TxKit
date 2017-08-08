using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class GstCustomAudioGrabber: GstIAudioGrabber {
	

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private System.IntPtr mray_gst_createCustomAudioGrabber();

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void mray_gst_CustomAudioGrabberInit(System.IntPtr g,[MarshalAs(UnmanagedType.LPStr)]string pipeline,int channels,int samplingrate);

	public GstCustomAudioGrabber()
	{
		GStreamerCore.Ref();
		m_Instance = mray_gst_createCustomAudioGrabber ();
	}

	public void Init(string pipeline,int channels,int samplingrate)
	{
		mray_gst_CustomAudioGrabberInit (m_Instance,pipeline,channels,samplingrate);
	}

}
