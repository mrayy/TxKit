using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class KeysMap : MonoBehaviour {
	[Serializable]
	public struct KeyInfo
	{
		public string Name;
		public KeyCode Key;
	}
	public KeyInfo[] Keys;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
