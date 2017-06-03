using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListenerContainer<T>
{
	protected List<T> _listeners=new List<T>();
	public void AddListener(T l)
	{
		_listeners.Add (l);
	}

	public void RemoveListener(T l)
	{
		_listeners.Remove (l);

	}

}
