using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	public bool developerMode;

	public Wave[] waves  { get; private set; }
	public Enemy enemy;

	LivingEntity player;

	Wave currentWave;
	int currentWaveNumber;

	int enemiesRemainingToSpawn, enemiesRemainingAlive;
	float nextSpawnTime;

	MapGenerator map;

	float idleTimeCheck = 2;
	float idleThresholdDistance = 1.5f;
	float nextIdleTimeCheck;
	Vector3 idlePositionPrevious;
	bool isIdle;

	bool isDisabled;

	public event System.Action<int> OnNewWave;

	public static Spawner Create() {
		GameObject go = new GameObject ("Spawner");
		Spawner spawner = go.AddComponent<Spawner> ();
		spawner.enemy = ((GameObject)Resources.Load ("Enemy")).GetComponent<Enemy>();
		spawner.developerMode = true;
		Wave[] myWaves = new Wave[GameManager.waves];
		for (int i = 0; i < myWaves.Length; i++) {
			myWaves [i] = new Wave ();
			myWaves[i].enemyCount = (int)Random.Range(3 * (i + 1), 5 * (i+ 1));
			myWaves[i].timeBetweenSpawns = Random.Range(.2f, 1f);

			myWaves[i].moveSpeed = 2f + 0.2f * i;
			myWaves[i].damage = (int)(20 * Mathf.Log(i + 3) / (i + 1));
			myWaves[i].health = (int)(i / 5 + 1);
			myWaves[i].skinColor = new Color(Random.Range(0,1f),Random.Range(0,1f),Random.Range(0,1f));
		}
		spawner.waves = myWaves;
		return spawner;
	}

	void Start () {
		if(player == null)
			player = FindObjectOfType<Player> ();

		nextIdleTimeCheck = idleTimeCheck + Time.time;
		idlePositionPrevious = player.transform.position;
		player.OnDeath += OnPlayerDeath;

		map = FindObjectOfType<MapGenerator> ();
		NextWave ();
	}

	public Player setPlayer {
		set 
		{
			player = value; 
		}
	}

	void Update () {
		if (isDisabled)
			return;

		if (Time.time > nextIdleTimeCheck) {
			nextIdleTimeCheck = idleTimeCheck + Time.time;

			isIdle = (Vector3.Distance(player.transform.position, idlePositionPrevious) < idleThresholdDistance);
			idlePositionPrevious = player.transform.position;
		}

		if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime) {
			enemiesRemainingToSpawn--;
			nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

			StartCoroutine("SpawnEnemy");
		}

		if (developerMode) {
			if(Input.GetKeyDown(KeyCode.Return)) {
				StopCoroutine("SpawnEnemy");
				foreach(Enemy enemy in FindObjectsOfType<Enemy>()) {
					GameObject.Destroy(enemy.gameObject);
				}
				NextWave();
			}
		}
	}

	IEnumerator SpawnEnemy() {
		float spawnDelay = 1;
		float tileFlashSpeed = 4;

		Transform spawnTile = isIdle ? map.getTileFromPosition(player.transform.position) : map.getRandomOpenTile ();
		Material tileMaterial = spawnTile.GetComponent<Renderer> ().material;
		Color initialColor = map.getInitialTileColor ();
		Color flashColor = Color.red;
		float spawnTimer = 0;

		while (spawnTimer < spawnDelay) {

			tileMaterial.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

			spawnTimer += Time.deltaTime;
			yield return null;
		}
		tileMaterial.color = initialColor;
		Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
		spawnedEnemy.OnDeath += OnEnemyDeath;

		spawnedEnemy.SetCharacteristics (currentWave.moveSpeed, currentWave.damage, currentWave.health, currentWave.skinColor);
	}

	void OnPlayerDeath() {
		isDisabled = true;
	}

	void OnEnemyDeath (){
		enemiesRemainingAlive--;

		if (enemiesRemainingAlive == 0) {
			NextWave();
		}
	}

	void ResetPlayerPosition() {
		player.transform.position = map.getTileFromPosition(Vector3.zero).position + Vector3.up * 1.5f;
	}

	void NextWave() {
		if (currentWaveNumber > 0) {
			AudioManager.instance.PlaySound ("Level Complete");
		}

		currentWaveNumber++;
		if (currentWaveNumber - 1 < waves.Length) {
			currentWave = waves [currentWaveNumber - 1];
			
			enemiesRemainingToSpawn = currentWave.enemyCount;
			enemiesRemainingAlive = enemiesRemainingToSpawn;

			if(OnNewWave != null) OnNewWave(currentWaveNumber);
			ResetPlayerPosition();
		}
	}

	[System.Serializable]
	public class Wave {
		public bool infinite;
		public int enemyCount;
		public float timeBetweenSpawns;

		public float moveSpeed;
		public int damage;
		public float health;
		public Color skinColor;
	}
}
