using UnityEngine;
using System.Collections;

public enum EyeName
{
	LeftEye=0,
	RightEye=1
}

public interface ICameraSource  {

	GstBaseTexture GetBaseTexture();
	void Init(RobotInfo ifo);

	bool IsSynced();

	Texture GetEyeTexture(int index);
	Texture GetRawEyeTexture(int index);

	ulong GetGrabbedBufferID (int index);

	Rect GetEyeTextureCoords(int index);
	Vector2 GetEyeScalingFactor(int index);

	void Close(); 

	void Pause();
	void Resume();

	int GetCaptureRate(int e);

}
