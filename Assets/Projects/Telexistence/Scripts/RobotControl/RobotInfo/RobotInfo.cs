using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;
using System.Collections.Generic;

public class RobotInfo  {

	public enum EConnectionType
	{
		Local=0,
		Ovrvision,
		RTP,
		WebRTC,
		Movie
	}

	public enum EStreamCodec
	{
		Raw,
		Coded,
		Ovrvision
	}
    public long ID;
    public string Name;
    public string IP;
	public string URL;// for WebRTC type
    public string Location;
    public double lng;
    public double lat;
	public int communicationPort;

	public EStreamCodec StreamCodec;

    public bool Connected;
    public bool Available;

	public List<int> CameraIndex=new List<int>();
	public string MediaPath;

	public EConnectionType ConnectionType;

	public void LoadXML(XmlReader r)
	{
		Name=r.GetAttribute ("Name");
		string type = r.GetAttribute ("Type");
		ConnectionType = EConnectionType.RTP;
		if (type == "WebRTC")
			ConnectionType = EConnectionType.WebRTC;
		if (type == "Ovrvision")
			ConnectionType = EConnectionType.Ovrvision;
		else if (type == "RTP")
			ConnectionType = EConnectionType.RTP;
		else if (type == "Local") {
			ConnectionType = EConnectionType.Local;
			int count;
			int.TryParse (r.GetAttribute ("Count"), out count);
			for (int i = 0; i < count; ++i) {
				int idx;
				if (int.TryParse (r.GetAttribute ("Index" + i.ToString ()), out idx)) {
					CameraIndex.Add (idx);
				}
			}
		}
		else if(type=="Movie")
		{
			ConnectionType = EConnectionType.Movie;
			MediaPath=r.GetAttribute("Path");
		}

		type = r.GetAttribute ("StreamCodec");
		StreamCodec = EStreamCodec.Coded;
		if (type == "Raw")
			StreamCodec = EStreamCodec.Raw;
		else if (type == "Coded")
			StreamCodec = EStreamCodec.Coded;
		else if (type == "Ovrvision")
			StreamCodec = EStreamCodec.Ovrvision;

		IP=r.GetAttribute ("IP");
		URL=r.GetAttribute ("URL");
		Location=r.GetAttribute ("Location");
		string v=r.GetAttribute ("Longitude");
		if (v != null && v != "")
			lng = double.Parse (v);
		
		v=r.GetAttribute ("Latitude");
		if (v != null && v != "")
			lat = double.Parse (v);
	}

	public void Read(BinaryReader reader)
	{
		ID=reader.ReadInt32();
		ConnectionType=(EConnectionType)reader.ReadInt32();//Added 2015-12-24
		communicationPort = reader.ReadInt32 ();//Added 2015-12-24
		Name=reader.ReadStringNative();
		IP = reader.ReadStringNative();
		Location = reader.ReadStringNative();
		lng=reader.ReadSingle();
		lat=reader.ReadSingle();
		Connected=reader.ReadBoolean();
		Available=reader.ReadBoolean();
	}
}
