using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour {

	public bool developerMode;

	public Wave[] waves;
	public Customer customer;

	LivingEntity player;

	Wave currentWave;
	int currentWaveNumber;

	int customersRemainingToSpawn, customersRemainingAlive;
	float nextSpawnTime;

	MapGenerator map;

	float idleTimeCheck = 2;
	float idleThresholdDistance = 1.5f;
	float nextIdleTimeCheck;
	Vector3 idlePositionPrevious;
	bool isIdle;

	bool isDisabled;

	public event System.Action<int> OnNewWave;

	// public static Spawner Create () {
	// 	GameObject go = new GameObject ("Spawner");
	// 	Spawner spawner = go.AddComponent<Spawner> ();
	// 	spawner.customer = ((GameObject) Resources.Load ("Customer")).GetComponent<Customer> ();
	// 	spawner.developerMode = true;
	// 	Wave[] myWaves = new Wave[GameManager.waves];
	// 	for (int i = 0; i < myWaves.Length; i++) {
	// 		myWaves[i] = new Wave ();
	// 		myWaves[i].customerCount = (int) Random.Range (3 * (i + 1), 5 * (i + 1));
	// 		myWaves[i].timeBetweenSpawns = Random.Range (.2f, 1f);

	// 		myWaves[i].moveSpeed = 2f + 0.2f * i;
	// 		myWaves[i].damage = (int) (20 * Mathf.Log (i + 3) / (i + 1));
	// 		myWaves[i].health = (int) (i / 5 + 1);
	// 		myWaves[i].skinColor = new Color (Random.Range (0, 1f), Random.Range (0, 1f), Random.Range (0, 1f));
	// 	}
	// 	spawner.waves = myWaves;
	// 	return spawner;
	// }

	void Start () {
		if (player == null)
			player = FindObjectOfType<Player> ();

		nextIdleTimeCheck = idleTimeCheck + Time.time;
		idlePositionPrevious = player.transform.position;
		player.OnDeath += OnPlayerDeath;

		map = FindObjectOfType<MapGenerator> ();
		NextWave ();
	}

	public Player setPlayer {
		set {
			player = value;
		}
	}

	void Update () {
		if (isDisabled)
			return;

		if (Time.time > nextIdleTimeCheck) {
			nextIdleTimeCheck = idleTimeCheck + Time.time;

			isIdle = (Vector3.Distance (player.transform.position, idlePositionPrevious) < idleThresholdDistance);
			idlePositionPrevious = player.transform.position;
		}

		if ((customersRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime) {
			customersRemainingToSpawn--;
			nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

			StartCoroutine ("SpawnCustomer");
		}

		if (developerMode) {
			if (Input.GetKeyDown (KeyCode.Return)) {
				StopCoroutine ("SpawnCustomer");
				foreach (Customer customer in FindObjectsOfType<Customer> ()) {
					GameObject.Destroy (customer.gameObject);
				}
				NextWave ();
			}
		}
	}

	IEnumerator SpawnCustomer () {
		float spawnDelay = 1;
		float tileFlashSpeed = 4;

		Transform spawnTile = isIdle ? map.getTileFromPosition (player.transform.position) : map.getRandomOpenTile ();
		Material tileMaterial = spawnTile.GetComponent<Renderer> ().material;
		Color initialColor = map.getInitialTileColor ();
		Color flashColor = Color.red;
		float spawnTimer = 0;

		while (spawnTimer < spawnDelay) {

			tileMaterial.color = Color.Lerp (initialColor, flashColor, Mathf.PingPong (spawnTimer * tileFlashSpeed, 1));

			spawnTimer += Time.deltaTime;
			yield return null;
		}
		tileMaterial.color = initialColor;
		Customer spawnedCustomer = Instantiate (customer, spawnTile.position + Vector3.up, Quaternion.identity) as Customer;
		spawnedCustomer.OnDeath += OnCustomerDeath;

		spawnedCustomer.SetCharacteristics (currentWave.moveSpeed, currentWave.damage, currentWave.health, currentWave.skinColor);
	}

	void OnPlayerDeath () {
		isDisabled = true;
	}

	void OnCustomerDeath () {
		customersRemainingAlive--;

		if (customersRemainingAlive == 0) {
			NextWave ();
		}
	}

	void NextWave () {
		if (currentWaveNumber > 0) {
			AudioManager.instance.PlaySound ("Level Complete");
		}

		currentWaveNumber++;
		if (currentWaveNumber - 1 < waves.Length) {
			currentWave = waves[currentWaveNumber - 1];

			customersRemainingToSpawn = currentWave.customerCount;
			customersRemainingAlive = customersRemainingToSpawn;

			OnNewWave (currentWaveNumber);
		} else {
			FindObjectOfType<CompleteLevel> ().Complete ();
		}
	}

	[System.Serializable]
	public class Wave {
		public bool infinite;
		public int customerCount;
		public float timeBetweenSpawns;

		public float moveSpeed;
		public int damage;
		public float health;
		public Color skinColor;
	}
}