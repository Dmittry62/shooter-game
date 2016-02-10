using System.Collections;

[System.Serializable]
public struct int2
{
	public int2 (int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public int sqrMagnitude
	{
		get
		{
			return x * x + y * y;
		}
	}

	public static int2 operator+ (int2 a, int2 b)
	{
		return new int2 (a.x + b.x, a.y + b.y);
	}

	public static int2 operator- (int2 a, int2 b)
	{
		return new int2 (a.x - b.x, a.y - b.y);
	}

	public static bool operator== (int2 a, int2 b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public static bool operator!= (int2 a, int2 b)
	{
		return a.x != b.x || a.y != b.y;
	}

	public override bool Equals (object obj)
	{
		int2 other = (int2)obj;
		return x == other.x && y == other.y;
	}

	public override int GetHashCode ()
	{
		return x ^ y;
	}

	public int x;
	public int y;
}

public static class Utility
{
	public static void ShuffleArray<T> (T[] array)
	{
		System.Random generator = new System.Random ();

		for (int i = 0; i < array.Length - 1; i++)
		{
			int index = generator.Next (i, array.Length);

			T t = array [i];
			array [i] = array [index];
			array [index] = t;
		}
	}
}
