using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JSONDataSerializer : IDataSerializer {

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
			JSONObject jobj = new JSONObject ();

			foreach(var v in _values)
			{
				jobj.AddField (v.Key, v.Value.value);
			}

			_outputValues= jobj.Print();
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
