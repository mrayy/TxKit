using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class GstAudioPacketGrabber  {

	static int _GRABBER_ID=0;

	int _ID=0;
	public IGstAudioPlayer Player;

	public List<GstAudioPlayer> AttachedPlayers=new List<GstAudioPlayer>();

	class AudioPacket
	{
		public float[] data=new float[1];	
		public int startIndex=0;

	}
	object _dataMutex=new object();

	List<AudioPacket> _graveYard=new List<AudioPacket>();
	List<AudioPacket> _packets=new List<AudioPacket>();

	public int PacketsCount;
	public int WaitCount=0;

	Thread _processingThread;
	bool _isDone=false;

	AudioPacket GetExistingPacket()
	{
		if (_packets.Count > 0) {

			AudioPacket p = _packets [0];
			_packets.RemoveAt (0);
			return p;
		}
		return null;
	}
	AudioPacket CreatePacket()
	{
		if (_graveYard.Count > 0) {
			AudioPacket p=_graveYard [0];
			_graveYard.RemoveAt (0);
			p.startIndex = 0;
			return p;
		}return new AudioPacket ();
	}

	public GstAudioPacketGrabber()
	{
		_ID = _GRABBER_ID++;
		_processingThread = new Thread(new ThreadStart(this.ProcessPackets));
		_processingThread.Start();
	}
	public void Close()
	{
		_isDone = true;
		if(_processingThread !=null) _processingThread.Join();
		_processingThread = null;
	}

	void RemovePacket(AudioPacket p)
	{
		_graveYard.Add (p);
	}

	bool _ProcessPackets()
	{
		//check if we have packets
		if (!_Process ())
			return false; //no packets available
		else
		{

			AudioPacket p;
			int channelsCount = 2;

			lock (_dataMutex) {
				p = GetExistingPacket ();
				channelsCount = Player.ChannelsCount ();
			}

			//Broadcast the grabbed audio packets to the attached players
			foreach (var player in AttachedPlayers) {
				player.AddAudioPacket (p.data, p.startIndex, channelsCount);
			}
		}
		return true;
	}



	bool _Process()
	{
		if (!Player.IsUsingCustomOutput () )
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
				if (_packets.Count > 3)
					_packets.RemoveAt (0);
				PacketsCount = _packets.Count;
			}
		} else
			return false;

		return true;
	}

	void ProcessPackets()
	{
		Debug.Log("Starting AudioGrabber Process: "+this._ID.ToString());
		while(!_isDone)
		{
			_ProcessPackets ();
		}
		Debug.Log("Finished AudioGrabber Process: "+this._ID.ToString());
	}
}
