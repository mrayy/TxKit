using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JSONDataSerializer : IDataSerializer {

	class DataInfo
	{
		public string key;
		public string value;
		public bool statusData;
	};
	Dictionary<string,List<DataInfo>> _values = new Dictionary<string, List<DataInfo>>();

	string _outputValues="";
	public string SerializeData()
	{

		lock(_values)
		{
			JSONObject jobj = new JSONObject ();

			foreach(var v in _values)
			{
				foreach (var k in v.Value) {
					jobj.AddField (v.Key + ":"+k.key, k.value);
				}
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
				foreach (var t in _values) {
					t.Value.RemoveAll(item => item.statusData==false);
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
				var r=_values [target].Find (item => item.key == key);
				if (r != null)
					v = r.value;
			}
		}
		return v;
	}

	public  void SetData(string target, string key, string value, bool statusData) 
	{
		DataInfo di = new DataInfo ();
		di.statusData = statusData;
		di.key = key;
		di.value = value;
		lock(_values)
		{
			if(!_values.ContainsKey(target))
				_values.Add(target,new List<DataInfo>());
			_values[target].Add(di);
		}
	}
	public  void RemoveData(string target, string key) 
	{
		lock(_values)
		{
			if (_values.ContainsKey (target)) {
				_values[target].RemoveAll (item => item.key==key);
			}
		}
	}


}
