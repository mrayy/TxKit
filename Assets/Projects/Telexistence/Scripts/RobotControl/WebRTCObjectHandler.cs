using UnityEngine;
using System.Collections;

public class WebRTCObjectHandler : MonoBehaviour {

	CoherentUIView _view;
	CoherentUITextureListener _viewListener;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Close()
	{
		if (_view) {
			Destroy (_view);
			_view = null;
			_viewListener = null;
		}
	}
	public bool IsCreated()
	{
		return _view != null;
	}
	public CoherentUIView GetOrCreateView()
	{
		if (_view != null)
			return _view;
		gameObject.SetActive (false);
		_view= gameObject.AddComponent<CoherentUIView> ();
		_view.UseCameraDimensions = false;

		_view.OnViewCreatedEvent += _OnViewCreated;
		_view.Reload (true);

		gameObject.SetActive (true);
		return _view;
	}
	void _OnViewCreated(CoherentUIView view)
	{
		Debug.Log ("_OnViewCreated");
		_viewListener = new CoherentUITextureListener (_view,_view.Width,_view.Height);
		_view.SetListener (_viewListener);
	}


	public Texture GetTexture()
	{
		return _viewListener.ViewTexture;
	}
}
