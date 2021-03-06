﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
	public Map map;
	public Workarea[] workareas;
	public int seed;

	public Transform tilePrefab, obstaclePrefab, wallPrefab, barPrefab, counterPrefab, mapFloor, navmeshFloor, navmeshMaskPrefab;
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
		float margin = 0.5f * tileSize;
		float width = map.mapSize.x * tileSize + margin;
		float depth = map.mapSize.y * tileSize + margin;
		float height = 2f;
		Vector3 wallVectorUp = Vector3.up * (height / 2f - .1f);

		Transform wallWest = Instantiate (wallPrefab, Vector3.left * width / 2f + wallVectorUp, Quaternion.identity) as Transform;
		wallWest.parent = mapHolder;
		wallWest.localScale = new Vector3 (tileSize / 2f, height, depth - margin);

		Transform wallEast = Instantiate (wallPrefab, Vector3.right * width / 2f + wallVectorUp, Quaternion.identity) as Transform;
		wallEast.parent = mapHolder;
		wallEast.localScale = new Vector3 (tileSize / 2f, height, depth - margin);

		Transform wallNorth = Instantiate (wallPrefab, Vector3.forward * depth / 2f + wallVectorUp, Quaternion.identity) as Transform;
		wallNorth.parent = mapHolder;
		wallNorth.localScale = new Vector3 (width + margin, height, tileSize / 2f);

		Transform wallSouth = Instantiate (wallPrefab, Vector3.back * depth / 2f + wallVectorUp, Quaternion.identity) as Transform;
		wallSouth.parent = mapHolder;
		wallSouth.localScale = new Vector3 (width + margin, height, tileSize / 2f);

		// Creating inner walls
		float ratio = (float) r.NextDouble ();
		int barSizeX = Mathf.RoundToInt (ratio * (map.mapSize.x - 3) + 3);
		int barSizeY = Mathf.RoundToInt ((1 - ratio) * (map.mapSize.y - 3f) + 3f);

		float barWidth = barSizeX * tileSize + margin / 4f;
		float barDepth = barSizeY * tileSize + margin / 4f;
		float barHeight = 1f;

		bool openVertical = barSizeX > barSizeY;

		Vector3 barVectorUp = Vector3.up * (barHeight / 2f - 0.1f);

		Transform barHolder = new GameObject ("Bar").transform;
		barHolder.parent = mapHolder;

		if (openVertical) {
			float openPosW = (Mathf.Floor ((barSizeX + 1) / 2f) + 0.125f) / 2f * tileSize;
			float openWidthW = (Mathf.Ceil ((barSizeX - 1) / 2f) + 0.125f) * tileSize;
			float openPosE = (Mathf.Ceil ((barSizeX + 1) / 2f) + 0.125f) / 2f * tileSize;
			float openWidthE = (Mathf.Floor ((barSizeX - 1) / 2f) + 0.125f) * tileSize;

			Transform barWest = Instantiate (barPrefab, Vector3.left * barWidth / 2f + barVectorUp, Quaternion.identity) as Transform;
			barWest.parent = barHolder;
			barWest.localScale = new Vector3 (tileSize / 8f, barHeight, barDepth - margin / 4f);

			Transform barEast = Instantiate (barPrefab, Vector3.right * barWidth / 2f + barVectorUp, Quaternion.identity) as Transform;
			barEast.parent = barHolder;
			barEast.localScale = new Vector3 (tileSize / 8f, barHeight, barDepth - margin / 4f);

			Transform barNorthW = Instantiate (barPrefab, Vector3.forward * barDepth / 2f + Vector3.left * openPosW + barVectorUp, Quaternion.identity) as Transform;
			barNorthW.parent = barHolder;
			barNorthW.localScale = new Vector3 (openWidthW, barHeight, tileSize / 8f);

			Transform barNorthE = Instantiate (barPrefab, Vector3.forward * barDepth / 2f + Vector3.right * openPosE + barVectorUp, Quaternion.identity) as Transform;
			barNorthE.parent = barHolder;
			barNorthE.localScale = new Vector3 (openWidthE, barHeight, tileSize / 8f);

			Transform barSouthW = Instantiate (barPrefab, Vector3.back * barDepth / 2f + Vector3.left * openPosW + barVectorUp, Quaternion.identity) as Transform;
			barSouthW.parent = barHolder;
			barSouthW.localScale = new Vector3 (openWidthW, barHeight, tileSize / 8f);

			Transform barSouthE = Instantiate (barPrefab, Vector3.back * barDepth / 2f + Vector3.right * openPosE + barVectorUp, Quaternion.identity) as Transform;
			barSouthE.parent = barHolder;
			barSouthE.localScale = new Vector3 (openWidthE, barHeight, tileSize / 8f);
		} else {
			float openPosN = (Mathf.Floor ((barSizeY + 1) / 2f)) / 2f * tileSize;
			float openDepthN = (Mathf.Ceil ((barSizeY - 1) / 2f)) * tileSize;
			float openPosS = (Mathf.Ceil ((barSizeY + 1) / 2f)) / 2f * tileSize;
			float openDepthS = (Mathf.Floor ((barSizeY - 1) / 2f)) * tileSize;

			Transform barNorth = Instantiate (barPrefab, Vector3.forward * barDepth / 2f + barVectorUp, Quaternion.identity) as Transform;
			barNorth.parent = barHolder;
			barNorth.localScale = new Vector3 (barWidth + margin / 4f, barHeight, tileSize / 8f);

			Transform barSouth = Instantiate (barPrefab, Vector3.back * barDepth / 2f + barVectorUp, Quaternion.identity) as Transform;
			barSouth.parent = barHolder;
			barSouth.localScale = new Vector3 (barWidth + margin / 4f, barHeight, tileSize / 8f);

			Transform barWestN = Instantiate (barPrefab, Vector3.left * barWidth / 2f + Vector3.forward * openPosN + barVectorUp, Quaternion.identity) as Transform;
			barWestN.parent = barHolder;
			barWestN.localScale = new Vector3 (tileSize / 8f, barHeight, openDepthN);

			Transform barWestS = Instantiate (barPrefab, Vector3.left * barWidth / 2f + Vector3.back * openPosS + barVectorUp, Quaternion.identity) as Transform;
			barWestS.parent = barHolder;
			barWestS.localScale = new Vector3 (tileSize / 8f, barHeight, openDepthS);

			Transform barEastN = Instantiate (barPrefab, Vector3.right * barWidth / 2f + Vector3.forward * openPosN + barVectorUp, Quaternion.identity) as Transform;
			barEastN.parent = barHolder;
			barEastN.localScale = new Vector3 (tileSize / 8f, barHeight, openDepthN);

			Transform barEastS = Instantiate (barPrefab, Vector3.right * barWidth / 2f + Vector3.back * openPosS + barVectorUp, Quaternion.identity) as Transform;
			barEastS.parent = barHolder;
			barEastS.localScale = new Vector3 (tileSize / 8f, barHeight, openDepthS);
		}

		for (int x = 0; x < barSizeX; x++) {
			if (!openVertical || x != (int) barSizeX / 2) {
				Transform counter = Instantiate (counterPrefab, Vector3.left * ((barSizeX - 1) / 2f - x) * tileSize + Vector3.forward * (barSizeY - 1) / 2f * tileSize + Vector3.up * 0.4f, Quaternion.identity) as Transform;
				counter.parent = barHolder;
				counter = Instantiate (counterPrefab, Vector3.left * ((barSizeX - 1) / 2f - x) * tileSize + Vector3.back * (barSizeY - 1) / 2f * tileSize + Vector3.up * 0.4f, Quaternion.identity) as Transform;
				counter.parent = barHolder;
			}
		}

		for (int y = 1; y < barSizeY - 1; y++) {
			if (openVertical || y != (int) barSizeY / 2) {
				Transform counter = Instantiate (counterPrefab, Vector3.forward * ((barSizeY - 1) / 2f - y) * tileSize + Vector3.right * (barSizeX - 1) / 2f * tileSize + Vector3.up * 0.4f, Quaternion.identity) as Transform;
				counter.parent = barHolder;
				counter = Instantiate (counterPrefab, Vector3.forward * ((barSizeY - 1) / 2f - y) * tileSize + Vector3.left * (barSizeX - 1) / 2f * tileSize + Vector3.up * 0.4f, Quaternion.identity) as Transform;
				counter.parent = barHolder;
			}
		}

		int barPosX = r.Next (map.mapSize.x - barSizeX + 1);
		int barPosY = r.Next (map.mapSize.y - barSizeY + 1);

		Vector3 origin = Vector3.right * width / 2f + Vector3.forward * depth / 2f;
		Vector3 barSize = Vector3.right * barWidth / 2f + Vector3.forward * barDepth / 2f;
		Vector3 barPos = Vector3.right * barPosX * tileSize + Vector3.forward * barPosY * tileSize;
		barHolder.position = barSize + barPos - origin;

		// Bar area accessible
		bool[, ] accessibleMap = new bool[(int) map.mapSize.x, (int) map.mapSize.y];
		Coord barCentre = new Coord (barPosX + barSizeX / 2, barPosY + barSizeY / 2);
		accessibleMap[barCentre.x, barCentre.y] = true;

		bool[, ] obstacleMap = new bool[(int) map.mapSize.x, (int) map.mapSize.y];
		int obstacleCount = (int) (map.mapSize.x * map.mapSize.y * map.obstaclePercent);
		int currentObstacleCount = 0;

		for (int x = 0; x < barSizeX; x++) {
			for (int y = 0; y < barSizeY; y++) {
				tileCoords.Remove (new Coord (x + barPosX, y + barPosY));

				bool isEdge = x == 0 || x == barSizeX - 1 || y == 0 || y == barSizeY - 1;
				bool hasWall = (!openVertical || x != (int) barSizeX / 2) && (openVertical || y != (int) (barSizeY - 1) / 2); // why barSizeY - 1 ?
				if (isEdge && hasWall) {
					obstacleMap[x + barPosX, y + barPosY] = true;
					currentObstacleCount++;
				} else if (isEdge) { // edge is at door
					if (openVertical) {
						tileCoords.Remove (new Coord (x + barPosX, y + barPosY + 2));
						tileCoords.Remove (new Coord (x + barPosX, y + barPosY + 1));
						tileCoords.Remove (new Coord (x + barPosX, y + barPosY - 1));
						tileCoords.Remove (new Coord (x + barPosX, y + barPosY - 2));
					} else {
						tileCoords.Remove (new Coord (x + barPosX + 2, y + barPosY));
						tileCoords.Remove (new Coord (x + barPosX + 1, y + barPosY));
						tileCoords.Remove (new Coord (x + barPosX - 1, y + barPosY));
						tileCoords.Remove (new Coord (x + barPosX - 2, y + barPosY));
					}
				}
			}
		}

		shuffledCoords = new Queue<Coord> (Utility.ShuffleArray (tileCoords.ToArray (), seed + level));
		List<Coord> openCoords = new List<Coord> (tileCoords);

		// Spawning obstacles
		for (int i = 0; i < obstacleCount; i++) {
			Coord randomCoord = getRandomCoord ();
			obstacleMap[randomCoord.x, randomCoord.y] = true;
			currentObstacleCount++;

			if (MapIsFullyAccessible (obstacleMap, currentObstacleCount, accessibleMap, barCentre)) {
				InstantiateObstacle (r, mapHolder, randomCoord);

				openCoords.Remove (randomCoord);
			} else {
				obstacleMap[randomCoord.x, randomCoord.y] = false;
				currentObstacleCount--;
			}
		}
		shuffledOpenCoords = new Queue<Coord> (Utility.ShuffleArray (openCoords.ToArray (), seed + level));
	}

	private void InstantiateObstacle (System.Random r, Transform mapHolder, Coord randomCoord) {
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
	}

	/** Flood-fill algorithm */
	bool MapIsFullyAccessible (bool[, ] obstacleMap, int currentObstacleCount, bool[, ] accessibleMap, Coord barCentre) {
		bool[, ] mapFlags = new bool[accessibleMap.GetLength (0), accessibleMap.GetLength (1)];
		System.Array.Copy (accessibleMap, mapFlags, mapFlags.GetLength (0) * mapFlags.GetLength (1));
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (barCentre);

		int accessibleTileCount = 1;

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

	[System.Serializable]
	public class Workarea {
		public Transform tool;
		public Vector2 amountRange;
	}
}