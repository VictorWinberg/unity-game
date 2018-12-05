using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {
	public Map[] maps;
	public int mapIndex;

	public Transform tilePrefab, obstaclePrefab, mapFloor, navmeshFloor, navmeshMaskPrefab;
	public Vector2 maxMapSize;

	[Range(0,1)]
	public float outlinePercent;

	public float tileSize;
	List<Coord> tileCoords;
	Queue<Coord> shuffledCoords;
	Queue<Coord> shuffledOpenCoords;
	Transform[,] tileMap;

	Map currentMap;

	public static MapGenerator Create() {
		MapGenerator generator = new GameObject ("Map").AddComponent<MapGenerator> ();
		generator.tilePrefab = ((GameObject)Resources.Load ("Tile")).transform;
		generator.obstaclePrefab = ((GameObject)Resources.Load ("Obstacle")).transform;
		generator.navmeshMaskPrefab = ((GameObject)Resources.Load ("Navmesh Mask")).transform;
		generator.maxMapSize = new Vector2 (40, 40);
		generator.mapIndex = 0;
		generator.outlinePercent = .1f;
		generator.tileSize = 1.5f;

		Transform mapFloor = GameObject.CreatePrimitive (PrimitiveType.Quad).transform;
		mapFloor.name = "Map Floor";
		mapFloor.parent = generator.transform;
		mapFloor.position = new Vector3 (0, -.1f, 0);
		mapFloor.rotation = Quaternion.AngleAxis (90f, Vector3.right);
		mapFloor.gameObject.isStatic = false;
		mapFloor.gameObject.AddComponent<BoxCollider> ();
		Destroy (mapFloor.gameObject.GetComponent<MeshCollider> ());
		mapFloor.GetComponent<Renderer> ().material = (Material)Resources.Load ("NavySmooth");
		generator.mapFloor = mapFloor;

		Transform navmeshFloor = GameObject.CreatePrimitive (PrimitiveType.Quad).transform;
		navmeshFloor.name = "Navmesh Floor";
		navmeshFloor.parent = generator.transform;
		navmeshFloor.localScale = new Vector3 (60, 60, 60);
		navmeshFloor.rotation = Quaternion.AngleAxis (90f, Vector3.right);
		navmeshFloor.gameObject.isStatic = true;
		navmeshFloor.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
		Destroy (navmeshFloor.gameObject.GetComponent<MeshCollider> ());
		generator.navmeshFloor = navmeshFloor.transform;

		int myMapSize = GameManager.waves;
		Map[] myMaps = new Map[myMapSize];
		for (int i = 0; i < myMapSize; i++) {
			myMaps[i] = new Map();
			myMaps[i].mapSize = new Coord((int)Random.Range(8, 14), (int)Random.Range(8, 14));
			myMaps[i].obstaclePercent = Random.Range(0f, 5f * Mathf.Log(i+3) / myMapSize);
			System.Random rand = new System.Random();
			myMaps[i].seed = rand.Next();
			myMaps[i].minObstacleHeight = Random.Range(0.2f, 1f);
			myMaps[i].maxObstacleHeight = myMaps[i].minObstacleHeight + Random.Range(0.2f, 3f);
			myMaps[i].foregroundColor = new Color(Random.Range(0,1f),Random.Range(0,1f),Random.Range(0,1f));
			myMaps[i].backgroundColor = new Color(Random.Range(0,1f),Random.Range(0,1f),Random.Range(0,1f));
		}
		generator.maps = myMaps;
		FindObjectOfType<Spawner> ().OnNewWave += generator.OnNewWave;
		return generator;
	}

	void OnNewWave(int waveNumber) {
		mapIndex = waveNumber - 1;
		GenerateMap ();
	}

	public void GenerateMap() {
		currentMap = maps [mapIndex];
		tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
		System.Random r = new System.Random (currentMap.seed);

		// Generating coords
		tileCoords = new List<Coord> ();
		for (int x = 0; x < currentMap.mapSize.x; x++) {
			for (int y = 0; y < currentMap.mapSize.y; y++) {
				tileCoords.Add(new Coord(x,y));
			}
		}
		shuffledCoords = new Queue<Coord> (Utility.ShuffleArray (tileCoords.ToArray(), currentMap.seed));

		// Create mapholder object
		string holderName = "Generated Map";
		if (transform.Find (holderName)) {
			DestroyImmediate(transform.Find(holderName).gameObject);
		}

		Transform mapHolder = new GameObject (holderName).transform;
		mapHolder.parent = transform;

		// Spawning tiles
		for (int x = 0; x < currentMap.mapSize.x; x++) {
			for(int y = 0; y < currentMap.mapSize.y; y++) {
				Vector3 tilePosition = CoordToPosition(x, y);
				Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
				newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
				newTile.parent = mapHolder;
				tileMap[x, y] = newTile;
			}
		}

		// Spawning obstacles
		bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

		int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
		int currentObstacleCount = 0;
		List<Coord> openCoords = new List<Coord> (tileCoords);

		for (int i = 0; i < obstacleCount; i++) {
			Coord randomCoord = getRandomCoord();
			obstacleMap[randomCoord.x, randomCoord.y] = true;
			currentObstacleCount++;

			if(randomCoord != currentMap.mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount)) {
				float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)r.NextDouble());
				Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

				Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight/2, Quaternion.identity) as Transform;
				newObstacle.localScale = new Vector3 ((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
				newObstacle.parent = mapHolder;

				Renderer render = newObstacle.GetComponent<Renderer>();
				Material material = new Material(render.sharedMaterial);
				float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
				material.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
				render.sharedMaterial = material;

				openCoords.Remove(randomCoord);

			} else {
				obstacleMap[randomCoord.x, randomCoord.y] = false;
				currentObstacleCount--;
			}
		}
		shuffledOpenCoords = new Queue<Coord> (Utility.ShuffleArray (openCoords.ToArray(), currentMap.seed));

		// Creating navmesh mask
		Transform maskLeft = Instantiate (navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskLeft.parent = mapHolder;
		maskLeft.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
		
		Transform maskRight = Instantiate (navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskRight.parent = mapHolder;
		maskRight.localScale = new Vector3 ((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
		
		Transform maskTop = Instantiate (navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskTop.parent = mapHolder;
		maskTop.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
		
		Transform maskBottom = Instantiate (navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskBottom.parent = mapHolder;
		maskBottom.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

		// Creating walls
		Transform wallWest = new GameObject ("Wall(Clone)").transform;
		BoxCollider coll = wallWest.gameObject.AddComponent<BoxCollider> ();
		coll.size = new Vector3 (1, 10, (currentMap.mapSize.y + .5f) * tileSize);
		wallWest.position = Vector3.left * currentMap.mapSize.x / 2f * tileSize + Vector3.up * (coll.size.y / 2f - .1f);
		wallWest.parent = mapHolder;

		Transform wallEast = new GameObject ("Wall(Clone)").transform;
		coll = wallEast.gameObject.AddComponent<BoxCollider> ();
		coll.size = new Vector3 (1, 10, (currentMap.mapSize.y + .5f) * tileSize);
		wallEast.position = Vector3.right * currentMap.mapSize.x / 2f * tileSize + Vector3.up * (coll.size.y / 2f - .1f);
		wallEast.parent = mapHolder;

		Transform wallNorth = new GameObject ("Wall(Clone)").transform;
		coll = wallNorth.gameObject.AddComponent<BoxCollider> ();
		coll.size = new Vector3 ((currentMap.mapSize.x + .5f) * tileSize, 10, 1);
		wallNorth.position = Vector3.forward * currentMap.mapSize.y / 2f * tileSize + Vector3.up * (coll.size.y / 2f - .1f);
		wallNorth.parent = mapHolder;

		Transform wallSouth = new GameObject ("Wall(Clone)").transform;
		coll = wallSouth.gameObject.AddComponent<BoxCollider> ();
		coll.size = new Vector3 ((currentMap.mapSize.x + .5f) * tileSize, 10, 1);
		wallSouth.position = Vector3.back * currentMap.mapSize.y / 2f * tileSize + Vector3.up * (coll.size.y / 2f - .1f);
		wallSouth.parent = mapHolder;

		navmeshFloor.localScale = new Vector3 (maxMapSize.x, maxMapSize.y) * tileSize;
		mapFloor.localScale = new Vector3 (currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize, 0.05f); 
	}

	/** Flood-fill algorithm*/
	bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount) {
		bool[,] mapFlags = new bool[obstacleMap.GetLength (0), obstacleMap.GetLength (1)];
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (currentMap.mapCentre);
		mapFlags [currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

		int accessibleTileCount = 1; // centre accessible

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();

			for(int x = -1; x <= 1; x++) {
				for(int y = -1; y <= 1; y++) {
					int neighbourX = tile.x + x;
					int neighbourY = tile.y + y;
					if(x == 0 || y == 0) {
						if(neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1)) {
							if(!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY]) {
								mapFlags[neighbourX, neighbourY] = true;
								queue.Enqueue(new Coord(neighbourX, neighbourY));
								accessibleTileCount++;
							}
						}
					}
				}
			}
		}

		int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
		return targetAccessibleTileCount == accessibleTileCount;
	}

	Vector3 CoordToPosition(int x, int y) {
		return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;;
	}

	public Transform getTileFromPosition(Vector3 position) {
		int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
		int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
		x = Mathf.Clamp (x, 0, tileMap.GetLength (0) - 1);
		y = Mathf.Clamp (y, 0, tileMap.GetLength (1) - 1);
		return tileMap [x, y];
	}

	public Coord getRandomCoord() {
		Coord randomCoord = shuffledCoords.Dequeue ();
		shuffledCoords.Enqueue (randomCoord);
		return randomCoord;
	}

	public Transform getRandomOpenTile() {
		Coord randomCoord = shuffledOpenCoords.Dequeue ();
		shuffledOpenCoords.Enqueue (randomCoord);
		return tileMap [randomCoord.x, randomCoord.y];
	}

	public Color getInitialTileColor() {
		return tilePrefab.GetComponent<Renderer> ().sharedMaterial.color;
		//return tileMap [0, 0].GetComponent<Renderer> ().material.color;
	}

	[System.Serializable]
	public struct Coord {
		public int x, y;

		public Coord(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public static bool operator == (Coord c1, Coord c2) {
			return c1.x == c2.x && c1.y == c2.y;
		}

		public static bool operator != (Coord c1, Coord c2) {
			return !(c1 == c2);
		}
	}

	[System.Serializable]
	public class Map {
		public Coord mapSize;
		[Range(0,1)]
		public float obstaclePercent;
		public int seed;
		public float minObstacleHeight, maxObstacleHeight;
		public Color foregroundColor, backgroundColor;

		public Coord mapCentre {
			get {
				return new Coord(mapSize.x / 2, mapSize.y / 2);
			}
		}
	}
}
