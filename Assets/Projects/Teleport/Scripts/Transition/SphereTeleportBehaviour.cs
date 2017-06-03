using UnityEngine;
using System.Collections;

public class SphereTeleportBehaviour : ITeleportBehaviour {


	enum EMode
	{
		Entering,
		Entered,
		Exiting,
		Exitted
	}

	TxKitEyes _camera;
	EMode _mode=EMode.Exitted;

	float _scale=0;


	Mesh GenerateSphere(float radius,int nbLong,int nbLat)
	{
		Mesh mesh = new Mesh();
		mesh.Clear();


		#region Vertices
		Vector3[] vertices = new Vector3[(nbLong+1) * nbLat + 2];
		float _pi = Mathf.PI;
		float _2pi = _pi * 2f;

		vertices[0] = Vector3.up * radius;
		for( int lat = 0; lat < nbLat; lat++ )
		{
			float a1 = _pi * (float)(lat+1) / (nbLat+1);
			float sin1 = Mathf.Sin(a1);
			float cos1 = Mathf.Cos(a1);

			for( int lon = 0; lon <= nbLong; lon++ )
			{
				float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
				float sin2 = Mathf.Sin(a2);
				float cos2 = Mathf.Cos(a2);

				vertices[ lon + lat * (nbLong + 1) + 1] = new Vector3( sin1 * cos2, cos1, sin1 * sin2 ) * radius;
			}
		}
		vertices[vertices.Length-1] = Vector3.up * -radius;
		#endregion

		#region Normales		
		Vector3[] normales = new Vector3[vertices.Length];
		for( int n = 0; n < vertices.Length; n++ )
			normales[n] = vertices[n].normalized;
		#endregion

		#region UVs
		Vector2[] uvs = new Vector2[vertices.Length];
		uvs[0] = Vector2.up;
		uvs[uvs.Length-1] = Vector2.zero;
		for( int lat = 0; lat < nbLat; lat++ )
			for( int lon = 0; lon <= nbLong; lon++ )
				uvs[lon + lat * (nbLong + 1) + 1] = new Vector2( (float)lon / nbLong, 1f - (float)(lat+1) / (nbLat+1) );
		#endregion

		#region Triangles
		int nbFaces = vertices.Length;
		int nbTriangles = nbFaces * 2;
		int nbIndexes = nbTriangles * 3;
		int[] triangles = new int[ nbIndexes ];

		//Top Cap
		int i = 0;
		for( int lon = 0; lon < nbLong; lon++ )
		{
			triangles[i++] = lon+2;
			triangles[i++] = lon+1;
			triangles[i++] = 0;
		}

		//Middle
		for( int lat = 0; lat < nbLat - 1; lat++ )
		{
			for( int lon = 0; lon < nbLong; lon++ )
			{
				int current = lon + lat * (nbLong + 1) + 1;
				int next = current + nbLong + 1;

				triangles[i++] = current;
				triangles[i++] = current + 1;
				triangles[i++] = next + 1;

				triangles[i++] = current;
				triangles[i++] = next + 1;
				triangles[i++] = next;
			}
		}

		//Bottom Cap
		for( int lon = 0; lon < nbLong; lon++ )
		{
			triangles[i++] = vertices.Length - 1;
			triangles[i++] = vertices.Length - (lon+2) - 1;
			triangles[i++] = vertices.Length - (lon+1) - 1;
		}
		#endregion

		mesh.vertices = vertices;
		mesh.normals = normales;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();
		;

		return mesh;

	}

	// Use this for initialization
	void Start () {
		_switchMode (EMode.Exitted);
		MeshFilter mf = gameObject.GetComponent<MeshFilter> ();
		if(mf==null)
			mf=gameObject.AddComponent<MeshFilter> ();
		Mesh mesh = GenerateSphere (0.8f, 40, 40);
		mesh.name = "SphereMesh";
		mf.mesh = mesh;
	}

	float _time=0;

	// Update is called once per frame
	public  void Update ()  {
		float t = Time.deltaTime;

		float factor = 1;
		float speed = 0.6f;
		float maxScale = 1;
		switch (_mode) {
		case EMode.Entering:
			_time += t;
			if (_curve != null) {
				factor = _curve.Evaluate (_time);
			} else
				factor = _time;
			_scale=maxScale*speed*factor;
			if(_time>1f)
			{
				_time=1;
				_scale = maxScale;
				_switchMode(EMode.Entered);
			}
			this.transform.localScale=new Vector3(_scale,_scale,_scale);

			break;
		case EMode.Entered:
			break;
		case EMode.Exiting:
			_time -= t;
			if (_curve != null) {
				factor = _curve.Evaluate (_time);
			} else
				factor = _time;
			_scale=maxScale*speed*factor;
			if(_time<0.0f)
			{
				_time=0;
				_scale = 0;
				_switchMode(EMode.Exitted);
			}
			this.transform.localScale=new Vector3(_scale,_scale,_scale);
			break;
		case EMode.Exitted:
			break;
		}
	}


	void _switchMode(EMode mode)
	{
		_mode = mode;
		switch (_mode) {
		case EMode.Entering:
			_scale = 0.0f;
			GetComponent<Renderer>().enabled=true;

			break;
		case EMode.Entered:
			if(OnEntered!=null)
				OnEntered(this);
			break;
		case EMode.Exiting:
			break;
		case EMode.Exitted:
			transform.localScale=Vector3.zero;
			GetComponent<Renderer>().enabled=false;
			if(OnExitted!=null)
				OnExitted(this);
			break;
		}
	}
	
	public override void OnEnter(TxKitEyes camera)
	{
		_camera = camera;
		_switchMode(EMode.Entering);
	}
	public override void OnExit()
	{
		_switchMode(EMode.Exiting);
	}

	public override bool IsActive()
	{
		return _mode != EMode.Exitted;
	}
}
