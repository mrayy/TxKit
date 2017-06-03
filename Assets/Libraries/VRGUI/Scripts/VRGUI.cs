using UnityEngine;
using System.Collections;

public abstract class VRGUI : MonoBehaviour 
{
	public Vector3 guiPosition      = new Vector3(0f, 0f, 2f);
	public float   guiSize          = 2f;
	public bool    useCurvedSurface = false;
	public bool    acceptMouse      = true;
	public bool    acceptKeyboard   = true;
	public int     cursorSize       = 32;
	public Texture customCursor     = null;
	
	private GameObject    guiRenderPlane    = null;
	public RenderTexture guiRenderTexture  = null;
	private Vector2       cursorPosition    = Vector2.zero;
	private Texture       cursor            = null;
	public Material RenderMat;
	public Camera TargetCamera;

	private bool isInitialized = false;
	
	private void Initialize () 
	{

		// create the render plane
		/*if (useCurvedSurface)
		{
			guiRenderPlane = Instantiate(Resources.Load("VRGUICurvedSurface")) as GameObject;
		}
		else
		{
			guiRenderPlane = Instantiate(Resources.Load("VRGUIFlatSurface")) as GameObject;
		}*/
		/*
		guiRenderPlane = new GameObject ("VRGUI_Plane");

		guiRenderPlane.AddComponent<MeshRenderer> ();
		MeshFilter f = guiRenderPlane.AddComponent<MeshFilter> ();

		{
			f.mesh.vertices = new Vector3[]{
				new Vector3 (1, -1, 0),
				new Vector3 (-1, -1, 0),
				new Vector3 (-1, 1, 0),
				new Vector3 (1, 1, 0)
			};
			
			Rect r = new Rect (-1, -1, 1, 1);
			Vector2[] uv = new Vector2[]{
				new Vector2 (r.x, r.y),
				new Vector2 (r.x + r.width, r.y),
				new Vector2 (r.x + r.width, r.y + r.height),
				new Vector2 (r.x, r.y + r.height),
			};
			f.mesh.uv = uv;
			f.mesh.triangles = new int[]
			{
				0,2,1,0,3,2
			};
		}

		// position the render plane
		guiRenderPlane.transform.parent = this.transform;
		guiRenderPlane.transform.localPosition = guiPosition;
		guiRenderPlane.transform.localRotation = Quaternion.Euler (0f, 180f, 0f);
		guiRenderPlane.transform.localScale = new Vector3 (guiSize, guiSize, guiSize);
*/
		// create the render texture
		guiRenderTexture = new RenderTexture (Screen.width, Screen.height, 24);
		
		RenderMat = new Material ("");
		RenderMat.shader=Shader.Find ("VRGUITransparentOverlayShader");

		// assign the render texture to the render plane
		//guiRenderPlane.GetComponent<MeshRenderer> ().material = mat;
		//guiRenderPlane.GetComponent<MeshRenderer>().material.mainTexture = guiRenderTexture;
		
		if (acceptMouse)
		{
			// create the cursor
			if (customCursor != null)
			{
				cursor = customCursor;
			}
			else
			{
				cursor = Resources.Load("SimpleCursor") as Texture;
			}
		}
		
		isInitialized = true;
	}
	
	protected void OnEnable()
	{
		if (guiRenderPlane != null)
		{
			guiRenderPlane.SetActive(true);
		}
	}
	
	protected void OnDisable()
	{
		if (guiRenderPlane != null)
		{
			guiRenderPlane.SetActive(false);
		}
	}
	
	void _Resize(int w,int h)
	{
		if (guiRenderTexture == null)
			guiRenderTexture = new RenderTexture (w,h, 24,RenderTextureFormat.Default);
		
		if (guiRenderTexture.width != w || guiRenderTexture.height != h) {
			guiRenderTexture = new RenderTexture (w,h, 24,RenderTextureFormat.Default);
		}
		//guiRenderPlane.GetComponent<MeshRenderer>().material.mainTexture = guiRenderTexture;
	}
	public void OnGUI()
	{
		if (!isInitialized)
		{
			Initialize();
		}

		if(TargetCamera!=null)
			_Resize ((int)TargetCamera.pixelWidth,(int)TargetCamera.pixelHeight);
		// handle mouse events
		/*
		if (Event.current.isMouse)
		{
			// return if not accepting mouse events
			if (!acceptMouse)
			{
				return;
			}
		}
		if (acceptMouse)
		{
			// save the mouse position
			cursorPosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
		}
		
		// handle key events
		if (Event.current.isKey)
		{
			// return if not accepting key events
			if (!acceptKeyboard)
			{
				return;
			}
		}*/
		
		// save current render texture
		RenderTexture tempRenderTexture = RenderTexture.active; 
		
		// set the render texture to render the GUI onto
		if (Event.current.type == EventType.Repaint)
		{			
			RenderTexture.active = guiRenderTexture;
			GL.Clear (true, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));
		}
		
		// draw the VRGUI
		OnVRGUI();
		/*
		if (Event.current.type == EventType.Repaint)
		{	
			if (acceptMouse)
			{
				// draw the cursor
				GUI.DrawTexture(new Rect(cursorPosition.x, cursorPosition.y, cursorSize, cursorSize), 
					cursor, ScaleMode.StretchToFill);
			}

		}*/
		if (Event.current.type == EventType.Repaint) {			
			// restore the previous render texture
			RenderTexture.active = tempRenderTexture;
		}
	}
	
	public abstract void OnVRGUI();
}