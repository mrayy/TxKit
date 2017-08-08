using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MediaFileSource : ICameraSource 
{
	public MovieTexture Media;

	public MediaFileSource()
	{
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
		return null;
	}
	public void SetCameraConfigurations (CameraConfigurations config)
	{
	}

	public void Init(RobotInfo ifo)
    {
		Media = Resources.Load<MovieTexture> (Path.GetFileNameWithoutExtension (ifo.MediaPath));
		Media.Play ();
		Media.loop = true;
    }
	public void Close()
	{
		if (Media == null)
			return;
	}
	public void Pause()
	{
		if (Media == null)
			return;
	}

	public void Resume()
	{
		if (Media == null)
			return;
		Media.Play ();
		Media.loop = true;
	}

    public Texture GetEyeTexture(int e)
    {
		return Media;
    }

	public Texture GetRawEyeTexture(int e)
	{
		return GetEyeTexture (e);
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
		return 30;
	}

	public float GetAverageAudioLevel ()
	{
		return 0;
	}
	public void SetAudioVolume (float vol)
	{
	}
}
