using UnityEngine;
//using System;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;



public class CaveGenerator : MonoBehaviour
{
	public MeshFilter mainMeshFilter;
	public MeshFilter wallsMeshFilter;

	public int width = 60;
	public int height = 80;
	public float tileSize = 1f;
	public float wallHeight = 5f;
	public int smoothingInterationCount = 5;
	public int passageRadius = 2;

	public int seed;
	public bool useSeed;

	public int minIslandSize = 50;
	public int minRoomSize = 50;

	[Range(0, 100)]public int randomFillPercent = 50;

	bool[,] map;

	void Start ()
	{
		GenerateMap ();
	}

	void Update ()
	{
		if (Input.GetButtonDown("Fire1"))
			GenerateMap ();
	}

	void GenerateMap ()
	{
		map = new bool[width, height];
		RandomFillMap ();

		for (int i = 0; i < smoothingInterationCount; i++)
			SmoothMap ();

		ProcessMap ();

		MeshGenerator meshGen = new MeshGenerator ();
		meshGen.GenerateMesh (map, tileSize, wallHeight);

		Mesh mesh = new Mesh ();
		mesh.name = "Cave (main)";
		mesh.vertices = meshGen.vertices.ToArray ();
		mesh.triangles = meshGen.indices.ToArray ();

		int numVertices = meshGen.vertices.Count;
		Vector3[] normals = new Vector3[numVertices];
		Vector4[] tangents = new Vector4[numVertices];
		for (int i = 0; i < numVertices; i++)
		{
			normals[i] = Vector3.up;
			tangents[i] = new Vector4(1f, 0f, 0f, 1f);
		}
		mesh.normals = normals;
		mesh.tangents = tangents;
		//mesh.RecalculateNormals ();
		mesh.uv = meshGen.texCoords.ToArray ();
		mainMeshFilter.mesh = mesh;
		mesh.Optimize ();

		mesh = new Mesh ();
		mesh.name = "Cave (walls)";
		mesh.vertices = meshGen.wallVertices.ToArray ();
		mesh.triangles = meshGen.wallIndices.ToArray ();
		mesh.RecalculateNormals ();
		wallsMeshFilter.mesh = mesh;
		mesh.Optimize ();
		
		MeshCollider meshCollider = wallsMeshFilter.gameObject.GetComponent<MeshCollider> ();
		meshCollider.sharedMesh = mesh;
	}

	void RandomFillMap ()
	{
		if (useSeed)
			Random.seed = seed;
		//else
		//	seed = Random.seed;

		//System.Random random = new System.Random(seed);


		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
			{
				bool isBorder = (x == 0 || x == width - 1 || y == 0 || y == height - 1);
				map[x, y] = isBorder || Random.Range(0, 100) < randomFillPercent; //random.Next(0, 100) < randomFillPercent;
			}
	}

	void SmoothMap ()
	{
		//bool[,] newMap = new bool[width, height];

		for (int x = 1; x < width - 1; x++)
			for (int y = 1; y < height - 1; y++)
			{
				int neighbourCount = GetSurroundingWallCount (x, y);
				if (neighbourCount > 4)
					//newMap[x, y] = true;
					map[x, y] = true;
				else if (neighbourCount < 4)
					//newMap[x, y] = false;
					map[x, y] = false;
			}

		//for (int x = 1; x < width - 1; x++)
		//	for (int y = 1; y < height - 1; y++)
		//		map[x, y] = newMap[x, y];
	}

	int GetSurroundingWallCount (int X, int Y)
	{
		int wallCount = 0;

		for (int x = X - 1; x <= X + 1; x++)
			for (int y = Y - 1; y <= Y + 1; y++)
			{
				if (x != X || y != Y)
					wallCount += map [x, y] ? 1 : 0;
			}

		return wallCount;
	}

	void ProcessMap ()
	{
		List<List<int2>> wallRegions = GetRegions (true);
		foreach (List<int2> region in wallRegions)
		{
			if (region.Count < minIslandSize)
				foreach(int2 tile in region)
					map[tile.x, tile.y] = false;
		}

		List<Room> rooms = GetRooms ();

		if (rooms.Count <= 1)
			return;
		
		rooms.Sort ();
		rooms[0].SetAsMain ();

		Room.passageRadius = passageRadius;
		Room.mapWidth = width;
		Room.mapHeight = height;
		Room.ConnectClosestRooms (map, rooms);

		//List<List<int2>> roomRegions = GetRegions (false);
		//foreach (List<int2> region in roomRegions)
		//{
		//	if (region.Count < minRoomSize)
		//		foreach(int2 tile in region)
		//			map[tile.x, tile.y] = true;
		//}
	}

	List<List<int2>> GetRegions (bool tileType)
	{
		List<List<int2>> regions = new List<List<int2>> ();

		bool[,] checkMap = new bool[width, height];

		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
			{
				if (!checkMap[x, y] && map[x, y] == tileType)
				{
					List<int2> region = GetRegionTiles (checkMap, x, y);
					regions.Add (region);
				}
			}	

		return regions;
	}

	List<Room> GetRooms ()
	{
		List<Room> rooms = new List<Room> ();

		bool[,] checkMap = new bool[width, height];

		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
			{
				if (!checkMap[x, y] && map[x, y] == false)
				{
					List<int2> edgeTiles = new List<int2> ();
					List<int2> tiles = GetRegionTiles (checkMap, x, y, edgeTiles);

					if (tiles.Count < minRoomSize)
					{
						// delete small room
						foreach (int2 tile in tiles)
							map[tile.x, tile.y] = true;
					}
					else
						rooms.Add (new Room (edgeTiles, tiles.Count));
				}
			}	

		return rooms;
	}

	List<int2> GetRegionTiles (bool[,] checkMap, int startX, int startY, List<int2> edgeTiles = null)
	{
		List<int2> tiles = new List<int2> ();

		Queue<int2> queue = new Queue<int2> ();
		queue.Enqueue (new int2(startX, startY));
		checkMap[startX, startY] = true;
		bool tileType = map[startX, startY];

		while (queue.Count > 0)
		{
			int2 tile = queue.Dequeue ();

			tiles.Add (tile);

			bool isEdgeTile = false;

			bool[] allowed = { tile.x > 0, tile.x < width - 1, tile.y > 0, tile.y < height - 1 };

			for (int i = 0; i < 4; i++)
			{
				if (allowed[i])
				{
					//int dt = (i & 1) << 1 - 1; // interesting result
					int dt = ((i & 1) << 1) - 1;
					int c = -(i >> 1);
					int2 t = tile + new int2 (dt & ~c, dt & c);
					if (!checkMap[t.x, t.y])
					{
						if (map[t.x, t.y] == tileType)
						{
							checkMap[t.x, t.y] = true;
							queue.Enqueue (t);
						}
						else
							isEdgeTile = true;
					}
				}
			}

			if (edgeTiles != null && isEdgeTile)
				edgeTiles.Add (tile);
		}

		return tiles;
	}

	//void OnDrawGizmos ()
	//{
	//	if (map != null)
	//	{
	//		for (int x = 0; x < width; x++)
	//			for (int y = 0; y < height; y++)
	//			{
	//				//Gizmos.color = map[x, y] ? Color.black : Color.white;
	//				if (map[x,y])
	//				{
	//					Vector3 pos = new Vector3 (-width * 0.5f + 0.5f + x, wallHeight, -height * 0.5f + 0.5f + y);
	//					Gizmos.DrawCube (pos, Vector3.one * 0.4f);
	//				}
	//
	//			}
	//	}
	//}
}

