using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UINEDOVehicleStatus : MonoBehaviour {
	
	public Text AngleLabel;
	public Image angleBackground;
	public Image TorsoStatus;
	public Image XyrisStatus;
	public Image YBMStatus;
	
	public Sprite InfoBG;
	public Sprite WarningBG;
	public Sprite ErrorBG;


	enum EWarningLevel
	{
		Normal,
		Warning,
		High
	}
	EWarningLevel _warningLevel=EWarningLevel.Normal;

	float _angle;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetTorsoStatus(int s,bool overCurrent)
	{
		if (overCurrent)
			TorsoStatus.sprite = ErrorBG;
		else {
			if (s == 1)//TORSO: Parked
				TorsoStatus.sprite = WarningBG;
			else if (s == 3)//TORSO: Operation
				TorsoStatus.sprite = InfoBG;
		}
	}
	public void SetAngle(float angle)
	{
		_angle = angle;

		AngleLabel.text = angle.ToString ("0.0") + "°";

		if (_angle >= 41) {
			_warningLevel = EWarningLevel.High;

			angleBackground.sprite=ErrorBG;
		} else if (_angle >= 36) {
			_warningLevel = EWarningLevel.Warning;
			angleBackground.sprite=WarningBG;
		} else if (_angle >= -36) {
			_warningLevel = EWarningLevel.Normal;
			angleBackground.sprite=InfoBG;
		} else if (_angle >= -42) {
			_warningLevel = EWarningLevel.Warning;
			angleBackground.sprite=WarningBG;
		} else {
			_warningLevel = EWarningLevel.High;
			angleBackground.sprite=ErrorBG;
		}
	}
}
