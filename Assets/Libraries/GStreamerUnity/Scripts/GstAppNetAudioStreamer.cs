using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class GstAppNetAudioStreamer:IGstStreamer {


	
	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private System.IntPtr mray_gst_createAppNetAudioStreamer();


	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void mray_gst_AppNetAudioStreamerSetIP(System.IntPtr a,[MarshalAs(UnmanagedType.LPStr)]string ip,uint port,bool rtcp);

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void mray_gst_AppNetAudioStreamerAttachGrabber(System.IntPtr a,System.IntPtr g);



	public GstAppNetAudioStreamer()
	{
		GStreamerCore.Ref();
		m_Instance = mray_gst_createAppNetAudioStreamer();	
	}

	public void SetIP(string ip,uint port)
	{
		mray_gst_AppNetAudioStreamerSetIP (m_Instance, ip, port, false);
	}
	
	public void AttachGrabber(GstIAudioGrabber g)
	{
		mray_gst_AppNetAudioStreamerAttachGrabber (m_Instance, g.Instance);
	}

}
