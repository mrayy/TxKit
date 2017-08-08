using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocalWebcameraSource : ICameraSource {

	public delegate void Delg_OnFrameGrabbed(LocalWebcameraSource src,int index);
	public event Delg_OnFrameGrabbed OnFrameGrabbed;

	List<int> _camIndex=new List<int>();
	class CamInfo
	{
		public CamInfo(WebCamTexture c,int idx)
		{
			Cam=c;
			index=idx;
		}
		public WebCamTexture Cam;
		public int index;
	}
	List<CamInfo> _camObject=new List<CamInfo>();

	public int ResolutionX=1280;
	public int ResolutionY=720;

	public ulong GetGrabbedBufferID (int index)
	{
		return 0;
	}
	public bool IsSynced()
	{
		return true;
	}
	public LocalWebcameraSource()
	{
		for (int i = 0; i < WebCamTexture.devices.Length; ++i)
			Debug.Log ("["+i.ToString()+"]: "+WebCamTexture.devices [i].name);
	}
	public void SetCameraConfigurations (CameraConfigurations config)
	{
	}

	public void SetCameraIndex(List<int> indicies)
	{
		_camIndex = indicies;
	}
	public GstBaseTexture GetBaseTexture()
	{
		return null;
	}
	public void Init(RobotInfo ifo)
    {
		_camObject.Clear ();
		_camIndex = ifo.CameraIndex;
		for (int i = 0; i < _camIndex.Count; ++i) {
			int idx = Mathf.Min (_camIndex [i], WebCamTexture.devices.Length - 1);
			if (idx < 0)
				continue;

			CamInfo ci=null;
			for (int j = 0; j < _camObject.Count; ++j) {
				if (_camObject [j].index == idx) {
					ci=_camObject[j];
				}
			}

			if(ci!=null)
			{
				_camObject.Add(ci);
			}
			else{
				WebCamTexture c = new WebCamTexture (WebCamTexture.devices [idx].name, ResolutionX, ResolutionY);
				c.Play ();
				_camObject.Add (new CamInfo(c,idx));
			}
		}
    }
	public void Close()
	{
		for (int i = 0; i < _camObject.Count; ++i) {
			_camObject [i].Cam.Stop ();
		}
		_camObject.Clear();
	}
	public void Pause()
	{
		for (int i = 0; i < _camObject.Count; ++i) {
			_camObject [i].Cam.Pause ();
		}
	}

	public void Resume()
	{
		for (int i = 0; i < _camObject.Count; ++i) {
			_camObject [i].Cam.Play ();
		}
	}

	public Texture GetRawEyeTexture(int e)
	{
		return GetEyeTexture (e);
	}
    public Texture GetEyeTexture(int e)
    {
		if (e < _camObject.Count && _camObject[e].Cam.isPlaying) {
			if (OnFrameGrabbed != null)
				OnFrameGrabbed (this, e);
			return _camObject [e].Cam;
		}
		return null;
    }

	public Rect GetEyeTextureCoords(int e)
    {
        return new Rect(0, 0, 1, 1);
	}
	public Vector2 GetEyeScalingFactor(int e)
	{
		return Vector2.one;
	}
	public int GetCaptureRate(int e)
	{
		if (e < _camObject.Count)
			return (int)_camObject [e].Cam.requestedFPS;
		return 0;
	}

	public float GetAverageAudioLevel ()
	{
		return 0;
	}
	public void SetAudioVolume (float vol)
	{
	}
}
