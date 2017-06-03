using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;

public class GstAudioPlayer:MonoBehaviour  {
	//public GstNetworkAudioPlayer Player;
	public IGstAudioPlayer Player;
	public GstAudioPacketGrabber grabber;
	public AudioSource TargetSrc;
	AudioClip TargetClip;

	public int PacketsCount;
	public int WaitCount=0;

	public float averageAudio=0;
	public bool SupportSpatialAudio=true;

	public enum SourceChannel
	{
		Both,
		Left,
		Right
	}

	public SourceChannel Channel=SourceChannel.Both;

	float _volume=1;

	bool _paused=false;
	public float Volume
	{
		set{ 
			_volume = value;
			if (_volume < 0)
				_volume = 0;
			else if (_volume > 2)
				_volume = 2;
		}
		get{
			return _volume;
		}
	}

	class AudioPacket
	{
		public float[] data=new float[1];	
		public int startIndex=0;
		public int channelsCount;
	}

	MovingAverageF _movingAverage = new MovingAverageF (50);

	List<AudioPacket> _graveYard=new List<AudioPacket>();
	List<AudioPacket> _packets=new List<AudioPacket>();

	object _dataMutex=new object();

	// Use this for initialization
	void Start () {

		//if (SupportSpatialAudio) {
		//	Debug.Log ("AudioSettings.outputSampleRate: " + AudioSettings.outputSampleRate.ToString ());
			_CreateAudioClip (32000);


		//}
	}

	void _CreateAudioClip(int freq)
	{
		if (freq == 0)
			return;
		TargetSrc.Stop ();
		/*
		TargetClip = AudioClip.Create (name + "_Clip", freq, 1, freq, true,true, OnAudioRead,OnAudioSetPosition);
*/
		TargetClip = AudioClip.Create (name + "_Clip", 1, 1, AudioSettings.outputSampleRate, false);
		TargetClip.SetData(new float[] { 1 }, 0);
		TargetSrc.clip = TargetClip;
		TargetSrc.loop = true;
		TargetSrc.spatialBlend=SupportSpatialAudio?1.0f:0.0f;
		TargetSrc.Play ();

		Debug.Log ("Creating AudioClip with Frequency: " + freq.ToString ());
	}

	void OnAudioSetPosition(int newPosition) {
	//	position = newPosition;
	}
	
	// Update is called once per frame
	void Update () {

		if (SupportSpatialAudio && Player.IsPlaying ()) {
			if (TargetClip==null || TargetClip.frequency != Player.SampleRate ()) {
				_CreateAudioClip (Player.SampleRate ());
			}
		}

	}

	void OnDestroy()
	{
	}

	public void PauseAudio()
	{
		if(TargetSrc!=null)
			TargetSrc.Stop ();
		_paused = true;
	}
	public void ResumeAudio()
	{
		if(TargetSrc!=null)
			TargetSrc.Play ();
		_paused = false;
		lock (_dataMutex) {
			_packets.Clear ();
			PacketsCount = _packets.Count;
		}
	}

	public void AddAudioPacket(float[] data,int startIndex, int channels)
	{
		AudioPacket packet=CreatePacket();
		packet.data = data;
		packet.channelsCount = channels;
		packet.startIndex = startIndex;

		lock (_dataMutex) {
			_packets.Add (packet);
			PacketsCount = _packets.Count;
		}
	}
	AudioPacket GetExistingPacket()
	{
		if (_packets.Count > 0) {
			AudioPacket p;
			lock (_dataMutex) 
			{
				p = _packets [0];
				_packets.RemoveAt (0);
			}
			return p;
		}
		return null;
	}
	AudioPacket CreatePacket()
	{
		lock (_dataMutex) 
		{
			if (_graveYard.Count > 0) {
				AudioPacket p = _graveYard [0];
				_graveYard.RemoveAt (0);
				p.startIndex = 0;
				return p;
			}
		}


		return new AudioPacket ();
	}

	void RemovePacket(AudioPacket p)
	{
		lock (_dataMutex) 
		{
			_graveYard.Add (p);
		}
	}
	void OnAudioFilterRead(float[] data, int channels)
	{
		//if(!SupportSpatialAudio)
			ReadAudio (data, channels);
	}


