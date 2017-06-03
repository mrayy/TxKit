using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotJointControllerTst : MonoBehaviour {
	public Transform X;
	public Transform Y;
	public Transform Z;

    
	public enum RotSeq{ zyx, zyz, zxy, zxz, yxz, yxy, yzx, yzy, xyz, xyx, xzy, xzx };

	public RotSeq rotSeq;

	// Use this for initialization
	void Start () {
		
	}
	
    void twoaxisrot(float r11, float r12, float r21, float r31, float r32, ref Vector3 res){
	    res.x = Mathf.Atan2(r11, r12);
		res.y = Mathf.Acos(r21);
		res.z = Mathf.Atan2(r31, r32);
    }

    void threeaxisrot(float r11, float r12, float r21, float r31, float r32, ref Vector3 res)
    {
		res.x = Mathf.Atan2(r31, r32);
		res.y = Mathf.Asin(r21);
		res.z = Mathf.Atan2(r11, r12);
    }
    
	void quaternion2Euler(Quaternion q, ref Vector3 res, RotSeq rotSeq)
	{
		switch (rotSeq){
		case RotSeq.zyx:
			threeaxisrot(2 * (q.x*q.y + q.w*q.z),
				q.w*q.w + q.x*q.x - q.y*q.y - q.z*q.z,
				-2 * (q.x*q.z - q.w*q.y),
				2 * (q.y*q.z + q.w*q.x),
				q.w*q.w - q.x*q.x - q.y*q.y + q.z*q.z,
				ref res);
			break;

		case RotSeq.zyz:
			twoaxisrot(2 * (q.y*q.z - q.w*q.x),
				2 * (q.x*q.z + q.w*q.y),
				q.w*q.w - q.x*q.x - q.y*q.y + q.z*q.z,
				2 * (q.y*q.z + q.w*q.x),
				-2 * (q.x*q.z - q.w*q.y),
				ref res);
			break;

		case RotSeq.zxy:
			threeaxisrot(-2 * (q.x*q.y - q.w*q.z),
				q.w*q.w - q.x*q.x + q.y*q.y - q.z*q.z,
				2 * (q.y*q.z + q.w*q.x),
				-2 * (q.x*q.z - q.w*q.y),
				q.w*q.w - q.x*q.x - q.y*q.y + q.z*q.z,
				ref res);
			break;

		case RotSeq.zxz:
			twoaxisrot(2 * (q.x*q.z + q.w*q.y),
				-2 * (q.y*q.z - q.w*q.x),
				q.w*q.w - q.x*q.x - q.y*q.y + q.z*q.z,
				2 * (q.x*q.z - q.w*q.y),
				2 * (q.y*q.z + q.w*q.x),
				ref res);
			break;

		case RotSeq.yxz:
			threeaxisrot(2 * (q.x*q.z + q.w*q.y),
				q.w*q.w - q.x*q.x - q.y*q.y + q.z*q.z,
				-2 * (q.y*q.z - q.w*q.x),
				2 * (q.x*q.y + q.w*q.z),
				q.w*q.w - q.x*q.x + q.y*q.y - q.z*q.z,
				ref res);
			break;

		case RotSeq.yxy:
			twoaxisrot(2 * (q.x*q.y - q.w*q.z),
				2 * (q.y*q.z + q.w*q.x),
				q.w*q.w - q.x*q.x + q.y*q.y - q.z*q.z,
				2 * (q.x*q.y + q.w*q.z),
				-2 * (q.y*q.z - q.w*q.x),
				ref res);
			break;

		case RotSeq.yzx:
			threeaxisrot(-2 * (q.x*q.z - q.w*q.y),
				q.w*q.w + q.x*q.x - q.y*q.y - q.z*q.z,
				2 * (q.x*q.y + q.w*q.z),
				-2 * (q.y*q.z - q.w*q.x),
				q.w*q.w - q.x*q.x + q.y*q.y - q.z*q.z,
				ref res);
			break;

		case RotSeq.yzy:
			twoaxisrot(2 * (q.y*q.z + q.w*q.x),
				-2 * (q.x*q.y - q.w*q.z),
				q.w*q.w - q.x*q.x + q.y*q.y - q.z*q.z,
				2 * (q.y*q.z - q.w*q.x),
				2 * (q.x*q.y + q.w*q.z),
				ref res);
			break;

		case RotSeq.xyz:
			threeaxisrot(-2 * (q.y*q.z - q.w*q.x),
				q.w*q.w - q.x*q.x - q.y*q.y + q.z*q.z,
				2 * (q.x*q.z + q.w*q.y),
				-2 * (q.x*q.y - q.w*q.z),
				q.w*q.w + q.x*q.x - q.y*q.y - q.z*q.z,
				ref res);
			break;

		case RotSeq.xyx:
			twoaxisrot(2 * (q.x*q.y + q.w*q.z),
				-2 * (q.x*q.z - q.w*q.y),
				q.w*q.w + q.x*q.x - q.y*q.y - q.z*q.z,
				2 * (q.x*q.y - q.w*q.z),
				2 * (q.x*q.z + q.w*q.y),
				ref res);
			break;

		case RotSeq.xzy:
			threeaxisrot(2 * (q.y*q.z + q.w*q.x),
				q.w*q.w - q.x*q.x + q.y*q.y - q.z*q.z,
				-2 * (q.x*q.y - q.w*q.z),
				2 * (q.x*q.z + q.w*q.y),
				q.w*q.w + q.x*q.x - q.y*q.y - q.z*q.z,
				ref res);
			break;

		case RotSeq.xzx:
			twoaxisrot(2 * (q.x*q.z - q.w*q.y),
				2 * (q.x*q.y + q.w*q.z),
				q.w*q.w + q.x*q.x - q.y*q.y - q.z*q.z,
				2 * (q.x*q.z + q.w*q.y),
				-2 * (q.x*q.y - q.w*q.z),
				ref res);
			break;
		default:
			break;
		}



		res.x = Mathf.Rad2Deg*(res.x);
		res.y = Mathf.Rad2Deg*(res.y);
		res.z = Mathf.Rad2Deg*(res.z);



	}
	public Vector3 rot = new Vector3 ();
	// Update is called once per frame
	void Update () {
		Vector3 r;
		quaternion2Euler (transform.localRotation, ref rot, rotSeq);

		r=X.localRotation.eulerAngles;
		r.x = -rot.z;
		X.localRotation = Quaternion.Euler(r);

		r=Y.localRotation.eulerAngles;
		r.y = rot.y;
		Y.localRotation = Quaternion.Euler(r);

		r=Z.localRotation.eulerAngles;
		r.x = rot.x;
		Z.localRotation = Quaternion.Euler(r);
	}
}
