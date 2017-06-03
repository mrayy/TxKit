using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class OvrvisionSource : ICameraSource {

	public delegate void Delg_OnFrameGrabbed(LocalWebcameraSource src,int index);
	public event Delg_OnFrameGrabbed OnFrameGrabbed;

	Texture2D[] camTex = new Texture2D[2];
	System.IntPtr[] camPtr = new System.IntPtr[2];

	COvrvisionUnity ovrPro=new COvrvisionUnity();
	public int conf_exposure = 11155;
	public int conf_gain = 27;
	public int conf_blc = 5;
	public int conf_wb_r = 1474;
	public int conf_wb_g = 1024;
	public int conf_wb_b = 1738;
	public bool conf_wb_auto = true;

	ulong _bufferID=0;

	public void UpdateOvrvisionSetting()
	{
		if (!ovrPro.camStatus)
			return;

		//set config
		{
			ovrPro.SetExposure(conf_exposure);
			ovrPro.SetGain(conf_gain);
			ovrPro.SetBLC(conf_blc);
			ovrPro.SetWhiteBalanceR(conf_wb_r);
			ovrPro.SetWhiteBalanceG(conf_wb_g);
			ovrPro.SetWhiteBalanceB(conf_wb_b);
			ovrPro.SetWhiteBalanceAutoMode(conf_wb_auto);
		}
		Thread.Sleep (100);

	}

	public OvrvisionSource()
	{
	}

	public bool IsSynced()
	{
		return true;
	}
	public ulong GetGrabbedBufferID (int index)
	{
		return _bufferID;
	}
	public GstBaseTexture GetBaseTexture()
	{
		return null;
	}
	public void Init(RobotInfo ifo)
    {
		if (!ovrPro.Open (COvrvisionUnity.OV_CAMVR_FULL, 0.15f)) {
			return;
		} else {

			Thread.Sleep (100);
			if (true)
			{
				ovrPro.SetExposure(conf_exposure);
				ovrPro.SetGain(conf_gain);
				ovrPro.SetBLC(conf_blc);
				ovrPro.SetWhiteBalanceR(conf_wb_r);
				ovrPro.SetWhiteBalanceG(conf_wb_g);
				ovrPro.SetWhiteBalanceB(conf_wb_b);
				ovrPro.SetWhiteBalanceAutoMode(conf_wb_auto);
			}
		}
		Thread.Sleep (100);
		UpdateOvrvisionSetting ();

		ovrPro.useOvrvisionTrack_Calib = true;
		ovrPro. useProcessingQuality= COvrvisionUnity.OV_CAMQT_DMSRMP;

		for (int i = 0; i < 2; ++i) {
			camTex [i] = new Texture2D (ovrPro.imageSizeW, ovrPro.imageSizeH, TextureFormat.BGRA32, false);
			camTex [i].wrapMode = TextureWrapMode.Clamp;
			camTex [i].Apply ();
			camPtr[i]=camTex [i].GetNativeTexturePtr ();
		}

		_bufferID = 0;
    }
	public void Close()
	{
		ovrPro.Close ();
		_bufferID = 0;
	}
	public void Pause()
	{
	}

	public void Resume()
	{
	}

	public Texture GetRawEyeTexture(int e)
	{
		return GetEyeTexture (e);
	}
    public Texture GetEyeTexture(int e)
    {
		if (ovrPro.UpdateImage (camPtr [0], camPtr [1])) 
		{
			++_bufferID;
		}
		return camTex [e];
    }

	public Rect GetEyeTextureCoords(int e)
	{
		return new Rect (0, 0, 1, 1);
	}
	public Vector2 GetEyeScalingFactor(int e)
	{
		return new Vector2 (1, 1);
	}
	public int GetCaptureRate(int e)
	{
		return 60;
	}

	public float GetAverageAudioLevel ()
	{
		return 0;
	}
	public void SetAudioVolume (float vol)
	{
	}
}
