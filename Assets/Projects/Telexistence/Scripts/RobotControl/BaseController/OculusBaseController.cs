using UnityEngine;
using UnityEngine.VR;
using System.Collections;

public class OculusBaseController:IRobotBaseControl {
	
	
	Vector3 m_headPosCalib=Vector3.zero;
	
	Vector2 m_currentSpeed=Vector2.zero;
	float m_currentRotation=0;

	public Vector2 GetSpeed()
	{
		if (!VRDevice.isPresent) {
			return Vector2.zero;
		}

		
		Vector3 p = InputTracking.GetLocalPosition(VRNode.Head);
		Vector3 diff = p - m_headPosCalib;
		
		float x = diff.z;
		float y = diff.x;
		
		float minOffset = 0.05f;
		float maxOffset = 0.15f;
		
		x = Mathf.Sign(x)*Mathf.Pow(Mathf.Min(1.0f, Mathf.Max(0, Mathf.Abs(x) - minOffset) / (maxOffset-minOffset)),2);
		y = Mathf.Sign(y)*Mathf.Pow(Mathf.Min(1.0f, Mathf.Max(0, Mathf.Abs(y) - minOffset) / (maxOffset - minOffset)),2);
		
		m_currentSpeed += new Vector2(x, -y)*Time.deltaTime * 2;
		m_currentSpeed -= m_currentSpeed*Time.deltaTime*1;
		m_currentSpeed.x = Mathf.Clamp(m_currentSpeed.x, -1, 1);
		m_currentSpeed.y = Mathf.Clamp(m_currentSpeed.y, -1, 1);

		Vector2 speed = m_currentSpeed;
		if (Mathf.Abs (speed.x) < 0.1)
			speed.x = 0;
		if (Mathf.Abs (speed.y) < 0.1)
			speed.y = 0;
		return m_currentSpeed;
	}
	public float GetRotation()
	{
		if (!VRDevice.isPresent) 
			return 0;

        float y = InputTracking.GetLocalRotation(VRNode.Head).eulerAngles.y;

		if(y>180)
			y = y-360;
		
		float minAngle = 20;
		float maxAngle = 60;
		
		y = Mathf.Sign(y)*Mathf.Pow(Mathf.Min(1.0f, Mathf.Max(0, Mathf.Abs(y) - minAngle) / (maxAngle - minAngle)),2);
		//m_currentRotation = y;
		m_currentRotation += Mathf.Clamp(2 * y, -1, 1)*Time.deltaTime* 2;
		m_currentRotation -= m_currentRotation*Time.deltaTime*1.5f;
		m_currentRotation = Mathf.Clamp(m_currentRotation, -1, 1);

		return -m_currentRotation;
	}
	public void Recalibrate()
	{
        if (!VRDevice.isPresent)
        {
			m_headPosCalib = InputTracking.GetLocalPosition(VRNode.Head);
		}
		m_currentSpeed = Vector2.zero;
		m_currentRotation = 0;
	}
}
