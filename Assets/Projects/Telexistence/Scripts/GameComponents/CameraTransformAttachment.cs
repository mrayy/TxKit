using UnityEngine;
using System.Collections;

public class CameraTransformAttachment : MonoBehaviour {

	public Transform attachedAnchor;
	public float speed=500;

	public TxKitBody body;
	public bool EnableHeadMatching=true;

	public Vector3 MinLimit= new Vector3 (0, 0, 0);
	public Vector3 MaxLimit = new Vector3 (0, 0, 0);

	class Average
	{
		float[] samples;
		int curr=0;
		float _val=0;

		public Average(int count)
		{
			samples=new float[count];
		}

		public float Sample(float v)
		{
			samples [curr] = v;
			curr = (curr + 1) % samples.Length;
			_val = 0;
			for (int i = 0; i < samples.Length; ++i)
				_val += samples [i];
			_val= _val / samples.Length;
			return _val;
		}

		public float Value(){return _val;}
	}

	Average[] _angleAverage=new Average[3]{new Average(5),new Average(5),new Average(5)};


	// Use this for initialization
	void Start () {
	
	}
	// Update is called once per frame
	void Update () {
		if (attachedAnchor == null)
			return;

		transform.position = attachedAnchor.position;
		if (body == null || body.RobotJointValues.Length<6 || !EnableHeadMatching || body.TrackingMode== TxKitBody.Mode.OculusSync) {
			
			//float t = (speed * Time.deltaTime);
			//Quaternion rot = Quaternion.Slerp (transform.localRotation, attachedAnchor.rotation, t);
			transform.localRotation = attachedAnchor.rotation;
			//Vector3 e = rot.eulerAngles;

			//e.x = Mathf.Clamp (e.x.NormalizeAngle(), MinLimit.x, MaxLimit.x);
			//e.y = Mathf.Clamp (e.y.NormalizeAngle(), MinLimit.y, MaxLimit.y);
			//e.z = Mathf.Clamp (e.z.NormalizeAngle(), MinLimit.z, MaxLimit.z);

			//transform.localRotation = Quaternion.Euler(e);
			//var diff=attachedAnchor.rotation* Quaternion.Inverse(transform.rotation);
			//transform.rotation = Quaternion.RotateTowards(transform.localRotation,attachedAnchor.rotation , t);
		} else {

			_angleAverage [0].Sample (body.RobotJointValues [1]);
			_angleAverage [1].Sample (body.RobotJointValues [3]);
			_angleAverage [2].Sample (body.RobotJointValues [5]);

			if (body.TrackingMode ==TxKitBody.Mode.RobotSync) {
				Quaternion rot = Quaternion.AngleAxis (_angleAverage [0].Value (), Vector3.left) *
				                 Quaternion.AngleAxis (_angleAverage [2].Value (), Vector3.forward) * Quaternion.AngleAxis (_angleAverage [1].Value (), Vector3.up);
				transform.localRotation = rot;
			} else {

				MinLimit.x = Mathf.Min (MinLimit.x, _angleAverage [0].Value ());
				MaxLimit.x = Mathf.Max (MaxLimit.x, _angleAverage [0].Value ());

				MinLimit.y = Mathf.Min (MinLimit.y, _angleAverage [1].Value ());
				MaxLimit.y = Mathf.Max (MaxLimit.y, _angleAverage [1].Value ());

				MinLimit.z = Mathf.Min (MinLimit.z, _angleAverage [2].Value ());
				MaxLimit.z = Mathf.Max (MaxLimit.z, _angleAverage [2].Value ());


				Quaternion rot = attachedAnchor.rotation;
				Vector3 e = rot.eulerAngles;


				e.x = Mathf.Clamp (e.x.NormalizeAngle (), MinLimit.x, MaxLimit.x);
				e.y = Mathf.Clamp (e.y.NormalizeAngle (), MinLimit.y, MaxLimit.y);
				e.z = Mathf.Clamp (e.z.NormalizeAngle (), MinLimit.z, MaxLimit.z);

				transform.localRotation = Quaternion.Euler (e);
			}

		}
	}
}
