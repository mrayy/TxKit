using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class UINEDOMessage : MonoBehaviour {

	struct ErrorCodes
	{
		public ErrorCodes(ushort c,string e,string j,MessageLevel l)
		{
			code=c;
			enString=e;
			jpString=j;
			level=l;
		}
		public ushort code;
		public string enString;
		public string jpString;
		public MessageLevel level;
	};

	List<ErrorCodes> _errCodes=new List<ErrorCodes>();

	public enum MessageLevel
	{
		Info,
		Warning,
		Error
	}

	public CanvasGroup messageHandler;
	public Text messageLabel;
	public Image msgBackground;

	public Sprite InfoBG;
	public Sprite WarningBG;
	public Sprite ErrorBG;

	public float TimeOut=4;
	float _timeOut;
	float _blinkSpeed;

	bool _isDismissed=false;


	// Use this for initialization
	void Start () {
		_errCodes.Add(new ErrorCodes(0x2000,"Forward (45°) ≦ Xyris","前方45°≦機体角度",MessageLevel.Error));
		_errCodes.Add(new ErrorCodes(0x2001,"Backward 40°≦ Xyris","後方40°≦機体角度",MessageLevel.Error));
		_errCodes.Add(new ErrorCodes(0x2002,"Forward (41°) ≦ Xyris","前方41°≦機体角度",MessageLevel.Error));
		_errCodes.Add(new ErrorCodes(0x2003,"Left (42°) ≦ Xyris","左に42°≦機体角度",MessageLevel.Error));

		_errCodes.Add(new ErrorCodes(0x2100,"(Forward) 40° ≦ Xyris<45°","前方40°≦機体角度<前方45°",MessageLevel.Warning));
		_errCodes.Add(new ErrorCodes(0x2101,"(Forward) 36° ≦ Xyris<40°","後方36°≦機体角度<後方40°",MessageLevel.Warning));
		_errCodes.Add(new ErrorCodes(0x2102,"(Right) 36°≦ Xyris <41°","右に36°≦機体角度<右に41°",MessageLevel.Warning));
		_errCodes.Add(new ErrorCodes(0x2103,"(Left) 37°≦ Xyris <42°","左に37°≦機体角度<左に42°",MessageLevel.Warning));

		_errCodes.Add(new ErrorCodes(0x2200,"Backwards (36°)>Xyris <Front(40°)","後方36°>機体角度<前方40°",MessageLevel.Info));
		_errCodes.Add(new ErrorCodes(0x2201,"Left (37°)> Xyris < Right (36°)","左に37°>機体角度<右に36°",MessageLevel.Info));

		messageHandler.gameObject.SetActive (false);
		
	}
	
	// Update is called once per frame
	void Update () {
		if (_timeOut >= 0 || !_isDismissed) {
			_timeOut-=Time.deltaTime;
			messageHandler.alpha=0.5f+0.5f*Mathf.Abs(Mathf.Sin(_timeOut*_blinkSpeed));

			if(Input.GetButtonDown("MsgDismiss"))
				_isDismissed=true;
			if(_timeOut<=0 && _isDismissed)
			{
				messageHandler.gameObject.SetActive (false);
			}
		}

	/*	foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown(kcode))
				Debug.Log("KeyCode down: " + kcode);
		}*/

		if (Input.GetKeyDown (KeyCode.M)) {
			int i=UnityEngine.Random.Range(0,_errCodes.Count-1);
			SetMessage(_errCodes[i].jpString,_errCodes[i].level);
		}
	}

	public void SetMessage(ushort code)
	{
		for (int i=0; i<_errCodes.Count; ++i) {
			if(_errCodes[i].code==code)
			{
				SetMessage(_errCodes[i].enString,_errCodes[i].level);
				break;
			}
		}
	}
	public void SetMessage(string msg,MessageLevel level)
	{
		messageLabel.text = msg;
		_timeOut = TimeOut;
		messageHandler.gameObject.SetActive (true);
		_isDismissed = false;
		switch (level) {
		case MessageLevel.Info:
			msgBackground.sprite=InfoBG;
			_blinkSpeed=1;
			break;
		case MessageLevel.Warning:
			msgBackground.sprite=WarningBG;
			_blinkSpeed=3;
			break;
		case MessageLevel.Error:
			msgBackground.sprite=ErrorBG;
			_blinkSpeed=5;
			break;
		}
	}
}
