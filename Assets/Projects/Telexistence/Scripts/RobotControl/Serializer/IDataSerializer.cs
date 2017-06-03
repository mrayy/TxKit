using UnityEngine;
using System.Collections;

public interface IDataSerializer  {

	string SerializeData ();
	void CleanData(bool statusValues);
	string GetData (string key);
	void SetData (string key, string value, bool statusData) ;
	void RemoveData (string key) ;


}
