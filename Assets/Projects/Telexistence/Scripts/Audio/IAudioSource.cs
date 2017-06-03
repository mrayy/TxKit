using UnityEngine;
using System.Collections;

public interface IAudioSource {

	void Init(RobotInfo ifo);

	void Close(); 

	void Pause();
	void Resume();

	void Update();

	float GetAverageAudioLevel ();
	void SetAudioVolume (float vol);
}
