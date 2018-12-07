using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static int waves = 30;
	private Player p1, p2;
	private Enemy enemy;
	private Spawner spawner;

	void Awake () {
		FindObjectOfType<MapGenerator> ().GenerateMap ();
		p1 = ((GameObject) Instantiate (Resources.Load ("Player"), Vector3.zero, Quaternion.identity)).GetComponent<Player> ();
		p1.horizontal = "Horizontal_P1";
		p1.vertical = "Vertical_P1";
		p1.fire1 = "Fire1_P1";
		p1.fire2 = "Fire2_P1";
		p2 = ((GameObject) Instantiate (Resources.Load ("Player"), Vector3.forward, Quaternion.identity)).GetComponent<Player> ();
		p2.horizontal = "Horizontal_P2";
		p2.vertical = "Vertical_P2";
		p2.fire1 = "Fire1_P2";
		p2.fire2 = "Fire2_P2";
		enemy = ((GameObject) Resources.Load ("Enemy")).GetComponent<Enemy> ();
		spawner = Spawner.Create ();
		GameObject audioManager = Instantiate (Resources.Load ("AudioManager"), Vector3.zero, Quaternion.identity) as GameObject;
		audioManager.GetComponent<AudioManager> ().SetPlayer (p1.gameObject);
	}

	void Start () {
		//p1.startingHealth = 100000;
	}

	void Update () {

	}

	public Player getPlayer1 () {
		return p1;
	}

	public Player getPlayer2 () {
		return p2;
	}
}