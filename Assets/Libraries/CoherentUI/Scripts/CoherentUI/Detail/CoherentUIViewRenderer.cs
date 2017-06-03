using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class CoherentUIViewRenderer : MonoBehaviour
{
	CoherentUIViewRenderer()
	{
		ViewId = 0;
		DrawAfterPostEffects = false;
		IsActive = true;
	}

	[DllImport("CoherentUI_Native")]
	private static extern void DummyUnityCall();

	void Start()
	{
		DummyUnityCall();

		if (DrawAfterPostEffects && m_DrawAtEndOfFrame == null)
		{
			m_DrawAtEndOfFrame = StartCoroutine("DrawAtEndOfFrame");
		}
	}

	public static void WakeRenderer(byte contextId)
	{
		DummyUnityCall();

		GL.IssuePluginEvent(System.IntPtr.Zero,MakeEvent(
			CoherentUISystem.CoherentRenderingEventType.WakeRenderer,
			CoherentUISystem.CoherentRenderingFlags.None, 0, 0, contextId));
	}

	private static int MakeEvent(
			CoherentUISystem.CoherentRenderingEventType evType,
			CoherentUISystem.CoherentRenderingFlags renderingFlags,
			CoherentUISystem.CoherentFilteringModes filteringMode,
			byte viewId,
			byte contextId)
	{
		int eventId = CoherentUISystem.COHERENT_PREFIX << 24;

		eventId |= ((int)renderingFlags) << 20;
		eventId |= ((int)filteringMode) << 18;
		eventId |= ((int)evType) << 16;
		eventId |= (contextId << 8);
		eventId |= viewId;
		
		return eventId;
	}

	void OnPostRender()
	{
		if (!DrawAfterPostEffects)
		{
			Draw();
		}
	}
	private IEnumerator DrawAtEndOfFrame()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			Draw();
		}
	}

	private Coroutine m_DrawAtEndOfFrame;

	public void OnEnable()
	{
		if (DrawAfterPostEffects)
		{
			m_DrawAtEndOfFrame = StartCoroutine("DrawAtEndOfFrame");
		}
	}

	public void OnDisable()
	{
		if (DrawAfterPostEffects)
		{
			StopCoroutine("DrawAtEndOfFrame");
			m_DrawAtEndOfFrame = null;
		}
	}


	private void Draw()
	{
		if(!IsActive) return;

		CoherentUISystem.CoherentRenderingFlags renderingFlags = CoherentUISystem.CoherentRenderingFlags.None;
		
		if (FlipY)
		{
			renderingFlags |= CoherentUISystem.CoherentRenderingFlags.FlipY;
		}
		
		if(ShouldCorrectGamma)
		{
			renderingFlags |= CoherentUISystem.CoherentRenderingFlags.CorrectGamma;
		}

		GL.IssuePluginEvent(System.IntPtr.Zero,MakeEvent(
					CoherentUISystem.CoherentRenderingEventType.DrawView,
					renderingFlags, FilteringMode, ViewId, ContextId));
	}

	internal byte ViewId
	{
		get;
		set;
	}

	internal byte ContextId
	{
		get;
		set;
	}

	internal bool DrawAfterPostEffects
	{
		get;
		set;
	}

	internal bool IsActive
	{
		get;
		set;
	}

	internal bool FlipY;
	
	internal CoherentUISystem.CoherentFilteringModes FilteringMode
	{
		get;
		set;
	}
	internal bool ShouldCorrectGamma
	{
		get;
		set;
	}
}
