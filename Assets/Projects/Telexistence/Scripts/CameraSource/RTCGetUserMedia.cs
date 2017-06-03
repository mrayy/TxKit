using UnityEngine;
using System.Collections;

public class RTCGetUserMedia : MonoBehaviour {

	public void Init(CoherentUIView view)
	{
	//	var view = GetComponent<CoherentUIView>();
		view.Listener.RequestMediaStream += (request) => {
			var devices = request.Devices;
			Debug.Log(devices.Length);
			for (var i = 0; i != devices.Length; ++i)
			{
				if (devices[i].Type == Coherent.UI.MediaStreamType.MST_DEVICE_AUDIO_CAPTURE)
				{
					Debug.Log(string.Format("Using audio device {0} {1}", devices[i].DeviceId, devices[i].Name));
					request.Respond(new uint[] { (uint)i });
					return;
				}
			}
			Debug.LogError("No audio or video devices detected?");
		};
		/*
        view.Listener.RequestMediaStream += (request) => {
            var devices = request.Devices;
            for (var i = 0; i != devices.Length; ++i)
            {
                if (devices[i].Type == Coherent.UI.MediaStreamType.MST_DEVICE_VIDEO_CAPTURE)
                {
                    if (i > 0)
                    {
                        // respond with first video and last audio device
                        Debug.Log(string.Format("Using audio device {0} {1}", devices[i - 1].DeviceId, devices[i - 1].Name));
                        Debug.Log(string.Format("Using video device {0} {1}", devices[i].DeviceId, devices[i].Name));
						request.Respond(new uint[] { (uint)i - 1,(uint)i});
                        return;
                    }
                    else
                    {
                        Debug.LogError("No audio devices detected?");
                    }
                }
            }
            Debug.LogError("No audio or video devices detected?");
        };*/
    }
}
