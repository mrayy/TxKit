using UnityEngine;
using System.Collections;

public class FileCameraSource : ICameraSource {
	
	private GstCustomTexture m_Texture = null;
	public string FileName;
	public GameObject TargetNode;
	OffscreenProcessor _Processor;
	bool _needProcessing;

	public AudioSource AudioObject;
	GstAudioPlayer _player;

	public GstCustomTexture Texture
	{
		get{
			return m_Texture;
		}
	}
	public ulong GetGrabbedBufferID (int index)
	{
		return 0;
	}
	public bool IsSynced()
	{
		return true;
	}
	public GstBaseTexture GetBaseTexture()
	{
		return m_Texture;
	}
	public void SetCameraConfigurations (CameraConfigurations config)
	{
	}

	public void Init(RobotInfo ifo)
	{
		GStreamerCore.Ref();
		m_Texture= TargetNode.AddComponent<GstCustomTexture> ();
		m_Texture.Initialize ();

		FileName = ifo.URL;

		_Processor=new OffscreenProcessor();
		_Processor.ShaderName = "Image/I420ToRGB";
		_needProcessing = false;

		string path = Application.dataPath + "/" + FileName;
		m_Texture.SetPipeline ("filesrc location=\"" + path + "\" ! qtdemux name=demux " +
			"demux.video_0 ! queue ! avdec_h264 ! videoconvert ! video/x-raw,format=I420 ! appsink name=videoSink " +
			"demux.audio_0 ! queue ! decodebin ! audioconvert ! volume volume=5 ! appsink name=audioSink");
		m_Texture.OnFrameGrabbed+=OnFrameGrabbed;
		m_Texture.Play ();


		GameObject audioObj=new GameObject("AudioObject_"+TargetNode.name);
		audioObj.transform.parent = TargetNode.transform;
		audioObj.transform.position = Vector3.zero;
		AudioObject=audioObj.AddComponent<AudioSource> ();
		AudioObject.loop = true;
		_player= audioObj.AddComponent<GstAudioPlayer> ();
		_player.Player = m_Texture.Player.AudioWrapper;
		_player.TargetSrc = AudioObject;
	}
	void OnFrameGrabbed(GstBaseTexture src,int index)
	{
		_needProcessing = true;
	}

	public void Close()
	{
		if (m_Texture != null) {
			m_Texture.Close();
			m_Texture.Destroy ();
			GameObject.Destroy(m_Texture);
			m_Texture=null;
		}
	}

	public void Pause()
	{
		if (m_Texture != null) {
			m_Texture.Pause ();
		}
	}

	public void Resume()
	{
		if (m_Texture != null) {
			m_Texture.Play ();
		}
	}
	public Texture GetRawEyeTexture(int e)
	{
		if (m_Texture== null || m_Texture.PlayerTexture() ==null || m_Texture.PlayerTexture().Length== 0)
			return null;
		return m_Texture.PlayerTexture() [0];
	}
	public Texture GetEyeTexture(int e)
	{
		if (m_Texture== null || m_Texture.PlayerTexture() ==null || m_Texture.PlayerTexture().Length== 0)
			return null;

		if (_needProcessing ) {
			_Processor.ProcessTexture (m_Texture.PlayerTexture() [0]);
			_needProcessing = false;
		}
		//return m_Texture.PlayerTexture [(int)e];
		return _Processor.ResultTexture;
	}
	
	public Rect GetEyeTextureCoords(int e)
	{
		return new Rect (0, 0, 1, 1);
	}
	public Vector2 GetEyeScalingFactor(int e)
	{
		return Vector2.one;
	}
	public int GetCaptureRate(int e)
	{
		return GetBaseTexture ().GetCaptureRate (0);
	}


	public float GetAverageAudioLevel ()
	{
		return _player.averageAudio;
	}
	public void SetAudioVolume (float vol)
	{
		_player.Volume = vol;
	}
}
