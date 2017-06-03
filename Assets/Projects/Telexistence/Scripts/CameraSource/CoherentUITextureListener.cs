using UnityEngine;
using System.Collections;

public class CoherentUITextureListener : Coherent.UI.UnityViewListener {

	Texture _viewTexture;

	public Texture ViewTexture {
		get{ return _viewTexture; }
	}

	public CoherentUITextureListener(CoherentUIView component, int width, int height):
	base(component,width,height)
	{
		
	}

	protected override void OnSetMaterial(Material RTMaterial)
	{
		_viewTexture = RTMaterial.mainTexture;
	}
	protected override void OnSetTexture(Texture RTTexture)
	{
		_viewTexture = RTTexture;
	}
}
