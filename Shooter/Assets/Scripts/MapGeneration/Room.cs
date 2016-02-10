using UnityEngine;
using System;
using System.Collections.Generic;

class Room : System.IComparable<Room>
{
	public static int passageRadius;
	public static int mapWidth;
	public static int mapHeight;

	List<int2> tiles;
	List<int2> edgeTiles;
	List<Room> connectedRooms;
	int size;
	bool isAccessibleFromMainRoom;

	public Room (List<int2> _edgeTiles, int _size)
	{
		edgeTiles = _edgeTiles;
		size = _size;
		connectedRooms = new List<Room> ();
	}

	public int CompareTo (Room room)
	{
		return room.size.CompareTo(size);
	}

	public void SetAsMain ()
	{
		isAccessibleFromMainRoom = true;
	}

	public static void ConnectClosestRooms (bool[,] map, List<Room> rooms, bool forceAccessibilityFromMainRoom = false)
	{
		List<Room> roomListA = new List<Room> ();
		List<Room> roomListB = new List<Room> ();

		if (forceAccessibilityFromMainRoom)
		{
			foreach (Room room in rooms)
				if (room.isAccessibleFromMainRoom)
					roomListA.Add (room);
				else
					roomListB.Add (room);
		}
		else
		{
			roomListA = rooms;
			roomListB = rooms;
		}

		int bestDistance = 0;
		bool connectionFound = false;
		int2 bestTileA = new int2 ();
		int2 bestTileB = new int2 ();
		Room bestRoomA = null;
		Room bestRoomB = null;

		foreach (Room roomA in roomListA)
		{
			if (!forceAccessibilityFromMainRoom)
			{
				connectionFound = false;
				if (roomA.connectedRooms.Count > 0)
					continue;
			}

			foreach (Room roomB in roomListB)
			{
				if (roomA == roomB || roomA.connectedRooms.Contains (roomB))
					continue;

				for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
					for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
					{
						int2 tileA = roomA.edgeTiles[tileIndexA];
						int2 tileB = roomB.edgeTiles[tileIndexB];

						int dist = (tileA - tileB).sqrMagnitude;
						if (dist < bestDistance || !connectionFound)
						{
							connectionFound = true;
							bestDistance = dist;

							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
					}
			}

			if (connectionFound && !forceAccessibilityFromMainRoom)
				CreatePassage (map, bestRoomA, bestRoomB, bestTileA, bestTileB);
		}

		if (connectionFound && forceAccessibilityFromMainRoom)
		{
			CreatePassage (map, bestRoomA, bestRoomB, bestTileA, bestTileB);
			ConnectClosestRooms (map, rooms, true);
		}

		if (!forceAccessibilityFromMainRoom)
			ConnectClosestRooms (map, rooms, true);
	}

	static void CreatePassage (bool[,] map, Room roomA, Room roomB, int2 tileA, int2 tileB)
	{
		//Debug.DrawLine (
		//	new Vector3 (-100 * 0.5f + tileA.x + 0.5f, 5f, -80 * 0.5f + tileA.y + 0.5f),
		//	new Vector3 (-100 * 0.5f + tileB.x + 0.5f, 5f, -80 * 0.5f + tileB.y + 0.5f),
		//	Color.green,
		//	Mathf.Infinity,
		//	false
		//);

		roomA.connectedRooms.Add (roomB);
		roomB.connectedRooms.Add (roomA);
		if (roomA.isAccessibleFromMainRoom)
			roomB.SetAccessibleFromMainRoom ();
		else if (roomB.isAccessibleFromMainRoom)
			roomA.SetAccessibleFromMainRoom ();

		int2 dt = tileB - tileA;

		if (Mathf.Abs (dt.x) > Mathf.Abs (dt.y))
		{
			// y = a * x + b
			float _dx = 1f / dt.x;
			float a = dt.y * _dx;
			float b = (tileA.y * tileB.x - tileB.y * tileA.x) * _dx;

			int2 t0 = dt.x > 0 ? tileA : tileB;
			int2 t1 = dt.x > 0 ? tileB : tileA;

			for (int x = t0.x; x <= t1.x; x++)
			{
				int y = Mathf.RoundToInt(a * x + b);
				DrawCircle (map, x, y);


				//int Y = Mathf.RoundToInt(a * x + b);
				//for (int y = Y - passageRadius; y <= Y + passageRadius; y++)
				//	map[x,y] = false;
				//float y = a * x + b;
				//int y0 = Mathf.FloorToInt(y);
				//int y1 = Mathf.CeilToInt(y);
				//if (y1 == y0)
				//	y1 += (int)Mathf.Sign (a);
				//map[x, y0] = false;
				//map[x, y1] = false;
			}
		}
		else
		{
			// x = a * y + b
			float _dy = 1f / dt.y;
			float a = dt.x * _dy;
			float b = (tileA.x * tileB.y - tileB.x * tileA.y) * _dy;

			int2 t0 = dt.y > 0 ? tileA : tileB;
			int2 t1 = dt.y > 0 ? tileB : tileA;

			for (int y = t0.y; y <= t1.y; y++)
			{
				int x = Mathf.RoundToInt(a * y + b);
				DrawCircle (map, x, y);

				//int X = Mathf.RoundToInt(a * y + b);
				//for (int x = X - passageRadius; x <= X + passageRadius; x++)
				//	map[x,y] = false;
				//float x = a * y + b;
				//int x0 = Mathf.FloorToInt(x);
				//int x1 = Mathf.CeilToInt(x);
				//if (x1 == x0)
				//	x1 += (int)Mathf.Sign (a);
				//map[x0, y] = false;
				//map[x1, y] = false;
			}
		}
	}

	static void DrawCircle (bool[,] map, int x, int y)
	{
		for (int i = -passageRadius; i <= passageRadius; i++)
			for (int j = -passageRadius; j <= passageRadius; j++)
				if (x - i > 0 && x + i < mapWidth - 1 && y - j > 0 && y + j < mapHeight - 1)
				{
					if (i * i + j * j <= passageRadius * passageRadius)
						map [x + i, y + j] = false;
				}
	}

	void SetAccessibleFromMainRoom ()
	{
		if (!isAccessibleFromMainRoom)
		{
			isAccessibleFromMainRoom = true;
			foreach (Room room in connectedRooms)
				//room.isAccessibleFromMainRoom = true;
				room.SetAccessibleFromMainRoom ();
		}
	}
}

