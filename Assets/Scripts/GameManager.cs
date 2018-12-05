using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static int waves = 30;

	private GameObject camera;
	private MapGenerator map;
	private Scoreboard scoreboard;
	private Crosshairs crosshairs;
	private Player player;
	private Enemy enemy;
	private Spawner spawner;
	private GameUI canvas;

	void Awake () {
		player = ((GameObject)Instantiate(Resources.Load ("Player"), Vector3.zero, Quaternion.identity)).GetComponent<Player>();
		player.crosshairs = FindObjectOfType<Crosshairs>();
		enemy = ((GameObject)Resources.Load ("Enemy")).GetComponent<Enemy> ();
		spawner = Spawner.Create ();
		map = MapGenerator.Create();
		map.GenerateMap ();
		GameObject audioManager = Instantiate (Resources.Load ("AudioManager"), Vector3.zero, Quaternion.identity) as GameObject;
		audioManager.GetComponent<AudioManager> ().SetPlayer (player.gameObject);
	}

	void Start () {
		player.aimbot = false;
		//player.startingHealth = 100000;
	}
	
	void Update () {

	}

	public Player getPlayer() {
		return player;
	}
}
