using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System;

 internal static class Utilities  {
	public static float AxisSize=0.1f;

	public static void DrawAxis(Matrix4x4 frame)
	{
		Vector3 X = frame.GetColumn (0)*AxisSize;
		Vector3 Y = frame.GetColumn (1)*AxisSize;
		Vector3 Z = frame.GetColumn (2)*AxisSize;
		Vector3 pos = frame.GetColumn (3);
		Debug.DrawLine (pos,pos+X,Color.red);
		Debug.DrawLine (pos,pos+Y,Color.green);
		Debug.DrawLine (pos,pos+Z,Color.blue);
	}

	public static float NormalizeAngle (this float angle)
	{
		while (angle>180)
			angle -= 360;
		while (angle<-180)
			angle += 360;
		return angle;
	}

		public static void FromMatrix4x4(this Transform transform, Matrix4x4 matrix)
		{
			transform.localScale = matrix.GetScale();
			transform.localRotation = matrix.GetRotation();
			transform.localPosition = matrix.GetPosition();
		}


		private static float _copysign(float sizeval, float signval)
		{
			return Mathf.Sign(signval) == 1 ? Mathf.Abs(sizeval) : -Mathf.Abs(sizeval);
		}
		public static Quaternion GetRotation(this Matrix4x4 matrix)
		{

			Quaternion q = new Quaternion();
			q.w = Mathf.Sqrt(Mathf.Max(0, 1 + matrix.m00 + matrix.m11 + matrix.m22)) / 2;
			q.x = Mathf.Sqrt(Mathf.Max(0, 1 + matrix.m00 - matrix.m11 - matrix.m22)) / 2;
			q.y = Mathf.Sqrt(Mathf.Max(0, 1 - matrix.m00 + matrix.m11 - matrix.m22)) / 2;
			q.z = Mathf.Sqrt(Mathf.Max(0, 1 - matrix.m00 - matrix.m11 + matrix.m22)) / 2;
			q.x = _copysign(q.x, matrix.m21 - matrix.m12);
			q.y = _copysign(q.y, matrix.m02 - matrix.m20);
			q.z = _copysign(q.z, matrix.m10 - matrix.m01);
			return q;
		}
		public static Vector3 GetPosition(this Matrix4x4 matrix)
		{
			var x = matrix.m03;
			var y = matrix.m13;
			var z = matrix.m23;
			
			return new Vector3(x, y, z);
		}
		
		public static Vector3 GetScale(this Matrix4x4 m)
		{
			var x = Mathf.Sqrt(m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
			var y = Mathf.Sqrt(m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
			var z = Mathf.Sqrt(m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);
			
			return new Vector3(x, y, z);
		}
	public static byte[] GetBytes(string str)
	{
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		return bytes;
	}
	
	public static string GetString(byte[] bytes)
	{
		char[] chars = new char[bytes.Length / sizeof(char)];
		System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
		return new string(chars);
	}
	public static string LocalIPAddress()
	{
		IPHostEntry host;
		string localIP = "";
		try
		{
			host = Dns.GetHostEntry(Dns.GetHostName());
		}catch(Exception e) {
			return "127.0.0.1";
		}
		foreach (IPAddress ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				localIP = ip.ToString();
				break;
			}
		}
		return localIP;
	}

	public static float Map(float x,float a0,float b0,float a1,float b1)
	{
		return a1 + (b1-a1)*((x - a0) / (b0 - a0));
	}

	public static string ToExportString(this Quaternion q)
	{
		return string.Format ("{0},{1},{2},{3}", q.w.ToString("f6"), q.x.ToString("f6"), q.y.ToString("f6"), q.z.ToString("f6"));
	}
	
	public static string ToExportString(this Vector2 q)
	{
		return string.Format ("{0},{1}",  q.x, q.y);
	}
	public static string ToExportString(this Vector3 q)
	{
		return string.Format ("{0},{1},{2}",  q.x.ToString("f4"), q.y.ToString("f4"), q.z.ToString("f4"));
	}

	public static Vector2 ParseVector2(string str)
	{
		try
		{
			string[] splits= str.Split (",".ToCharArray (), 2);
			if (splits.Length < 2)
				return Vector2.zero;
			return new Vector2(float.Parse(splits[0]),float.Parse(splits[1]));
		}catch
		{
			return Vector2.zero;
		}
	}
	public static Vector3 ParseVector3(string str)
	{
		try
		{
			string[] splits= str.Split (",".ToCharArray (), 3);
			if (splits.Length < 3)
				return Vector3.zero;
			return new Vector3(float.Parse(splits[0]),float.Parse(splits[1]),float.Parse(splits[2]));
		}catch
		{
			return Vector3.zero;
		}
	}
	public static Vector4 ParseVector4(string str)
	{
		try
		{
			string[] splits= str.Split (",".ToCharArray (), 4);
			if (splits.Length < 4)
				return Vector4.zero;
			return new Vector4(float.Parse(splits[0]),float.Parse(splits[1]),float.Parse(splits[2]),float.Parse(splits[3]));
		}catch
		{
			return Vector4.zero;
		}
	}
	public static Quaternion ParseQuaternion(string str)
	{
		try
		{
			string[] splits= str.Split (",".ToCharArray (), 4);
			if (splits.Length < 4)
				return Quaternion.identity;
			return new Quaternion(float.Parse(splits[1]),float.Parse(splits[2]),float.Parse(splits[3]),float.Parse(splits[0]));
		}catch
		{
				return Quaternion.identity;
		}
	}

	public static string ReadStringNative(this BinaryReader reader)
	{
		int len=reader.ReadInt32 ();
		if (len == 0)
			return "";
		byte[] data=reader.ReadBytes (len);
		return Encoding.UTF8.GetString (data);
	}
	public static void WriteStringNative(this BinaryWriter writer,string s)
	{
		writer.Write (s.Length);
		writer.Write (Encoding.UTF8.GetBytes(s.ToCharArray ()));
	}


	public static Transform FindChildRecursive(this Transform parent,string name)
	{
		if (parent.name == name)
			return parent;
		foreach (Transform t in parent) {
			Transform r = t.FindChildRecursive (name);
			if (r != null)
				return r;
		}
		return null;
	}



	static float[] boxesForGauss(float sigma, int n)  // standard deviation, number of boxes
	{
		var wIdeal = Mathf.Sqrt((12*sigma*sigma/n)+1);  // Ideal averaging filter width 
		int wl = (int)Mathf.Floor(wIdeal); 
		if(wl%2==0) wl--;
		var wu = wl+2;

		var mIdeal = (12*sigma*sigma - n*wl*wl - 4*n*wl - 3*n)/(-4*wl - 4);
		var m = mIdeal;
		// var sigmaActual = Math.sqrt( (m*wl*wl + (n-m)*wu*wu - n)/12 );

		float[] sizes =new float[n]; 
		for(var i=0; i<n; i++) sizes[i]=(i<m?wl:wu);
		return sizes;
	}


	public static void GaussBlur (float[,] scl, float[,] tcl, int w, int h, float r) {
		var bxs = boxesForGauss(r, 3);
		boxBlur_3 (scl, tcl, w, h, (bxs[0]-1)/2);
		boxBlur_3 (tcl, scl, w, h, (bxs[1]-1)/2);
		boxBlur_3 (scl, tcl, w, h, (bxs[2]-1)/2);
	}
	static void boxBlur_3 (float[,] scl, float[,] tcl, int w, int h, float r) {
		for(var i=0; i<scl.GetLength(0); i++) 
			for(var j=0; j<scl.GetLength(1); j++) 
				tcl[i,j] = scl[i,j];
		boxBlurH_3(tcl, scl, w, h, r);
		boxBlurT_3(scl, tcl, w, h, r);
	}
	static void boxBlurH_3 (float[,] scl, float[,] tcl, int w, int h, float r) {
		for(var i=0; i<h; i++)
			for(var j=0; j<w; j++)  {
				float val = 0;
				for(var ix=j-r; ix<j+r+1; ix++) {
					int x = (int)Mathf.Min(w-1, Mathf.Max(0, ix));
					val += scl[x,i];
				}
				tcl[j,i] = val/(r+r+1);
			}
	}   
	static void boxBlurT_3 (float[,] scl, float[,] tcl, int w, int h, float r) {
		for(var i=0; i<h; i++)
			for(var j=0; j<w; j++) {
				float val = 0;
				for(var iy=i-r; iy<i+r+1; iy++) {
					int y = (int)Mathf.Min(h-1, Mathf.Max(0, iy));
					val += scl[j,y];
				}
				tcl[j,i] = val/(r+r+1);
			}
	}

    static int GetNextFilename(string pattern)
    {
        string tmp = string.Format(pattern, 1);
        if (tmp == pattern)
            throw new ArgumentException("The pattern must include an index place-holder", "pattern");

        if (!File.Exists(tmp))
            return 1; // short-circuit if no matches

        int min = 1, max = 2; // min is inclusive, max is exclusive/untested

        while (File.Exists(string.Format(pattern, max)))
        {
            min = max;
            max *= 2;
        }

        while (max != min + 1)
        {
            int pivot = (max + min) / 2;
            if (File.Exists(string.Format(pattern, pivot)))
                min = pivot;
            else
                max = pivot;
        }

        return max;

    }
    public static int GetNextAvailableFileID(string path)
    {
        const string numberPattern = "{0}";
        // Short-cut if already available
        //if (!File.Exists(path))
        //	return 0;

        // If path has extension then insert the number pattern just before the extension and return next filename
        if (Path.HasExtension(path))
            return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), numberPattern));

        // Otherwise just append the pattern to the path and return next filename
        return GetNextFilename(path + numberPattern);
    }


}
