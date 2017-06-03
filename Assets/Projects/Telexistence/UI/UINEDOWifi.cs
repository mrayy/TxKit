using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class UINEDOWifi : MonoBehaviour {

	[Serializable]
	public class SignalLevel
	{
		public Sprite img;
		public int level;
	}
	public SignalLevel[] levels;

	public Image image;

	public Text label;

	public int level;

	int m_level=-1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		SetLevel (level);
	}

	public void SetLevel(int level)
	{
		if (m_level == level)
			return;
		label.text = level.ToString () + "db";
		bool found = false;
		for(int i=0;i<levels.Length;++i)
		{
			if(level<levels[i].level)
			{
				image.sprite=levels[i].img;
				found=true;
				break;
			}
		}
		if (!found)
			image.sprite = levels [levels.Length - 1].img;
	}
}
