using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
	public Map map;
	public int seed;

	public Transform tilePrefab, obstaclePrefab, wallPrefab, mapFloor, navmeshFloor, navmeshMaskPrefab;
	public Vector2 maxMapSize;

	[Range (0, 1)]
	public float outlinePercent;

	public float tileSize;
	List<Coord> tileCoords;
	Queue<Coord> shuffledCoords;
	Queue<Coord> shuffledOpenCoords;
	Transform[, ] tileMap;

	public void GenerateMap () {
		int level = Level.instance.level;
		tileMap = new Transform[map.mapSize.x, map.mapSize.y];
		System.Random r = new System.Random (seed + level);

		// Generating coords
		tileCoords = new List<Coord> ();
		for (int x = 0; x < map.mapSize.x; x++) {
			for (int y = 0; y < map.mapSize.y; y++) {
				tileCoords.Add (new Coord (x, y));
			}
		}
		shuffledCoords = new Queue<Coord> (Utility.ShuffleArray (tileCoords.ToArray (), seed + level));

		// Create mapholder object
		string holderName = "Generated Map";
		if (transform.Find (holderName)) {
			DestroyImmediate (transform.Find (holderName).gameObject);
		}

		Transform mapHolder = new GameObject (holderName).transform;
		mapHolder.parent = transform;

		// Spawning tiles
		for (int x = 0; x < map.mapSize.x; x++) {
			for (int y = 0; y < map.mapSize.y; y++) {
				Vector3 tilePosition = CoordToPosition (x, y);
				Transform newTile = Instantiate (tilePrefab, tilePosition, Quaternion.Euler (Vector3.right * 90)) as Transform;
				newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
				newTile.parent = mapHolder;
				tileMap[x, y] = newTile;
			}
		}

		// Spawning obstacles
		bool[, ] obstacleMap = new bool[(int) map.mapSize.x, (int) map.mapSize.y];

		int obstacleCount = (int) (map.mapSize.x * map.mapSize.y * map.obstaclePercent);
		int currentObstacleCount = 0;
		List<Coord> openCoords = new List<Coord> (tileCoords);

		for (int i = 0; i < obstacleCount; i++) {
			Coord randomCoord = getRandomCoord ();
			obstacleMap[randomCoord.x, randomCoord.y] = true;
			currentObstacleCount++;

			if (randomCoord != map.mapCentre && MapIsFullyAccessible (obstacleMap, currentObstacleCount)) {
				float obstacleHeight = Mathf.Lerp (map.minObstacleHeight, map.maxObstacleHeight, (float) r.NextDouble ());
				Vector3 obstaclePosition = CoordToPosition (randomCoord.x, randomCoord.y);

				Transform newObstacle = Instantiate (obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
				newObstacle.localScale = new Vector3 ((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
				newObstacle.parent = mapHolder;

				Renderer render = newObstacle.GetComponent<Renderer> ();
				Material material = new Material (render.sharedMaterial);
				float colorPercent = randomCoord.y / (float) map.mapSize.y;
				material.color = Color.Lerp (map.foregroundColor, map.backgroundColor, colorPercent);
				render.sharedMaterial = material;

				openCoords.Remove (randomCoord);

			} else {
				obstacleMap[randomCoord.x, randomCoord.y] = false;
				currentObstacleCount--;
			}
		}
		shuffledOpenCoords = new Queue<Coord> (Utility.ShuffleArray (openCoords.ToArray (), seed + level));

		// Creating navmesh mask
		Transform maskLeft = Instantiate (navmeshMaskPrefab, Vector3.left * (map.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskLeft.parent = mapHolder;
		maskLeft.localScale = new Vector3 ((maxMapSize.x - map.mapSize.x) / 2f, 1, map.mapSize.y) * tileSize;

		Transform maskRight = Instantiate (navmeshMaskPrefab, Vector3.right * (map.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskRight.parent = mapHolder;
		maskRight.localScale = new Vector3 ((maxMapSize.x - map.mapSize.x) / 2f, 1, map.mapSize.y) * tileSize;

		Transform maskTop = Instantiate (navmeshMaskPrefab, Vector3.forward * (map.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskTop.parent = mapHolder;
		maskTop.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y - map.mapSize.y) / 2f) * tileSize;

		Transform maskBottom = Instantiate (navmeshMaskPrefab, Vector3.back * (map.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskBottom.parent = mapHolder;
		maskBottom.localScale = new Vector3 (maxMapSize.x, 1, (maxMapSize.y - map.mapSize.y) / 2f) * tileSize;

		navmeshFloor.localScale = new Vector3 (maxMapSize.x, maxMapSize.y) * tileSize;
		mapFloor.localScale = new Vector3 (map.mapSize.x * tileSize, map.mapSize.y * tileSize, 0.05f);

		// Creating outer walls
		float height = 2f;
		Vector3 upVector = Vector3.up * (height / 2f - .1f);
		float margin = 0.5f * tileSize;
		float width = map.mapSize.x * tileSize + margin;
		float depth = map.mapSize.y * tileSize + margin;

		Transform wallWest = Instantiate (wallPrefab, Vector3.left * width / 2f + upVector, Quaternion.identity) as Transform;
		wallWest.parent = mapHolder;
		wallWest.localScale = new Vector3 (tileSize / 2f, height, depth - margin);

		Transform wallEast = Instantiate (wallPrefab, Vector3.right * width / 2f + upVector, Quaternion.identity) as Transform;
		wallEast.parent = mapHolder;
		wallEast.localScale = new Vector3 (tileSize / 2f, height, depth - margin);

		Transform wallNorth = Instantiate (wallPrefab, Vector3.forward * depth / 2f + upVector, Quaternion.identity) as Transform;
		wallNorth.parent = mapHolder;
		wallNorth.localScale = new Vector3 (width + margin, height, tileSize / 2f);

		Transform wallSouth = Instantiate (wallPrefab, Vector3.back * depth / 2f + upVector, Quaternion.identity) as Transform;
		wallSouth.parent = mapHolder;
		wallSouth.localScale = new Vector3 (width + margin, height, tileSize / 2f);

		// Creating inner walls
		float ratio = (float) r.NextDouble ();
		float barWidth = (ratio * (map.mapSize.x - 3f) + 3f) * tileSize + margin;
		float barDepth = ((1 - ratio) * (map.mapSize.y - 3f) + 3f) * tileSize + margin;

		Transform barWest = Instantiate (wallPrefab, Vector3.left * barWidth / 2f + upVector, Quaternion.identity) as Transform;
		barWest.parent = mapHolder;
		barWest.localScale = new Vector3 (tileSize / 2f, height, barDepth - margin);

		Transform barEast = Instantiate (wallPrefab, Vector3.right * barWidth / 2f + upVector, Quaternion.identity) as Transform;
		barEast.parent = mapHolder;
		barEast.localScale = new Vector3 (tileSize / 2f, height, barDepth - margin);

		Transform barNorth = Instantiate (wallPrefab, Vector3.forward * barDepth / 2f + upVector, Quaternion.identity) as Transform;
		barNorth.parent = mapHolder;
		barNorth.localScale = new Vector3 (barWidth + margin, height, tileSize / 2f);

		Transform barSouth = Instantiate (wallPrefab, Vector3.back * barDepth / 2f + upVector, Quaternion.identity) as Transform;
		barSouth.parent = mapHolder;
		barSouth.localScale = new Vector3 (barWidth + margin, height, tileSize / 2f);
	}

	/** Flood-fill algorithm*/
	bool MapIsFullyAccessible (bool[, ] obstacleMap, int currentObstacleCount) {
		bool[, ] mapFlags = new bool[obstacleMap.GetLength (0), obstacleMap.GetLength (1)];
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (map.mapCentre);
		mapFlags[map.mapCentre.x, map.mapCentre.y] = true;

		int accessibleTileCount = 1; // centre accessible

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue ();

			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					int neighbourX = tile.x + x;
					int neighbourY = tile.y + y;
					if (x == 0 || y == 0) {
						if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength (0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength (1)) {
							if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY]) {
								mapFlags[neighbourX, neighbourY] = true;
								queue.Enqueue (new Coord (neighbourX, neighbourY));
								accessibleTileCount++;
							}
						}
					}
				}
			}
		}

		int targetAccessibleTileCount = (int) (map.mapSize.x * map.mapSize.y - currentObstacleCount);
		return targetAccessibleTileCount == accessibleTileCount;
	}

	Vector3 CoordToPosition (int x, int y) {
		return new Vector3 (-map.mapSize.x / 2f + 0.5f + x, 0, -map.mapSize.y / 2f + 0.5f + y) * tileSize;;
	}

	public Transform getTileFromPosition (Vector3 position) {
		int x = Mathf.RoundToInt (position.x / tileSize + (map.mapSize.x - 1) / 2f);
		int y = Mathf.RoundToInt (position.z / tileSize + (map.mapSize.y - 1) / 2f);
		x = Mathf.Clamp (x, 0, tileMap.GetLength (0) - 1);
		y = Mathf.Clamp (y, 0, tileMap.GetLength (1) - 1);
		return tileMap[x, y];
	}

	public Coord getRandomCoord () {
		Coord randomCoord = shuffledCoords.Dequeue ();
		shuffledCoords.Enqueue (randomCoord);
		return randomCoord;
	}

	public Transform getRandomOpenTile () {
		Coord randomCoord = shuffledOpenCoords.Dequeue ();
		shuffledOpenCoords.Enqueue (randomCoord);
		return tileMap[randomCoord.x, randomCoord.y];
	}

	public Color getInitialTileColor () {
		return tilePrefab.GetComponent<Renderer> ().sharedMaterial.color;
		//return tileMap [0, 0].GetComponent<Renderer> ().material.color;
	}

	[System.Serializable]
	public struct Coord {
		public int x, y;

		public Coord (int x, int y) {
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
		[Range (0, 1)]
		public float obstaclePercent;
		public float minObstacleHeight, maxObstacleHeight;
		public Color foregroundColor, backgroundColor;

		public Coord mapCentre {
			get {
				return new Coord (mapSize.x / 2, mapSize.y / 2);
			}
		}
	}
}