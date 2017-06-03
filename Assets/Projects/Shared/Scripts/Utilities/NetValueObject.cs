using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System;
using System.Text;

public class NetValueObject:IDisposable {
	
	public sealed class StringWriterWithEncoding : StringWriter
	{
		private readonly Encoding encoding;
		
		public StringWriterWithEncoding(Encoding encoding)
		{
			this.encoding = encoding;
		}
		
		public override Encoding Encoding
		{
			get { return encoding; }
		}
	}
	public class ValueControllerCtl
	{
		public string name;
		public string type;

		string _val;
		bool _changed=false;
		public string value
		{
			set{
				if (_val != value) {
					_changed = true;
					_val = value;
				}
			}
			get{
				return _val;
			}
		}
		public event EventHandler OnChanged;

		public bool Changed()
		{
			return _changed;
		}
		public void SetName(string name)
		{
			this.name = name;
		}
		public void SetType(string type,string value)
		{
			this.type=type;
			this.value = value;
			_changed = true;
		}
		
		public bool Export(XmlWriter xmlWriter)
		{
			_changed = false;
			xmlWriter.WriteStartElement("Value");
			xmlWriter.WriteAttributeString("Name", name);
			xmlWriter.WriteAttributeString("Value", value);
			xmlWriter.WriteEndElement();
			
			return true;
		}
	}
	public class ValueGroupCtl
	{
		List<ValueControllerCtl> values = new List<ValueControllerCtl>();
		List<ValueGroupCtl> groups = new List<ValueGroupCtl>();

		public string name;
		public event EventHandler OnChanged;
		
		public ValueControllerCtl CreateValue(string name, string type,string value)
		{
			Debug.Log ("Adding Value:" + name + "=" + value.ToString ());
			ValueControllerCtl c = new ValueControllerCtl();
			c.SetType(type, value);
			c.SetName(name);
			AddValue(c);
			return c;
		}

		public bool Changed()
		{
			bool c = false;
			foreach (var v in values) {
				c |= v.Changed ();
				if (c)
					return true;
			}
			foreach (var v in groups) {
				c |= v.Changed ();
				if (c)
					return true;
			}
			return false;
		}
		
		public void SetName(string name)
		{
			this.name = name;
		}
		public void AddValue(ValueControllerCtl v)
		{
			values.Add(v);
			v.OnChanged += c_OnChanged;
		}
		void c_OnChanged(object sender, EventArgs e)
		{
			if (OnChanged != null)
				OnChanged(sender, e);
		}
		public void AddGroup(ValueGroupCtl v)
		{
			groups.Add(v);
			v.OnChanged += c_OnChanged;
		}
		public ValueGroupCtl CreateValueGroup(string name)
		{
			Debug.Log ("Adding Group:" + name);
			ValueGroupCtl c = new ValueGroupCtl();
			c.SetName(name);
			
			AddGroup(c);
			return c;
		}

		public ValueGroupCtl GetGroup(string name)
		{
			foreach (var g in groups) {
				if(g.name==name)
					return g;
			}
			return null;
		}
		public ValueControllerCtl GetValue(string name)
		{
			foreach (var g in values) {
				if(g.name==name)
					return g;
			}
			return null;
		}
		
		public bool Export(XmlWriter xmlWriter)
		{
			
			xmlWriter.WriteStartElement("ValueGroup");
			xmlWriter.WriteAttributeString("Name", name);
			for (int i = 0; i < values.Count; ++i)
				values[i].Export(xmlWriter);
			for (int i = 0; i < groups.Count; ++i)
				groups[i].Export(xmlWriter);
			xmlWriter.WriteEndElement();
			
			return true;
		}
	}
	//
	
	class ReceiveThread:ThreadJob
	{
		NetValueObject _owner;
		public ReceiveThread(NetValueObject o)
		{
			_owner=o;
		}
		protected override void ThreadFunction() 
		{
			while (_owner.isConnected) {
				_owner._ReceiveThreadFunction ();
			}
		}
	}
	ValueGroupCtl values;
	UdpUser client;
	bool isConnected=false;

