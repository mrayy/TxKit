using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GUITools {
	//some code taken from: http://hyunkell.com/blog/rts-style-unit-selection-in-unity-5/

	static Material LineMaterial;
	static Texture2D _whiteTexture;
	public static Texture2D WhiteTexture
	{
		get
		{
			if( _whiteTexture == null )
			{
				_whiteTexture = new Texture2D( 1, 1 );
				_whiteTexture.SetPixel( 0, 0, Color.white );
				_whiteTexture.Apply();
			}

			return _whiteTexture;
		}
	}

	static void _prepareLineMaterial()
	{
		if (LineMaterial == null) {
			LineMaterial = new Material (Shader.Find("Lines/Colored Blended"));
			LineMaterial.hideFlags = HideFlags.HideAndDontSave;
			LineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			Debug.Log ("Creating Line Material");
		}
	}
	public static void DrawLine(Vector2 a,Vector2 b,Color color)
	{
		_prepareLineMaterial ();
		Camera cam = Camera.main;
		GL.PushMatrix ();
		LineMaterial.SetPass (0);
		GL.Begin(GL.LINES);
		GL.Color(color);
		GL.Vertex(a);
		GL.Vertex(b);
		GL.End();
		GL.PopMatrix ();
	}

	public static void DrawCircle(Vector2 center,float radius,int N,Color color,int skip=0)
	{
		_prepareLineMaterial ();

		Camera cam = Camera.main;
		GL.PushMatrix ();
		LineMaterial.SetPass (0);
		GL.Begin(GL.LINES);
		GL.Color(color);

		float step = 2*Mathf.PI / (float)N;
		Vector2 v1,v2;
		for (int j = 0; j < 4; ++j) {
			float a = 0;

			v1 = center;
			v1.x += Mathf.Cos (a) * radius;
			v1.y += Mathf.Sin (a) * radius;
			int s = skip;
			for (int i = 0; i <= N; ++i) {
			
				v2 = center;
				v2.x += Mathf.Cos (a) * radius;
				v2.y += Mathf.Sin (a) * radius;
				s--;
				if (s <= 0) {
					GL.Vertex (v1);
					GL.Vertex (v2);
					s = skip;
				}
				v1 = v2;

				a += step;
			}
			radius -= 1f;
		}

		GL.End();
		GL.PopMatrix ();
	}

	public static void DrawScreenRect( Rect rect, Color color )
	{
		GUI.color = color;
		GUI.DrawTexture( rect, WhiteTexture );
		GUI.color = Color.white;
	}
	public static void DrawScreenRectBorder( Rect rect, float thickness, Color color )
	{
		GUI.color = color;
		// Top
		GUI.DrawTexture( new Rect( rect.xMin, rect.yMin, rect.width, thickness ), WhiteTexture );
		// Left
		GUI.DrawTexture( new Rect( rect.xMin, rect.yMin, thickness, rect.height ) , WhiteTexture);
		// Right
		GUI.DrawTexture( new Rect( rect.xMax - thickness, rect.yMin, thickness, rect.height ), WhiteTexture);
		// Bottom
		GUI.DrawTexture( new Rect( rect.xMin, rect.yMax - thickness, rect.width, thickness ) , WhiteTexture);
		GUI.color = Color.white;
    }
    static Rect UniformRect = new Rect(0, 0, 1, 1);
    public static void GraphicsDrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        Graphics.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), WhiteTexture, UniformRect, 0, 0, 0, 0, color);
        // Left
        Graphics.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), WhiteTexture, UniformRect, 0, 0, 0, 0, color);
        // Right
        Graphics.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), WhiteTexture, UniformRect, 0, 0, 0, 0, color);
        // Bottom
        Graphics.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), WhiteTexture, UniformRect, 0, 0, 0, 0, color);
	}

	static Mesh CircleMesh=null;

	static void CreateCircle()
	{
		if (CircleMesh != null)
			return;
		int N = 180;
		float step = 360.0f /(float) N;
		List<Vector3> vertexList=new List<Vector3>();
		List<int> lines=new List<int>();
		Quaternion q=Quaternion.Euler(0,0,step);
		vertexList.Add (new Vector3 (0f, 0.5f, 0f));
		vertexList.Add (q*vertexList[0]);

		lines.Add(0);
		lines.Add(1);

		for(int i=0;i<N-1;i++)
		{
			lines.Add(vertexList.Count-1);
			lines.Add(vertexList.Count);

			vertexList.Add (q * vertexList [vertexList.Count - 1]);
		}

		Mesh mesh=new Mesh();
		mesh.vertices = vertexList.ToArray();
		mesh.SetIndices (lines.ToArray (), MeshTopology.Lines, 0);

		CircleMesh = mesh;
	}


	public static void GraphicsDrawScreeCircleBorder(Rect rect, float thickness, Color color)
	{
		Matrix4x4 m=new Matrix4x4();
		LineMaterial.color = color;
		Graphics.DrawMesh (CircleMesh, m, LineMaterial, 0);
	}
	public static Rect GetScreenRect( Vector3 screenPosition1, Vector3 screenPosition2 )
	{
		// Move origin from bottom left to top left
		screenPosition1.y = Screen.height - screenPosition1.y;
		screenPosition2.y = Screen.height - screenPosition2.y;
		// Calculate corners
		var topLeft = Vector3.Min( screenPosition1, screenPosition2 );
		var bottomRight = Vector3.Max( screenPosition1, screenPosition2 );
		// Create Rect
		return Rect.MinMaxRect( topLeft.x, topLeft.y, bottomRight.x, bottomRight.y );
	}

}
