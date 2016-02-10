using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshGenerator
{
	float x0;
	float y0;
	float increment;
	float height;

	int pointCountX;
	int pointCountY;

	public List<Vector3> vertices;
	public List<Vector2> texCoords;
	public List<int> indices;

	public List<Vector3> wallVertices;
	public List<int> wallIndices;

	public void GenerateMesh (bool[,] map, float squareSize, float wallHeight)
	{
		height = wallHeight;

		vertices = new List<Vector3> ();
		texCoords = new List<Vector2> ();
		indices = new List<int> ();
		wallVertices = new List<Vector3> ();
		wallIndices = new List<int> ();

		pointCountX = map.GetLength (0);
		pointCountY = map.GetLength (1);

		x0 = (1 - pointCountX) * squareSize * 0.5f;
		y0 = (1 - pointCountY) * squareSize * 0.5f;
		increment = squareSize * 0.5f;

		Dictionary<int2, int> points = new Dictionary<int2, int> ();

		// contour
		List<int2> lineList = new List<int2> ();

		for (int x = 0; x < pointCountX - 1; x++)
			for (int y = 0; y < pointCountY - 1; y++)
			{
				int configuration = 0;

				bool bottomLeft  = map[x, y];
				bool bottomRight = map[x + 1, y];
				bool topLeft     = map[x, y + 1];
				bool topRight    = map[x + 1, y + 1];

				if (bottomLeft)
					configuration |= 1;

				if (bottomRight)
					configuration |= 2;

				if (topLeft)
					configuration |= 4;

				if (topRight)
					configuration |= 8;

				switch (configuration)
				{
				case 0:
					break;

				case 1:
					{
						int2 point0 = new int2 (x * 2    , y * 2    );
						int2 point1 = new int2 (x * 2    , y * 2 + 1);
						int2 point2 = new int2 (x * 2 + 1, y * 2    );

						AddPoints (points, point0, point1, point2);
						lineList.Add (new int2 (points[point1], points[point2]));
					}
					break;

				case 2:
					{
						int2 point0 = new int2 (x * 2 + 1, y * 2    );
						int2 point1 = new int2 (x * 2 + 2, y * 2 + 1);
						int2 point2 = new int2 (x * 2 + 2, y * 2    );

						AddPoints (points, point0, point1, point2);
						lineList.Add (new int2 (points[point0], points[point1]));
					}
					break;

				case 3:
					{
						int2 point0 = new int2 (x * 2    , y * 2    );
						int2 point1 = new int2 (x * 2    , y * 2 + 1);
						int2 point2 = new int2 (x * 2 + 2, y * 2 + 1);
						int2 point3 = new int2 (x * 2 + 2, y * 2    );

						AddPoints (points, point0, point1, point2, point3);
						lineList.Add (new int2 (points[point1], points[point2]));
					}
					break;

				case 4:
					{
						int2 point0 = new int2 (x * 2    , y * 2 + 1);
						int2 point1 = new int2 (x * 2    , y * 2 + 2);
						int2 point2 = new int2 (x * 2 + 1, y * 2 + 2);

						AddPoints (points, point0, point1, point2);
						lineList.Add (new int2 (points[point2], points[point0]));
					}
					break;

				case 5:
					{
						int2 point0 = new int2 (x * 2    , y * 2    );
						int2 point1 = new int2 (x * 2    , y * 2 + 2);
						int2 point2 = new int2 (x * 2 + 1, y * 2 + 2);
						int2 point3 = new int2 (x * 2 + 1, y * 2    );

						AddPoints (points, point0, point1, point2, point3);
						lineList.Add (new int2 (points[point2], points[point3]));
					}
					break;

				case 6:
					{
						int2 point0 = new int2 (x * 2    , y * 2 + 1);
						int2 point1 = new int2 (x * 2    , y * 2 + 2);
						int2 point2 = new int2 (x * 2 + 1, y * 2 + 2);
						int2 point3 = new int2 (x * 2 + 2, y * 2 + 1);
						int2 point4 = new int2 (x * 2 + 2, y * 2    );
						int2 point5 = new int2 (x * 2 + 1, y * 2    );

						AddPoints (points, point0, point1, point2, point3, point4, point5);
						lineList.Add (new int2 (points[point2], points[point3]));
						lineList.Add (new int2 (points[point5], points[point0]));
					}
					break;

				case 7:
					{
						int2 point0 = new int2 (x * 2    , y * 2    );
						int2 point1 = new int2 (x * 2    , y * 2 + 2);
						int2 point2 = new int2 (x * 2 + 1, y * 2 + 2);
						int2 point3 = new int2 (x * 2 + 2, y * 2 + 1);
						int2 point4 = new int2 (x * 2 + 2, y * 2    );

						AddPoints (points, point0, point1, point2, point3, point4);
						lineList.Add (new int2 (points[point2], points[point3]));
					}
					break;

				case 8:
					{
						int2 point0 = new int2 (x * 2 + 1, y * 2 + 2);
						int2 point1 = new int2 (x * 2 + 2, y * 2 + 2);
						int2 point2 = new int2 (x * 2 + 2, y * 2 + 1);

						AddPoints (points, point0, point1, point2);
						lineList.Add (new int2 (points[point2], points[point0]));
					}
					break;

				case 9:
					{
						int2 point0 = new int2 (x * 2    , y * 2    );
						int2 point1 = new int2 (x * 2    , y * 2 + 1);
						int2 point2 = new int2 (x * 2 + 1, y * 2 + 2);
						int2 point3 = new int2 (x * 2 + 2, y * 2 + 2);
						int2 point4 = new int2 (x * 2 + 2, y * 2 + 1);
						int2 point5 = new int2 (x * 2 + 1, y * 2    );

						AddPoints (points, point0, point1, point2, point3, point4, point5);
						lineList.Add (new int2 (points[point1], points[point2]));
						lineList.Add (new int2 (points[point4], points[point5]));
					}
					break;

				case 10:
					{
						int2 point0 = new int2 (x * 2 + 1, y * 2    );
						int2 point1 = new int2 (x * 2 + 1, y * 2 + 2);
						int2 point2 = new int2 (x * 2 + 2, y * 2 + 2);
						int2 point3 = new int2 (x * 2 + 2, y * 2    );

						AddPoints (points, point0, point1, point2, point3);
						lineList.Add (new int2 (points[point0], points[point1]));
					}
					break;

				case 11:
					{
						int2 point0 = new int2 (x * 2    , y * 2    );
						int2 point1 = new int2 (x * 2    , y * 2 + 1);
						int2 point2 = new int2 (x * 2 + 1, y * 2 + 2);
						int2 point3 = new int2 (x * 2 + 2, y * 2 + 2);
						int2 point4 = new int2 (x * 2 + 2, y * 2    );

						AddPoints (points, point0, point1, point2, point3, point4);
						lineList.Add (new int2 (points[point1], points[point2]));
					}
					break;

				case 12:
					{
						int2 point0 = new int2 (x * 2    , y * 2 + 1);
						int2 point1 = new int2 (x * 2    , y * 2 + 2);
						int2 point2 = new int2 (x * 2 + 2, y * 2 + 2);
						int2 point3 = new int2 (x * 2 + 2, y * 2 + 1);

						AddPoints (points, point0, point1, point2, point3);
						lineList.Add (new int2 (points[point3], points[point0]));
					}
					break;

				case 13:
					{
						int2 point0 = new int2 (x * 2    , y * 2    );
						int2 point1 = new int2 (x * 2    , y * 2 + 2);
						int2 point2 = new int2 (x * 2 + 2, y * 2 + 2);
						int2 point3 = new int2 (x * 2 + 2, y * 2 + 1);
						int2 point4 = new int2 (x * 2 + 1, y * 2    );

						AddPoints (points, point0, point1, point2, point3, point4);
						lineList.Add (new int2 (points[point3], points[point4]));
					}
					break;

				case 14:
					{
						int2 point0 = new int2 (x * 2    , y * 2 + 1);
						int2 point1 = new int2 (x * 2    , y * 2 + 2);
						int2 point2 = new int2 (x * 2 + 2, y * 2 + 2);
						int2 point3 = new int2 (x * 2 + 2, y * 2    );
						int2 point4 = new int2 (x * 2 + 1, y * 2    );

						AddPoints (points, point0, point1, point2, point3, point4);
						lineList.Add (new int2 (points[point4], points[point0]));
					}
					break;

				case 15:
					{
						int2 point0 = new int2 (x * 2    , y * 2    );
						int2 point1 = new int2 (x * 2    , y * 2 + 2);
						int2 point2 = new int2 (x * 2 + 2, y * 2 + 2);
						int2 point3 = new int2 (x * 2 + 2, y * 2    );

						AddPoints (points, point0, point1, point2, point3);
					}
					break;
				}
			}

		// sort lines
		List<int[]> walls = new List<int[]> ();

		int startIndex = 0;
		for (int i = 0; i < lineList.Count; i++)
		{
			if (!NextLine (lineList, startIndex, i))
			{
				int[] cycle = new int[i + 1 - startIndex];
				for (int j = startIndex; j <= i; j++)
					cycle[j - startIndex] = lineList[j].x;

				walls.Add (cycle);

				startIndex = i + 1;
			}
		}


		// create walls
		foreach (int[] cycle in walls)
		{
			for (int i = 0; i < cycle.Length; i++)
				AddWallTile (cycle, i);
		}

		//// create main mesh
		//Mesh mesh = new Mesh ();
		//mesh.name = "Cave top";
		//mesh.vertices = vertices.ToArray ();
		//mesh.triangles = indices.ToArray ();
		//mesh.RecalculateNormals ();
		//
		//caveTop.mesh = mesh;



		//mesh = new Mesh ();
		//mesh.name = "Cave walls";
		//mesh.vertices = wallVertices.ToArray ();
		//mesh.triangles = wallIndices.ToArray ();
		//mesh.RecalculateNormals ();
		//
		//caveWalls.mesh = mesh;
		//
		//MeshCollider meshCollider = caveWalls.gameObject.GetComponent<MeshCollider> ();
		//meshCollider.sharedMesh = mesh;
	}

	void AddPoints (Dictionary<int2, int> points, params int2[] newPoints)
	{
		int index0 = 0;
		int index1 = 0;

		for (int i = 0; i < newPoints.Length; i++)
		{
			int index = IndexFromPoint (points, newPoints[i]);

			switch (i)
			{
			case 0:
				index0 = index;
				break;

			case 1:
				index1 = index;
				break;

			default:
				indices.Add (index0);
				indices.Add (index1);
				indices.Add (index);
				index1 = index;
				break;
			}
		}
	}

	int IndexFromPoint (Dictionary<int2, int> points, int2 point)
	{
		if (points.ContainsKey (point))
			return points [point];

		int index = points.Count;
		points.Add (point, index);

		vertices.Add(new Vector3 (x0 + point.x * increment, height, y0 + point.y * increment));
		texCoords.Add (new Vector2 (
			point.x / (float)pointCountX,
			(pointCountY - point.y) / (float)pointCountY
		));

		return index;
	}

	bool NextLine (List<int2> lines, int startIndex, int index)
	{
		int point = lines[index].y;
	
		if (point == lines[startIndex].x)
			return false; // cycled
	
		for (int i = index + 1; i < lines.Count; i++)
		{
			int2 line = lines[i];
			if (line.x == point)
			{
				lines[i] = lines[index + 1];
				lines[index + 1] = line;
				return true;
			}
		}

		Debug.LogWarning ("Not closed geometry");
		return false;
	}

	void AddWallTile (int[] cycle, int index)
	{
		int i0 = wallVertices.Count;
		int i1 = i0 + 1;
		int i2 = (index == cycle.Length - 1) ? i0 - (index * 2) : i1 + 1;
		int i3 = i2 + 1;

		Vector3 v = vertices[cycle[index]];
		wallVertices.Add (v);
		wallVertices.Add (new Vector3 (v.x, 0f, v.z));

		wallIndices.Add (i0);
		wallIndices.Add (i1);
		wallIndices.Add (i3);
		wallIndices.Add (i3);
		wallIndices.Add (i2);
		wallIndices.Add (i0);
	}
}
