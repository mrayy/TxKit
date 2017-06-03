using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MovingAverage2F
{
	List<Vector2> _data = new List<Vector2> ();
	int _count=10;
	Vector2 _v;
	public MovingAverage2F(int count)
	{
		_count=count;
	}
	public Vector2 Add(Vector2 data,float strength)
	{
		_data.Add (data);
		if (_data.Count > _count)
			_data.RemoveAt (0);
		Vector2 res = data*strength ;
		if (strength > 1)
			strength = 1;
		float invStr = 1 - strength;
		invStr /= (float)_data.Count;
		for (int i = 0; i < _data.Count-1; ++i) {
			res =res+ _data [i]*invStr;
		}

		//res = res / (float)_data.Count;

		//res = res / (float)_data.Count;
		_v=res;
		return res;
	}
	public Vector2 Value()
	{
		return _v;
	}
}

public class MovingAverageF
{
	List<float> _data = new List<float> ();
	int _count=10;
	float _v;
	public MovingAverageF(int count)
	{
		_count=count;
	}
	public float Add(float data,float strength)
	{
		_data.Add (data);
		if (_data.Count > _count)
			_data.RemoveAt (0);
		float res = 0;
		if (strength > 1)
			strength = 1;
		float invStr = 1 - strength;
		invStr /= (float)_data.Count;
		for (int i = 0; i < _data.Count-1; ++i) {
			res =res+ _data [i];
		}

		//res = res * invStr;
		res += data;// * strength;
		res = res / (float)_data.Count;
		_v=res;
		return res;
	}
	public float Value()
	{
		return _v;
	}
}
