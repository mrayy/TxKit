using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UINEDOBattery : MonoBehaviour {

	public Slider slider;
	public Text label;

	public float MaxCapacity=24;
	public float MinCapacity=20;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetLevel(float level)
	{
		slider.value = Mathf.Clamp01((level-MinCapacity) / (MaxCapacity-MinCapacity));


		label.text = level.ToString ("0.0") + "V";
	}
}
