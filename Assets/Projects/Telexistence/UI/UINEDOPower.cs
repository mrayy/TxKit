using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class UINEDOPower : MonoBehaviour {

	public Image image;

	public Sprite PowerOn;
	public Sprite PowerOff;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SetPowered(bool Powered)
	{
		image.sprite = Powered? PowerOn:PowerOff;
	}
}
