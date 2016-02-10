using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class Map
{
	public int2 size = new int2(10, 10);
	[Range(0f, 1f)] public float obstaclePercent = 0.9f;
	public int seed = 10;

	public float minObstacleHeight;
	public float maxObstacleHeight;

	public Color foregroundColor;
	public Color backgroundColor;

	public int2 center
	{
		get
		{
			return new int2 (size.x / 2, size.y / 2);
		}
	}

}

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
	public float maxWidth = 60f;
	public float maxHeight = 40f;

	public Map[] maps;
	int mapIndex;

	public Transform tilePrefab;
	public Transform obstaclePrefab;
	public float tileScale = 1f;
	[Range(0f, 1f)] public float outlinePercent = 0.05f;

	Transform[,] tiles;

	Transform[] openTiles;
	int tileIndex;

	Map currentMap;

	//void Awake ()
	//{
	//	Spawner spawner = GameObject.FindGameObjectWithTag ("Spawner").GetComponent<Spawner> ();
	//	spawner.OnNewWave += OnNewWave;
	//	//GenerateMap ();
	//}

	public void OnNewWave (int waveNumber)
	{
		mapIndex = waveNumber;
		GenerateMap ();
	}

	public Transform GetRandomTile ()
	{
		if (tileIndex >= openTiles.Length)
			tileIndex = 0;

		Transform tile = openTiles [tileIndex];
		tileIndex++;
		return tile;
	}

	public Transform GetTile (Vector3 position)
	{
		float _s = 1f / tileScale;

		int x = Mathf.Clamp(Mathf.RoundToInt(position.x * _s + currentMap.size.x * 0.5f), 0, currentMap.size.x - 1);
		int y = Mathf.Clamp(Mathf.RoundToInt(position.z * _s + currentMap.size.y * 0.5f), 0, currentMap.size.y - 1);

		return tiles [x, y];
	}

	public void GenerateMap ()
	{
		// validate map
		if (maps == null || maps.Length == 0)
			return;

		mapIndex = Mathf.Clamp(mapIndex, 0, maps.Length - 1);
		currentMap = maps[mapIndex];

		// destroy previuos map
		GameObject mapHolder = GameObject.Find("Map holder");

		if (mapHolder)
			DestroyImmediate (mapHolder);
		
		// create holder
		mapHolder = new GameObject ("Map holder");

		tileScale = Mathf.Clamp (tileScale, 0.1f, Mathf.Min (maxWidth, maxHeight) - 1f);

		int maxSizeX = (int)(maxWidth  / tileScale);
		int maxSizeY = (int)(maxHeight / tileScale);

		currentMap.size.x = Mathf.Clamp(currentMap.size.x, 1, maxSizeX);
		currentMap.size.y = Mathf.Clamp(currentMap.size.y, 1, maxSizeY);

		{
			// create floor collider
			BoxCollider boxCollider = mapHolder.AddComponent<BoxCollider> ();
			boxCollider.size = new Vector3 (currentMap.size.x * tileScale, 1f, currentMap.size.y * tileScale);
			boxCollider.center = new Vector3 (0f, -0.5f, 0f);

			// create surrounding walls
			float width = currentMap.size.x * tileScale;
			float height = currentMap.size.y * tileScale;
			float w = (maxWidth - width) * 0.5f;
			float h = (maxHeight - height) * 0.5f;
			float x = (maxWidth + width) * 0.25f;
			float y = (maxHeight + height) * 0.25f;

			Transform wall = Instantiate(obstaclePrefab, new Vector3 (x, 0.5f, 0f), Quaternion.identity) as Transform;
			wall.localScale = new Vector3 (w, 1f, height);
			wall.SetParent (mapHolder.transform, false);

			wall = Instantiate(obstaclePrefab, new Vector3 (-x, 0.5f, 0f), Quaternion.identity) as Transform;
			wall.localScale = new Vector3 (w, 1f, height);
			wall.SetParent (mapHolder.transform, false);

			wall = Instantiate(obstaclePrefab, new Vector3 (0f, 0.5f, y), Quaternion.identity) as Transform;
			wall.localScale = new Vector3 (maxWidth, 1f, h);
			wall.SetParent (mapHolder.transform, false);

			wall = Instantiate(obstaclePrefab, new Vector3 (0f, 0.5f, -y), Quaternion.identity) as Transform;
			wall.localScale = new Vector3 (maxWidth, 1f, h);
			wall.SetParent (mapHolder.transform, false);
		}

		tiles = new Transform[currentMap.size.x, currentMap.size.y];

		// generate obstacles
		int[] indices = new int[currentMap.size.x * currentMap.size.y];
		for (int i = 0; i < indices.Length; i++)
			indices[i] = i;

		Random.seed = currentMap.seed;

		int obstacleCount = (int)(indices.Length * currentMap.obstaclePercent);

		bool[,] obstacleMap = new bool[currentMap.size.x, currentMap.size.y];
		int2 centerCoord = currentMap.center;

		int currentObstacleCount = 0;

		for (int i = 0; i < obstacleCount; i++)
		{
			int randomIndex = Random.Range (i, indices.Length);
			int randomTileIndex = indices[randomIndex];
			indices[randomIndex] = indices[i];

			int2 tileCoord = new int2 ();
			tileCoord.x = randomTileIndex % currentMap.size.x;
			tileCoord.y = randomTileIndex / currentMap.size.x;

			currentObstacleCount++;
			obstacleMap[tileCoord.x, tileCoord.y] = true;

			if (tileCoord != centerCoord && MapIsFullyAccessible (obstacleMap, currentObstacleCount, centerCoord))
			{
				float obstacleHeight = Mathf.Lerp (currentMap.minObstacleHeight, currentMap.maxObstacleHeight, Random.value);

				Vector3 pos = IndicesToCoordinate (tileCoord.x, tileCoord.y);
				pos.y = obstacleHeight * 0.5f;

				Transform newObstacle = Instantiate (obstaclePrefab, pos, Quaternion.identity) as Transform;
				newObstacle.localScale = new Vector3 (
					(1f - outlinePercent) * tileScale,
					obstacleHeight,
					(1f - outlinePercent) * tileScale
				);

				float colorInterpolation = (float)tileCoord.y / (float)currentMap.size.y;

				Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
				Material material = new Material(obstacleRenderer.sharedMaterial);
				material.color = Color.Lerp(currentMap.backgroundColor, currentMap.foregroundColor, colorInterpolation);
				obstacleRenderer.material = material;

				newObstacle.SetParent (mapHolder.transform, false);

				tiles [tileCoord.x, tileCoord.y] = newObstacle;
			}
			else
			{
				currentObstacleCount--;
				obstacleMap[tileCoord.x, tileCoord.y] = false;
			}
		}

		openTiles = new Transform[obstacleMap.Length - currentObstacleCount];
		int currentTile = 0;

		// generate tiles
		for (int x = 0; x < currentMap.size.x; x++)
			for (int y = 0; y < currentMap.size.y; y++)
			{
				if (!obstacleMap[x, y])
				{
					Vector3 pos = IndicesToCoordinate (x, y);
					Transform newTile = Instantiate (tilePrefab, pos, tilePrefab.rotation) as Transform;
					newTile.localScale = Vector3.one * (1f - outlinePercent) * tileScale;
					newTile.SetParent (mapHolder.transform, false);

					openTiles [currentTile] = newTile;
					currentTile++;

					tiles [x, y] = newTile;
				}
			}

		Utility.ShuffleArray (openTiles);

	}

	bool MapIsFullyAccessible (bool[,] obstacleMap, int currentObstacleCount, int2 tileCoord0)
	{
		bool[,] checkMap = new bool[currentMap.size.x, currentMap.size.y];

		Queue<int2> tileCoords = new Queue<int2> ();
		tileCoords.Enqueue (tileCoord0);
		checkMap[tileCoord0.x, tileCoord0.y] = true;
		int accessibleTileCount = 1;

		while (tileCoords.Count > 0)
		{
			int2 tileCoord = tileCoords.Dequeue ();

			if (tileCoord.x > 0)
			{
				int2 t = new int2 (tileCoord.x - 1, tileCoord.y);
				if (!obstacleMap [t.x, t.y] && !checkMap[t.x, t.y])
				{
					tileCoords.Enqueue (t);
					checkMap[t.x, t.y] = true;
					accessibleTileCount++;
				}
			}

			if (tileCoord.x < currentMap.size.x - 1)
			{
				int2 t = new int2 (tileCoord.x + 1, tileCoord.y);
				if (!obstacleMap [t.x, t.y] && !checkMap[t.x, t.y])
				{
					tileCoords.Enqueue (t);
					checkMap[t.x, t.y] = true;
					accessibleTileCount++;
				}
			}

			if (tileCoord.y > 0)
			{
				int2 t = new int2 (tileCoord.x, tileCoord.y - 1);
				if (!obstacleMap [t.x, t.y] && !checkMap[t.x, t.y])
				{
					tileCoords.Enqueue (t);
					checkMap[t.x, t.y] = true;
					accessibleTileCount++;
				}
			}

			if (tileCoord.y < currentMap.size.y - 1)
			{
				int2 t = new int2 (tileCoord.x, tileCoord.y + 1);
				if (!obstacleMap [t.x, t.y] && !checkMap[t.x, t.y])
				{
					tileCoords.Enqueue (t);
					checkMap[t.x, t.y] = true;
					accessibleTileCount++;
				}
			}
		}

		return accessibleTileCount == obstacleMap.Length - currentObstacleCount;
	}

	Vector3 IndicesToCoordinate (int x, int y)
	{
		return new Vector3 (-currentMap.size.x * 0.5f + 0.5f + x, 0f, -currentMap.size.y * 0.5f + 0.5f + y) * tileScale;
	}
}
