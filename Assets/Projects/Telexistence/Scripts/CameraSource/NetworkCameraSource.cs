using UnityEngine;
using System.Collections;

public class NetworkCameraSource : ICameraSource {
	
	private GstNetworkTexture m_Texture = null;
	public GameObject TargetNode;
	public int port=7000;
	public bool isStereo = false;
	
	public GstBaseTexture GetBaseTexture()
	{
		return m_Texture;
	}
	public bool IsSynced()
	{
		return true;
	}
	public ulong GetGrabbedBufferID (int index)
	{
		return 0;
	}
	public void Init(RobotInfo ifo)
	{
		m_Texture= TargetNode.AddComponent<GstNetworkTexture> ();
		m_Texture.Initialize ();

		string ip = Settings.Instance.GetValue("Ports","ReceiveHost",ifo.IP);
		m_Texture.ConnectToHost (ip, port);
		m_Texture.Play ();
	}
	public void SetCameraConfigurations (CameraConfigurations config)
	{
	}

	public void Close()
	{
		if (m_Texture != null) {
			m_Texture.Close();
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
		return GetEyeTexture (e);
	}
	public Texture GetEyeTexture(int e)
	{
		if (m_Texture == null || m_Texture.PlayerTexture() == null)
			return null;
		return m_Texture.PlayerTexture()[0];
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
	public int GetCaptureRate(int e)
	{
		return GetBaseTexture ().GetCaptureRate (0);
	}

	public float GetAverageAudioLevel ()
	{
		return 0;
	}
	public void SetAudioVolume (float vol)
	{
	}
}