	ReceiveThread _thread;


	void _ReceiveThreadFunction()
	{
		try
		{
			var received = client.Receive();
			
			string Message = Encoding.ASCII.GetString(received.Message, 0, received.Message.Length);
			DataArrived(Message, received.Sender);
		}catch
		{
			
		}
	}

	public NetValueObject()
	{
		_thread = new ReceiveThread (this);
		values = new ValueGroupCtl ();
	}
	public void Dispose()
	{ 
		isConnected = false;
		if(client!=null)
			client.Disconnect ();
		_thread.Abort ();

		values = new ValueGroupCtl ();
	}

	public void Connect(string IP,int port)
	{
		
		if (isConnected)
		{
			isConnected = false;
			client.Disconnect();
		}
		client = UdpUser.ConnectTo(IP, port);
		isConnected = true;
		_thread.Start ();

		RequestScheme();
	}

	public ValueControllerCtl GetValue(string path)
	{
		if (values == null)
			return null;
		string[] objects=path.Split (".".ToCharArray());
		ValueGroupCtl g = values;
		for(int i=0;i<objects.Length-1;++i)
		{
			g=g.GetGroup(objects[i]);
			if(g==null)
				break;
		}
		if (g != null)
			return g.GetValue (objects[objects.Length - 1]);
		return null;
	}

	void RequestScheme()
	{
		string data = "RequestScheme#";
		client.Send(data); 
		
	}
	void DataArrived(string data,System.Net.IPEndPoint ep)
	{
		ParseScheme(data);
	}
	
	void ParseScheme(string scheme)
	{
		if(values!=null)
		{
			values = new ValueGroupCtl ();
		}
		using (var r = new StringReader(scheme))
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(r);
			XmlElement e = doc.DocumentElement;
			if (e!=null)
			{
				XmlLoadGroup(e,null);
			}
		}

	}
	void values_OnChanged(object sender, EventArgs e)
	{
		//SendData();
	}

	public void SendData()
	{
		if (values==null || !values.Changed ())
			return;
		string data;
		data = "Data#";
		StringWriterWithEncoding sw = new StringWriterWithEncoding(Encoding.UTF8);

		using (var xw = XmlWriter.Create(sw, new XmlWriterSettings() { Indent = true,CheckCharacters=true,OmitXmlDeclaration=true, Encoding = Encoding.UTF8 }))
		{
			// Build Xml with xw.
			
			values.Export(xw);
		}
		data+= sw.ToString();
		data += "\n";
		try
		{
			client.Send(data);
			
		}catch
		{
			
		}
		//textBox1.Text = data;
		//client.Send(data);
	}

	
	void XmlLoadValue(XmlElement reader,ValueGroupCtl parent)
	{
		
		parent.CreateValue(reader.GetAttribute("Name"), reader.GetAttribute("Type"), reader.GetAttribute("Value"));
	}
	void XmlLoadGroup(XmlElement reader, ValueGroupCtl parent)
	{
		ValueGroupCtl g=new ValueGroupCtl();
		if(parent==null)
		{
			values=g;
			values.SetName("Values");
			values.OnChanged += values_OnChanged;
		}
		else
		{
			parent.AddGroup(g);
		}
		//             while (reader.MoveToNextAttribute())
		//             {
		//             }
		g.SetName(reader.GetAttribute("Name"));
		XmlNode e;
		e = reader.FirstChild;
		while(e!=null)
		{
			if (e.NodeType == XmlNodeType.Element)
			{
				if (e.Name == "ValueGroup")
				{
					XmlLoadGroup(e as XmlElement, g);
				}
				else if (e.Name == "Value")
				{
					XmlLoadValue(e as XmlElement, g);
				}
			}
			e = e.NextSibling;
		}
		
	}

}