	void OnAudioRead(float[] data) {
		ReadAudio (data, 1);
	}
	void ReadAudio(float[] data, int channels)
	{
		if (!Player.IsLoaded() || !Player.IsPlaying() 
			|| _paused)
			return;
		int length = 0;
		int timeout = 0;
		float average = 0;
		int DataLength = data.Length;
		int srcChannelsCount = 2;
		int targetLength = DataLength;

		int channelIndex = 0;
		int stepSize = 1;

		while (length < DataLength) {
			AudioPacket p;

			lock (_dataMutex) {
				p = GetExistingPacket ();
			}
			if (p == null) {
				if (!_Process ()) {
					WaitCount++;
					++timeout;
					if (timeout > 20)
						break;
				} else
					timeout = 0;
				continue;
			}

			srcChannelsCount = p.channelsCount;
			if (srcChannelsCount == 2 && this.Channel != SourceChannel.Both) {
				srcChannelsCount = 1;
				stepSize = 2;
				channelIndex = (this.Channel == SourceChannel.Left ? 0 : 1);
			}
			/*
			if (channels == 2 && srcChannelsCount == 1 )
				targetLength = DataLength / 2;
			else
				targetLength = DataLength;*/

			//calculate the left amount of data in this packet
			int sz = Mathf.Max(0,p.data.Length - p.startIndex);
			//determine the amount of data we going to use of this packet
			int count = Mathf.Min (sz, 
				Mathf.Max(0,data.Length - length)/*Remaining data to be filled*/);
			/*
			if (channels == srcChannelsCount) {
				for (int i = 0,j=0; i < count; i+=stepSize,++j) {
					data [j + length] *= p.data [p.startIndex + i+channelIndex]*Volume;
					average += p.data [p.startIndex + i+channelIndex]*p.data [p.startIndex + i+channelIndex];
				}
			} else if (channels == 2 && srcChannelsCount == 1) {
				for (int i = 0,j=0; i < count;) {
					data [2*j + length] *= p.data [p.startIndex + i+channelIndex]*Volume;
					data [2*j+ length + 1] *= p.data [p.startIndex + i+channelIndex]*Volume;
					average += p.data [p.startIndex + i+channelIndex]*p.data [p.startIndex + i+channelIndex];
					i += stepSize;
					j++;
				}
			} else if (channels == 1 && srcChannelsCount == 2) {
				for (int i = 0; i < count; i++) {
					data [i + length] *= p.data [p.startIndex + 2 * i]*Volume;
					average += p.data [p.startIndex + 2 *i]*p.data [p.startIndex + 2 *i];
				}
			}*/

			average+= GstNetworkAudioPlayer.ProcessAudioPackets (p.data, p.startIndex, channelIndex, count, stepSize, srcChannelsCount, data, length, channels);

			lock (_dataMutex) {
				if (count+p.startIndex < p.data.Length) {
					p.startIndex = count+p.startIndex;
					_packets.Insert (0,p);
				} else
					RemovePacket (p);
			}
			length += count;
		}

		average /= (float)targetLength;
		_movingAverage.Add (average,0.5f);
		averageAudio = Mathf.Sqrt(_movingAverage.Value ());//20*Mathf.Log10(Mathf.Sqrt(_movingAverage.Value ()));// (_movingAverage.Value ());
		//if(averageAudio<-100)averageAudio=-100;
	}

	public bool _Process()
	{
		return false;// grabber.ProcessPackets ();
		/*
		if (!Player.IsUsingCustomOutput ())
			return false;
		if (!Player.IsLoaded() || !Player.IsPlaying() )
			return false;


		AudioPacket p;
		if (Player.GrabFrame ()) {
			int sz = Player.GetFrameSize ();
			lock (_dataMutex) {
				p = CreatePacket ();
			}
			if (sz != p.data.Length) {
				p.data = new float[sz];
			}
			Player.CopyAudioFrame (p.data);

			lock (_dataMutex) {
				_packets.Add (p);
				PacketsCount = _packets.Count;
			}
		} else
			return false;

		return true;*/

	}

	void OnDrawGizmos()
	{
		
		Gizmos.DrawWireSphere (transform.position, averageAudio*300.0f);
	}
}
