using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
	[System.Serializable]
	public struct Wave
	{
		public int enemyCount;
		public float timeBetweenSpawns;
	}

	public Wave[] waves;
	public Enemy enemy;

	public float timeBetweenCampingChecks = 2f;
	public float campRadius = 1.5f;
	float nextCampCheckTime;
	Vector3 prevCampPos;
	bool isCamping;


	LivingEntity playerEntity;

	int enemiesRemainingToSpawn;
	int enemiesRemainingAlive;
	float nextSpawnTime;
	int currentWaveNumber;
	Wave wave;

	MapGenerator map;

	event System.Action<int> OnNewWave;

	bool isDisabled;

	void NextWave ()
	{
		currentWaveNumber++;
		if (currentWaveNumber < waves.Length)
		{
			wave = waves[currentWaveNumber];
			enemiesRemainingToSpawn = wave.enemyCount;
			enemiesRemainingAlive = enemiesRemainingToSpawn;

			if (OnNewWave != null)
				OnNewWave (currentWaveNumber);

			//playerEntity.transform
			Transform tile = map.GetTile (Vector3.zero);
			playerEntity.transform.position = tile.position;
		}
	}

	void OnPlayerDeath ()
	{
		isDisabled = true;
	}

	void OnEnemyDeath ()
	{
		enemiesRemainingAlive--;
		if (enemiesRemainingAlive == 0)
			NextWave ();
	}

	IEnumerator SpawnEnemy ()
	{
		Color flashColor = Color.red;
		float spawnDelay = 1f;
		float flashSpeed = 4f;

		Transform tile = isCamping ? map.GetTile(playerEntity.transform.position) : map.GetRandomTile ();
		Material tileMaterial = tile.GetComponent<Renderer> ().material;
		Color initialColor = tileMaterial.color;

		float spawnTimer = 0f;

		while (spawnTimer < spawnDelay)
		{
			spawnTimer += Time.deltaTime;
			tileMaterial.color = Color.Lerp (initialColor, flashColor, Mathf.PingPong (spawnTimer * flashSpeed, 1f));

			yield return null;
		}


		Enemy spawnedEnemy = Instantiate (enemy, tile.position + Vector3.up, Quaternion.identity) as Enemy;
		spawnedEnemy.OnDeath += OnEnemyDeath;
	}

	void Start ()
	{
		map = GameObject.FindGameObjectWithTag ("MapGenerator").GetComponent<MapGenerator> ();
		OnNewWave += map.OnNewWave;

		playerEntity = GameObject.FindGameObjectWithTag ("Player").GetComponent<LivingEntity> ();
		nextCampCheckTime = Time.time + timeBetweenCampingChecks;
		prevCampPos = playerEntity.transform.position;

		playerEntity.OnDeath += OnPlayerDeath;

		currentWaveNumber = -1;
		NextWave ();
	}

	void Update ()
	{
		if (isDisabled)
			return;

		if (Time.time >= nextCampCheckTime)
		{
			nextCampCheckTime = Time.time + timeBetweenCampingChecks;
			isCamping = Vector3.Distance (prevCampPos, playerEntity.transform.position) < campRadius;
			prevCampPos = playerEntity.transform.position;
		}

		if (enemiesRemainingToSpawn > 0 && Time.time >= nextSpawnTime)
		{
			enemiesRemainingToSpawn--;
			nextSpawnTime = Time.time + wave.timeBetweenSpawns;

			StartCoroutine (SpawnEnemy ());
		}
	}
}
