using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class XMLDataSerializer:IDataSerializer  {

	struct DataInfo
	{
		public string value;
		public bool statusData;
	};
	Dictionary<string,DataInfo> _values = new Dictionary<string, DataInfo> ();

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
						xw.WriteStartElement("Data");
						xw.WriteAttributeString("N",v.Key);
						xw.WriteAttributeString("V",v.Value.value);
						xw.WriteEndElement();
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
				List<string> keys=new List<string>();
				foreach (var v in _values) {
					if (!v.Value.statusData) {
						keys.Add(v.Key);
					}
				}
				foreach(var v in keys)
				{
					_values.Remove(v);
				}
			}
		}
	}

	public  string GetData(string key)
	{
		string v="";
		lock(_values)
		{
			if (_values.ContainsKey (key))
				v=_values [key].value;
		}
		return v;
	}

	public  void SetData(string key, string value, bool statusData) 
	{
		DataInfo di = new DataInfo ();
		di.statusData = statusData;
		di.value = value;
		lock(_values)
		{
			if(!_values.ContainsKey(key))
				_values.Add(key,di);
			else _values[key]=di;
		}
	}
	public  void RemoveData(string key) 
	{
		lock(_values)
		{
			_values.Remove (key);
		}
	}

}
