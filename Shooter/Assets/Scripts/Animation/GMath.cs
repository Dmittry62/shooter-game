using UnityEngine;
using System.Collections;

public class GMath
{
	public static float epsilon = 1e-5f;

	public static bool Small(float v)
	{
		return Mathf.Abs (v) < epsilon;
	}

	public static bool Small(Vector2 v)
	{
		return Mathf.Abs (v.x) < epsilon && Mathf.Abs (v.y) < epsilon;
	}

	public static bool Small(Vector3 v)
	{
		return Mathf.Abs (v.x) < epsilon && Mathf.Abs (v.y) < epsilon && Mathf.Abs (v.z) < epsilon;
	}

	public static bool Small(Vector4 v)
	{
		return Mathf.Abs (v.x) < epsilon && Mathf.Abs (v.y) < epsilon && Mathf.Abs (v.z) < epsilon && Mathf.Abs (v.w) < epsilon;
	}

	public static Quaternion Twist(Quaternion q, Vector3 n)
	{
		Vector3 p = Vector3.Dot(n, new Vector3(q.x, q.y, q.z)) * n;

		if (Mathf.Abs (p.x) < epsilon &&
			Mathf.Abs (p.y) < epsilon &&
			Mathf.Abs (p.z) < epsilon &&
			Mathf.Abs (q.w) < epsilon)
			return Quaternion.identity;

		Vector4 v = new Vector4(p.x, p.y, p.z, q.w).normalized;

		return new Quaternion(v.x, v.y, v.z, v.w);
	}
}
