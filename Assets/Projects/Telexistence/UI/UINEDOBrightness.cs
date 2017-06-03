using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UINEDOBrightness : CameraBrightnessController {

	public Slider BrightnessSlider;
	public Text  BrigntnessLabel;
	public Image BrigntnessIcon;
	public CanvasGroup Holder;



	float _alpha;
	float _timeout=0;

	// Use this for initialization
	protected override void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update ();
		if (_timeout > 0) {
			_timeout -= Time.deltaTime;
		} else {
			if(_alpha>0)
			{
				_alpha-=Time.deltaTime;
				if(_alpha<=0)
					_alpha=0;
			}
		}

		if(Holder!=null)
			Holder.alpha = _alpha;
	}

	public override void SetBrightness(float v)
	{
		base.SetBrightness (v);
		_alpha = 1;

		_timeout = 5;
		/*
		BrigntnessIcon.alpha = _value * 0.8f + 0.2f;
		if (BrigntnessIcon.alpha < 0)
			BrigntnessIcon.alpha = 1;*/
		if(BrigntnessLabel!=null)
			BrigntnessLabel.text = ((int)(_value * 100)).ToString () + "%";
		if(BrightnessSlider!=null)
			BrightnessSlider.value = _value;
	}
}
