using UnityEngine;
using System.Collections;

public interface IAudioStream {

	void Init(RobotInfo ifo);

	void Close(); 

	void Pause();
	void Resume();

	void Update();

	void SetAudioVolume (float vol);
}
