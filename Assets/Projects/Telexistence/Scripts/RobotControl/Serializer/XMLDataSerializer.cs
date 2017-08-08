using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class XMLDataSerializer:IDataSerializer  {

	class DataInfo
	{
		public string value;
		public bool statusData;
	};
	 
	Dictionary<string,Dictionary<string,DataInfo>> _values = new Dictionary<string, Dictionary<string,DataInfo>>();

	string _outputValues="";
	public string SerializeData()
	{

		lock(_values)
		{
			using (var sw = new StringWriter()) {
				using (var xw = XmlWriter.Create(sw)) {
					// Build Xml with xw.
					xw.WriteStartElement("RobotData");
					//xw.WriteAttributeString("Connected",_userInfo.RobotConnected.ToString());

					foreach(var v in _values)
					{
						foreach (var k in v.Value) {
							xw.WriteStartElement ("Data");
							xw.WriteAttributeString ("T", v.Key);
							xw.WriteAttributeString ("N", k.Key);
							xw.WriteAttributeString ("V", k.Value.value);
							xw.WriteEndElement ();
						}
					}

					xw.WriteEndElement();
				}
				_outputValues= sw.ToString();
			}
		}
		return _outputValues;
	}

	public void CleanData(bool statusValues)
	{
		lock (_values) {
			if (statusValues)
				_values.Clear ();
			else {
				foreach (var t in _values) {
					List<string> keys=new List<string>();
					foreach (var v in t.Value) {
						if(!v.Value.statusData)
							keys.Add (v.Key);
					}
					foreach (var k in keys)
						t.Value.Remove (k);
				}
			}
		}
	}

	public  string GetData(string target, string key)
	{
		string v="";
		lock(_values)
		{
			if (_values.ContainsKey (target)) {
				if(_values [target].ContainsKey(key))
				{
					var r = _values [target] [key];
					if (r != null)
						v = r.value;
				}
			}
		}
		return v;
	}

	public  void SetData(string target, string key, string value, bool statusData) 
	{
		DataInfo di = new DataInfo ();
		di.statusData = statusData;
		di.value = value;
		lock(_values)
		{
			if(!_values.ContainsKey(target))
				_values.Add(target,new Dictionary<string,DataInfo>());
			if(!_values[target].ContainsKey(key))
				_values[target].Add(key,di);
			else
				_values[target][key]=di;
		}
	}
	public  void RemoveData(string target, string key) 
	{
		lock(_values)
		{
			if (_values.ContainsKey (target)) {
				_values [target].Remove(key);
			}
		}
	}

}
