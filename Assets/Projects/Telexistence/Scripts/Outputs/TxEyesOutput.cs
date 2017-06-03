using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TxEyesOutput {

	static Texture2D _nullTexture;

	static Texture2D NullTexture
	{
		get{
			if (_nullTexture == null) {
				_nullTexture = new Texture2D (1, 1, TextureFormat.RGB24,false);
				_nullTexture.SetPixel (0, 0, Color.blue);
				_nullTexture.Apply ();
			}
			return _nullTexture;
		}
	}

	public enum OutputType
	{
		Mono,
		Stereo,
		Omni
	}

	Texture[] _eyes;

	OutputType _type;

	public Texture GetTexture(EyeName e)
	{

		if (_eyes.Length == 0)
			return NullTexture;
		if (_type == OutputType.Mono || _type == OutputType.Omni || _eyes.Length == 1)
			return _eyes [0];

		return _eyes [(int)e];
	}

	public Texture LeftEye
	{
		get{
			return GetTexture(EyeName.LeftEye);
		}
	}


	public Texture RightEye
	{
		get{
			return GetTexture(EyeName.RightEye);
		}
	}


	public OutputType ResultType
	{
		get{
			return _type;
		}
	}

	public void SetEyesTextures(Texture[] eyes, OutputType type)
	{
		_type = type;
		_eyes = eyes;
	}


}
