using System;
using UnityEngine;
using System.Collections;
using Coherent.UI.Binding;

public interface RTCCallbackObserver
{
    void RecvMessage(string message);
}

public class IRTCSocket
{
	protected CoherentUIView UIView;

	// Use this for initialization
	public virtual void Start (CoherentUIView view) {
		UIView = view;
	}
	public bool IsOpen()
	{
		return UIView != null;
	}

	public void ConnectTo(RobotInfo ifo)
	{
		if (UIView == null)
			return;
		
		UIView.Page = ifo.URL;// "https://robo.skyway.io/";//"?ID="+ifo.ID.ToString();
		//UIView.enabled = true;
		//UIView.Reload (true);
	}

	public void Close()
	{

		if (UIView == null)
			return;
		UIView.enabled = false;
	}
}

public class RTCSocketReceiver:IRTCSocket
{
	private RTCCallbackObserver _observer = null;

	// Use this for initialization
	public override void Start (CoherentUIView view) {
		base.Start (view);
		UIView.Listener.ReadyForBindings += HandleReadyForBindings;
	}

	void HandleReadyForBindings(int frameId, string path, bool isMainFrame)
	{
		UIView.View.BindCall("helloFromJS", new Action<string>((msg) =>{
			if (_observer != null) 
				_observer.RecvMessage(msg);
		}));
	}

	// Update is called once per frame
	public void SetObserver (RTCCallbackObserver observer) {
		_observer = observer;
	}

}

public class RTCSocketSender:IRTCSocket
{

	public virtual void Send(string message)
	{
		UIView.View.TriggerEvent("send", message);
	}

}
