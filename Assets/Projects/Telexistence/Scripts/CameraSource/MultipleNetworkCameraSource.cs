using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultipleNetworkCameraSource : ICameraSource {
	
	private GstNetworkMultipleTexture m_Texture = null;
	public GameObject TargetNode;
	public int port=7000;
	public int StreamsCount=1;
	public int CameraStreams=1;
	public RobotConnectionComponent RobotConnector;
	OffscreenProcessor[] _Processor;
	bool[] _needProcessing;

	CameraConfigurations _config;

	uint[] _videoPorts;

	public bool SeparateStreams=false;


	public GstNetworkMultipleTexture Texture
	{
		get{
			return m_Texture;
		}
	}
	public GstBaseTexture GetBaseTexture()
	{
		return m_Texture;
	}
	public void SetCameraConfigurations (CameraConfigurations config)
	{
		_config = config;
		if (m_Texture != null)
			m_Texture.SetConfiguration (_config);
	}

	public ulong GetGrabbedBufferID (int index)
	{
		if(!SeparateStreams)
			index = 0;
		return m_Texture.GetGrabbedBufferID (index);
	}

	public bool IsSynced()
	{
		return m_Texture.IsSynced ();
	}

	public void Init(RobotInfo ifo,string profileType)
	{
		m_Texture= TargetNode.AddComponent<GstNetworkMultipleTexture> ();
		m_Texture.StreamsCount = StreamsCount;
		m_Texture.profileType = profileType;
		m_Texture.Initialize ();

		int texCount = m_Texture.GetTextureCount ();
		_Processor = new OffscreenProcessor[texCount];
		_needProcessing = new bool[texCount];
		for (int i = 0; i < texCount; ++i) {
			_Processor[i]=new OffscreenProcessor();
			_Processor[i].ShaderName = "Image/I420ToRGB";
			_needProcessing [i] = false;
		}
		string ip = Settings.Instance.GetValue("Ports","ReceiveHost",ifo.IP);
		m_Texture.ConnectToHost (ip, port,StreamsCount);
		m_Texture.Play ();


		if (_config != null)
			m_Texture.SetConfiguration (_config);

		m_Texture.OnFrameGrabbed+=OnFrameGrabbed;


		_videoPorts = new uint[StreamsCount];//{0,0};
		string streamsVals="";
		for (int i = 0; i < StreamsCount; ++i) {
			_videoPorts [i] = Texture.Player.GetVideoPort (i);
			streamsVals += _videoPorts [i].ToString ();
			if (i != StreamsCount - 1)
				streamsVals += ",";
		}
		RobotConnector.Connector.SendData(TxKitEyes.ServiceName,"VideoPorts",streamsVals,true);

	}

	public void Init(RobotInfo ifo)
	{
		Init (ifo, "None");
	}
	void OnFrameGrabbed(GstBaseTexture src,int index)
	{
		_needProcessing [index] = true;
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
		if(!SeparateStreams)
			e = 0;
		if (m_Texture!=null && m_Texture.PlayerTexture()!=null && (int)e < m_Texture.PlayerTexture().Length)
			return m_Texture.PlayerTexture() [(int)e];
		return null;
	}
	public Texture GetEyeTexture(int e)
	{
		if(!SeparateStreams || e>=_Processor.Length)
			e = 0;
		
		if (m_Texture !=null && ((int)e)<_Processor.Length && m_Texture.PlayerTexture ()!=null) {
			Texture tex= m_Texture.PlayerTexture () [(int)e];

			if(m_Texture.Player.Format==GstImageInfo.EPixelFormat.EPixel_I420){
				if (_needProcessing [e]) {
					_Processor[e].ProcessTexture (tex);
					_needProcessing [e] = false;
				}
				//return m_Texture.PlayerTexture [(int)e];
				return _Processor[(int)e].ResultTexture;
			}else 
				return tex;
		}
		return null;
	}
	
	public Rect GetEyeTextureCoords(int e)
	{
		if(!SeparateStreams  && CameraStreams>1)
			//return new Rect ( (float)e/(float)CameraStreams,0, 1.0f/(float)CameraStreams, 1);
		return new Rect (0, (float)e/(float)CameraStreams, 1, 1.0f/(float)CameraStreams);
		else return new Rect (0, 0, 1, 1);
	}
	public Vector2 GetEyeScalingFactor(int e)
	{
		if(!SeparateStreams && CameraStreams>1)
			return new Vector2(1,1.0f/(float)CameraStreams);//Vector2(1.0f/(float)CameraStreams,1);//
		else return Vector2.one;
	}
	public int GetCaptureRate(int e)
	{
		if(!SeparateStreams)
			e = 0;
		return GetBaseTexture ().GetCaptureRate (e);
	}

}
