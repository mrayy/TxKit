using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class ColorCorrectionImageProcessor : IImageProcessor {
	
	[Serializable]
	class ColorParameters
	{
		public float Contrast=1;
		public float Exposure=1;
		public float Saturation=1;
		public float[] Balance = new float[3];
	}

	OffscreenProcessor _processor;

	public float Contrast=1;
	public float Exposure=1;
	public float Saturation=1;
	public Color Balance=new Color(1,1,1,0);

	bool _ShowGUI=false;

	// Use this for initialization
	void Start () {
		_processor = new OffscreenProcessor ();
		_processor.ShaderName = "Image/ColorCorrection";

		_Load ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.F10)) {
			_ShowGUI = !_ShowGUI;
		}
	}

	void OnDestroy()
	{
		_Save ();
	}

	public override Texture ProcessTexture (Texture InputTexture, ref RenderTexture ResultTexture, int downSample = 0)
	{
		_processor.ProcessingMaterial.SetFloat ("_Contrast", Contrast);
		_processor.ProcessingMaterial.SetFloat ("_Exposure", Exposure);
		_processor.ProcessingMaterial.SetFloat ("_Saturation", Saturation);
		_processor.ProcessingMaterial.SetColor("_Balance", Balance);

		return _processor.ProcessTexture (InputTexture,ref ResultTexture, 0, downSample);
	}

	public void OnGUI()
	{
		if (!_ShowGUI)
			return;
		GUIStyle txtStyle = new GUIStyle();
		txtStyle.fontSize = 10;
		txtStyle.alignment = TextAnchor.UpperLeft;
		txtStyle.normal.textColor = Color.white;
		float y = 50;
		float x = 100;
		float height = 20;
		GUI.Box(new Rect(x-10,y-10,240,90),"");
		GUI.Label (new Rect(x,y,40,height),"Exposure",txtStyle);
		Exposure=GUI.HorizontalSlider (new Rect (x+60, y, 50, height), Exposure, 0.5f, 2);
		GUI.Label (new Rect(x+120,y,40,height),Exposure.ToString("F1"),txtStyle);
		y += 20;
		GUI.Label (new Rect(x,y,40,height),"Contrast",txtStyle);
		Contrast=GUI.HorizontalSlider (new Rect (x+60, y, 50, height), Contrast, 1, 2);
		GUI.Label (new Rect(x+120,y,40,height),Contrast.ToString("F1"),txtStyle);
		y += 20;
		GUI.Label (new Rect(x,y,40,height),"Saturation",txtStyle);
		Saturation=GUI.HorizontalSlider (new Rect (x+60, y, 50, height), Saturation, 1, 2);
		GUI.Label (new Rect(x+120,y,40,height),Saturation.ToString("F1"),txtStyle);
		y += 20;
		GUI.Label (new Rect(x,y,40,height),"Balance",txtStyle);
		Balance.r=GUI.HorizontalSlider (new Rect (x+40, y, 60, height), Balance.r, 0.8f, 1.2f);
		Balance.g=GUI.HorizontalSlider (new Rect (x+100, y, 60, height), Balance.g, 0.8f, 1.2f);
		Balance.b=GUI.HorizontalSlider (new Rect (x+160, y, 60, height), Balance.b, 0.8f, 1.2f);

	}
	void _Load()
	{
		if (File.Exists (Application.dataPath +  /*"/"+Application.productName+*/"/Settings/colorParams.dat")) {

			BinaryFormatter bf = new BinaryFormatter ();
			FileStream fs=File.Open(Application.dataPath+ /*"/"+Application.productName+*/"/Settings/colorParams.dat",FileMode.Open);

			ColorParameters Parameters=(ColorParameters) bf.Deserialize (fs);
			Contrast = Parameters.Contrast;
			Exposure = Parameters.Exposure;
			Saturation = Parameters.Saturation;
			Balance = new Color (Parameters.Balance [0], Parameters.Balance [1], Parameters.Balance [2], 0);
			fs.Close ();
		}
	}
	void _Save()
	{
		BinaryFormatter bf = new BinaryFormatter ();
		Directory.CreateDirectory (Application.dataPath +/*"/"+Application.productName+*/"/Settings/");
		FileStream fs=File.Open(Application.dataPath+/*"/"+Application.productName+*/"/Settings/colorParams.dat",FileMode.OpenOrCreate);

		ColorParameters Parameters = new ColorParameters ();
		Parameters.Contrast = Contrast;
		Parameters.Exposure = Exposure;
		Parameters.Saturation = Saturation;
		Parameters.Balance [0] = Balance.r;
		Parameters.Balance [1] = Balance.g;
		Parameters.Balance [2] = Balance.b;

		bf.Serialize (fs, Parameters);
		fs.Close ();
	}
}
