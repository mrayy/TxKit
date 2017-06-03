using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class UINEDOConnectScreen : MonoBehaviour {

	public GameObject ScreenHandler;
	public Text Message;
	public Image Background;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SetMessage(string msg)
	{
		if (Message != null)
			Message.text = msg;
	}

	public void SetBackgroundAlpha(float alpha)
	{
		if (Background != null) {
			Background.color = new Color (Background.color.r, Background.color.g, Background.color.b, alpha);
		}
	}
	public void SetConnected(bool connected)
	{
		ScreenHandler.SetActive(!connected);

	}

}
