using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UINEDOSpeed : MonoBehaviour {

	float _minLimit=120;
	float _maxLimit=-120;

	public float Speed=10;
	public float MaxSpeed;

	public Image indicator;
	public Text text;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		SetSpeed (Speed);
	}

	public void SetSpeed(float speed)
	{
		text.text = speed.ToString ("0.0") + " km/h";
		this.Speed = speed;
		if(speed>MaxSpeed)
		{
			speed=MaxSpeed;
		}

		indicator.rectTransform.localRotation= Quaternion.Euler (0, 0, _minLimit + (_maxLimit - _minLimit) * speed / MaxSpeed);
	}
}
