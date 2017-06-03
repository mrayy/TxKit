
//=============================================================================----
// Copyright © NaturalPoint, Inc. All Rights Reserved.
// 
// This software is provided by the copyright holders and contributors "as is" and
// any express or implied warranties, including, but not limited to, the implied
// warranties of merchantability and fitness for a particular purpose are disclaimed.
// In no event shall NaturalPoint, Inc. or contributors be liable for any direct,
// indirect, incidental, special, exemplary, or consequential damages
// (including, but not limited to, procurement of substitute goods or services;
// loss of use, data, or profits; or business interruption) however caused
// and on any theory of liability, whether in contract, strict liability,
// or tort (including negligence or otherwise) arising in any way out of
// the use of this software, even if advised of the possibility of such damage.
//=============================================================================----

// This script is intended to be attached to a Game Object.  It will receive
// XML data from the NatNet UnitySample application and notify any listening
// objects via the PacketNotification delegate.

// Attach Body.cs to an empty Game Object and it will parse and create visual
// game objects based on bone data.  Body.cs is meant to be a simple example 
// of how to parse and display skeletal data in Unity.

// Modified by MHD Yamen Saraiji

using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;



public delegate void PacketReceivedHandler(object sender, string PacketData);

public class SlipStream : MonoBehaviour
{
	public string IP = "127.0.0.1";
	public int Port  = 6000;
	
	public event PacketReceivedHandler PacketNotification;
	
	private IPEndPoint mRemoteIpEndPoint;
	private Socket     mListener;
	private byte[]     mReceiveBuffer;
	private string     mPacket;
	private int        mPreviousSubPacketIndex = 0;
	private const int  kMaxSubPacketSize       = 1400;


	
	void Start()
	{
		mReceiveBuffer = new byte[kMaxSubPacketSize];
		mPacket        = System.String.Empty;
		
		mRemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Port);
		mListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		mListener.Bind(mRemoteIpEndPoint);
		
		mListener.Blocking          = false;
		mListener.ReceiveBufferSize = 128*1024;
	}
	
	// packet received
	void OnPacketReceived(string Packet)
	{
		XmlDocument xmlDoc= new XmlDocument();
		xmlDoc.LoadXml(Packet);
		
		//== skeletons ==--
		
		XmlNodeList boneList = xmlDoc.GetElementsByTagName("Bone");
		
		for(int index=0; index<boneList.Count; index++)
		{
			
			int id = System.Convert.ToInt32(boneList[index].Attributes["ID"].InnerText);

			string name=boneList[index].Attributes["Name"].InnerText;
			
			float x = (float) System.Convert.ToDouble(boneList[index].Attributes["x"].InnerText);
			float y = (float) System.Convert.ToDouble(boneList[index].Attributes["y"].InnerText);
			float z = (float) System.Convert.ToDouble(boneList[index].Attributes["z"].InnerText);
			
			float qx = (float) System.Convert.ToDouble(boneList[index].Attributes["qx"].InnerText);
			float qy = (float) System.Convert.ToDouble(boneList[index].Attributes["qy"].InnerText);
			float qz = (float) System.Convert.ToDouble(boneList[index].Attributes["qz"].InnerText);
			float qw = (float) System.Convert.ToDouble(boneList[index].Attributes["qw"].InnerText);
			
			//== coordinate system conversion (right to left handed) ==--
			
			z = -z;
			qz = -qz;
			qw = -qw;
			
			//== bone pose ==--
			
			Vector3    position    = new Vector3(x,y,z);
			Quaternion orientation = new Quaternion(qx,qy,qz,qw);
			
			//== locate or create bone object ==--
			
			string objectName = "Bone"+id.ToString();
			
			GameObject bone;
			
			bone = GameObject.Find(objectName);
			
			if(bone==null)
			{
				bone = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Vector3 scale = new Vector3(0.1f,0.1f,0.1f);
				bone.transform.localScale = scale;
				bone.name = objectName;
			}		
			
			//== set bone's pose ==--
			
			bone.transform.position = position;
			bone.transform.rotation = orientation;
		}
		
		//== rigid bodies ==--
		
		XmlNodeList rbList = xmlDoc.GetElementsByTagName("RigidBody");
		
		for (int index = 0; index < rbList.Count; index++)
		{
			
			int id = System.Convert.ToInt32(rbList[index].Attributes["ID"].InnerText);
			string name=rbList[index].Attributes["Name"].InnerText;

			float x = (float)System.Convert.ToDouble(rbList[index].Attributes["x"].InnerText);
			float y = (float)System.Convert.ToDouble(rbList[index].Attributes["y"].InnerText);
			float z = (float)System.Convert.ToDouble(rbList[index].Attributes["z"].InnerText);
			
			float qx = (float)System.Convert.ToDouble(rbList[index].Attributes["qx"].InnerText);
			float qy = (float)System.Convert.ToDouble(rbList[index].Attributes["qy"].InnerText);
			float qz = (float)System.Convert.ToDouble(rbList[index].Attributes["qz"].InnerText);
			float qw = (float)System.Convert.ToDouble(rbList[index].Attributes["qw"].InnerText);
			
			//== coordinate system conversion (right to left handed) ==--
			
			z = -z;
			qz = -qz;
			qw = -qw;
			
			//== bone pose ==--
			
			Vector3 position = new Vector3(x, y, z);
			Quaternion orientation = new Quaternion(qx, qy, qz, qw);
			
			//== locate or create bone object ==--
			
			string objectName =name;// "RigidBody" + id.ToString();
			
			GameObject bone;
			
			bone = GameObject.Find(objectName);
			
			if (bone == null)
			{
				continue;
				/*
				bone = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Vector3 scale = new Vector3(0.1f, 0.1f, 0.1f);
				bone.transform.localScale = scale;
				bone.name = objectName;*/
			}
			
			//== set bone's pose ==--
			
			bone.transform.position = position;
			bone.transform.rotation = orientation;
		}
	}

	public void UDPRead()
	{
		try
		{
			int bytesReceived = mListener.Receive(mReceiveBuffer);
			
			int maxSubPacketProcess = 200;
			
			while(bytesReceived>0 && maxSubPacketProcess>0)
			{
				//== ensure header is present ==--
				if(bytesReceived>=2)
				{
					int  subPacketIndex = mReceiveBuffer[0];
					bool lastPacket     = mReceiveBuffer[1]==1;
					
					if(subPacketIndex==0)
					{
						mPacket = System.String.Empty;
					}
					
					if(subPacketIndex==0 || subPacketIndex==mPreviousSubPacketIndex+1)
					{
						mPacket += Encoding.ASCII.GetString(mReceiveBuffer, 2, bytesReceived-2);
						
						mPreviousSubPacketIndex = subPacketIndex;
						
						if(lastPacket)
						{
							//== ok packet has been created from sub packets and is complete ==--
							
							//== notify listeners ==--
							OnPacketReceived(mPacket);
							if(PacketNotification!=null)
								PacketNotification(this, mPacket);
						}
					}			
				}
								
				bytesReceived = mListener.Receive(mReceiveBuffer);

				//== time this out of packets are coming in faster than we can process ==--
				maxSubPacketProcess--;
			}
		}
		catch(System.Exception ex)
		{}
	}
 
	void Update()
	{
		UDPRead();
	
	}
}
