using UnityEngine;
using System.Collections;

public interface IDataSerializer  {

	string SerializeData ();
	void CleanData(bool statusValues);
	string GetData (string target, string key);
	void SetData (string target, string key, string value, bool statusData) ;
	void RemoveData (string target, string key) ;


}
