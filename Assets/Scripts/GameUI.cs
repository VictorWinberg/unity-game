using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameUI : MonoBehaviour {

	public Image fadeCanvas;
	public GameObject gameOverUI;

	public RectTransform waveBanner, healthbar;
	public Text waveTitle, waveEnemyCount, scoreUI, gameOverScore, healthbarHp;

	private GameManager manager;

	Spawner spawner;
	Player player;
	
	void Start () {
		manager = FindObjectOfType<GameManager>();
		spawner = FindObjectOfType<Spawner>();
		spawner.OnNewWave += OnNewWave;
		player = FindObjectOfType<Player>();
		player.OnDeath += OnGameOver;
	}

	void Update() {
		scoreUI.text = Scoreboard.score.ToString("D6");
		float healthPercent = 0;
		if (player != null) {
			healthPercent = player.health / player.startingHealth;
			healthbarHp.text = player.health + "/" + player.startingHealth;
		}
		healthbar.localScale = new Vector3 (healthPercent, 1, 1);
	}

	void OnNewWave(int waveNumber) {
		waveTitle.text = "- Wave " + HumanFriendlyInteger.IntegerToWritten (waveNumber) + " -";
		string enemyCount = (spawner.waves [waveNumber - 1].infinite) ? "Infinite" : spawner.waves [waveNumber - 1].enemyCount + "";
		//enemyCount += " | Health: " + (int)(manager.getPlayer ().getHealth ()) + " | Mode: " + manager.getPlayer ().getGun ().fireMode;
		waveEnemyCount.text = "Enemies: " + enemyCount;
		StopCoroutine ("AnimateWaveBanner");
		StartCoroutine ("AnimateWaveBanner");
	}

	IEnumerator AnimateWaveBanner () {
		float delayTime = 1.5f;
		float speed = 3f;
		float animatePercent = 0f;
		int dir = 1;

		float endDelayTime = Time.time + 1 / speed + delayTime;

		while (animatePercent >= 0) {
			animatePercent += Time.deltaTime * speed * dir;

			if(animatePercent >= 1) {
				animatePercent = 1;
				if(Time.time > endDelayTime) {
					dir = -1;
				}
			}

			waveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-150, 150, animatePercent);
			yield return null;
		}
	}
	
	void OnGameOver () {
		Cursor.visible = true;
		StartCoroutine(Fade(Color.clear, new Color(1, 1, 1, .8f), 1));
		gameOverScore.text = scoreUI.text;
		scoreUI.gameObject.SetActive (false);
		healthbar.transform.parent.gameObject.SetActive (false);
		gameOverUI.SetActive (true);
	}

	IEnumerator Fade(Color from, Color to, float time) {
		float speed = 1 / time;
		float percent = 0;

		while (percent < 1) {
			percent += Time.deltaTime * speed;
			fadeCanvas.color = Color.Lerp(from, to, percent);
			yield return null;
		}
	}

	// UI Input
	public void StartNewGame() {
		SceneManager.LoadScene ("Game");
	}

	public void ReturnToMainMenu() {
		SceneManager.LoadScene ("Menu");
	}
}
