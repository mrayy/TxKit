using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;


//http://stackoverflow.com/questions/19786668/c-sharp-udp-socket-client-and-server
public struct Received
{
    public IPEndPoint Sender;
    public byte[] Message;
}

public abstract class UdpBase
{
    protected UdpClient _Client;

    public UdpClient Client
    {
        get { return _Client; }
    }

    protected UdpBase()
    {
        _Client = new UdpClient();
    }
	
	public Received Receive()
	{
		Received ret = new Received ();
		ret.Message =  _Client.Receive(ref ret.Sender);
		return ret;
		/*
		return new Received()
		{
			Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
			Sender = result.RemoteEndPoint
		};*/
	}
}

//Server
public class UdpListener : UdpBase
{
    private IPEndPoint _listenOn;

    public UdpListener()
        : this(new IPEndPoint(IPAddress.Any, 32123))
    {
    }

    public UdpListener(IPEndPoint endpoint)
    {
        _listenOn = endpoint;
        _Client = new UdpClient(_listenOn);
    }


    public void Reply(string message, IPEndPoint endpoint)
    {
        var datagram = Encoding.ASCII.GetBytes(message);
        _Client.Send(datagram, datagram.Length, endpoint);
    }

}

//Client
public class UdpUser : UdpBase
{
	public  UdpUser() { }

    public static UdpUser ConnectTo(string hostname, int port)
    {
        var connection = new UdpUser();
        connection._Client.Connect(hostname, port);
        return connection;
    }
	
	public void Disconnect()
	{
		_Client.Close ();
	}
    public void Send(string message)
    {
        var datagram = Encoding.ASCII.GetBytes(message);
        _Client.Send(datagram, datagram.Length);
    }

}