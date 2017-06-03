using UnityEngine;
using System.Collections;

public class WebRTCCameraSource : ICameraSource {

	public bool isStereo = true;
	RTCGetUserMedia _userMedia;
	WebRTCObjectHandler _rtcHandler;
	GameObject _webrtcGameObject;
	GameObject _target;
	GameObject _owner;
	int _Width,_Height;
	public WebRTCCameraSource(GameObject target,int Width,int Height)
	{
		_owner = target;
		_target = new GameObject (target.name + "_webRTC");
		_target.transform.parent = target.transform;
		_Width = Width;
		_Height = Height;
	}

	public bool IsSynced()
	{
		return true;
	}
	public ulong GetGrabbedBufferID (int index)
	{
		return 0;
	}
	public GstBaseTexture GetBaseTexture(){
		return null;
	}

	public void Init(RobotInfo ifo)
	{	
		_rtcHandler = _owner.GetComponent<WebRTCObjectHandler> ();
		if(_rtcHandler==null)
			_rtcHandler= _owner.AddComponent<WebRTCObjectHandler> ();

		CoherentUIView _view = _rtcHandler.GetOrCreateView ();
		_view.Width = _Width;
		_view.Height = _Height;
		_view.Page = ifo.URL;

		_view.Reload (true);

		_userMedia=_target.AddComponent<RTCGetUserMedia> ();
		_userMedia.Init (_view);
	}
	public Texture GetEyeTexture(int e)
	{
		return _rtcHandler.GetTexture();
	}
	public Texture GetRawEyeTexture(int e)
	{
		return GetEyeTexture (e);
	}

	public Rect GetEyeTextureCoords(int e)
	{
		if (isStereo) {
			if ((EyeName)e == EyeName.LeftEye)
				return new Rect (0, 0, 0.5f, 1);
			return new Rect (0.5f, 0, 0.5f, 1);
		}else
			return new Rect (0, 0, 1, 1);
	}
	public Vector2 GetEyeScalingFactor(int e)
	{
		if (isStereo) 
			return new Vector2 (0.5f, 1);
		return Vector2.one;
	}
	public void Pause()
	{
		if (_rtcHandler!=null && _rtcHandler.IsCreated()) {
			_rtcHandler.GetOrCreateView ().View.TriggerEvent ("close");
		}
	}
	public void Resume()
	{
		if (_rtcHandler!=null && _rtcHandler.IsCreated()) {
			_rtcHandler.GetOrCreateView ().View.TriggerEvent ("open");
		}
	}
	public void Close()
	{
		if (_rtcHandler) {
			_rtcHandler.Close ();
		}
		GameObject.Destroy (_target);
	}

	public int GetCaptureRate(int e)
	{
		return 0;
	}


	public float GetAverageAudioLevel ()
	{
		return 0;
	}
	public void SetAudioVolume (float vol)
	{
		_rtcHandler.GetOrCreateView ().View.SetMasterVolume (vol);
	}
}
