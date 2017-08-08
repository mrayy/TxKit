using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class GstIAudioGrabber {
	

	internal const string DllName = "GStreamerUnityPlugin";

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void mray_gst_AudioGrabberDestroy(System.IntPtr a);

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private bool mray_gst_AudioGrabberStart(System.IntPtr a);

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void mray_gst_AudioGrabberPause(System.IntPtr a);

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private void mray_gst_AudioGrabberClose(System.IntPtr a);

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private bool mray_gst_AudioGrabberIsStarted(System.IntPtr a);

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private uint mray_gst_AudioGrabberGetSamplingRate(System.IntPtr a);

	[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static private uint mray_gst_AudioGrabberGetChannels(System.IntPtr a);

	protected System.IntPtr m_Instance;

	public System.IntPtr Instance
	{
		get {
			return m_Instance;
		}
	}
	public GstIAudioGrabber()
	{
		GStreamerCore.Ref();
	}

	public void Destroy()
	{
		mray_gst_AudioGrabberDestroy (m_Instance);
	}

	public bool Start()
	{
		return mray_gst_AudioGrabberStart (m_Instance);
	}

	public void Pause()
	{
		mray_gst_AudioGrabberPause (m_Instance);
	}

	public void Close()
	{
		mray_gst_AudioGrabberClose (m_Instance);
	}
	public bool IsStarted()
	{
		return mray_gst_AudioGrabberIsStarted(m_Instance);
	}
	public uint GetSamplingRate()
	{
		return mray_gst_AudioGrabberGetSamplingRate(m_Instance);
	}

	public uint GetChannels()
	{
		return mray_gst_AudioGrabberGetChannels(m_Instance);
	}
}
