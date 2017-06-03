using UnityEngine;
using System.Collections;

public class ObjAngleX : Data.IDataSource {
	public Transform Object;
	public ObjAngleX(Transform g)
	{
		Object = g;
	}
	public string GetName ()
	{
		return Object.name+"RX";
	}
	public string GetValue()
	{
		float v = Object.rotation.eulerAngles.x;
		if (v > 180)
			v = -(360 - v);
		return v.ToString();
	}
}
public class ObjAngleY : Data.IDataSource {
	public Transform Object;
	public ObjAngleY(Transform g)
	{
		Object = g;
	}
	public string GetName ()
	{
		return Object.name+"RY";
	}
	public string GetValue()
	{
		float v = Object.rotation.eulerAngles.y;
		if (v > 180)
			v = -(360 - v);
		return v.ToString();
	}
}
public class ObjAngleZ : Data.IDataSource {
	public Transform Object;
	public ObjAngleZ(Transform g)
	{
		Object = g;
	}
	public string GetName ()
	{
		return Object.name+"RZ";
	}
	public string GetValue()
	{
		float v = Object.rotation.eulerAngles.z;
		if (v > 180)
			v = -(360 - v);
		return v.ToString();
	}
}
