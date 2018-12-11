using System.Collections;
using UnityEngine;

public class Scoreboard : MonoBehaviour {

	public static int score { get; private set; }
	float lastKillTime;
	int killStreak;
	float killStreakExpiry = 1f;

	// Use this for initialization
	void Start () {
		Customer.OnServedCorrect += OnCustomerServedCorrect;
		Customer.OnServedIncorrect += OnCustomerServedIncorrect;
		FindObjectOfType<Player> ().OnDeath += OnPlayerDeath;
	}

	void OnCustomerServedCorrect () {
		if (Time.time < lastKillTime + killStreakExpiry) {
			killStreak++;
		} else {
			killStreak = 0;
		}

		lastKillTime = Time.time;

		score += 5 + 2 * killStreak;
	}

	void OnCustomerServedIncorrect () {
		score -= 5 + 2 * killStreak;
	}

	void OnPlayerDeath () {
		Customer.OnServedCorrect -= OnCustomerServedCorrect;
		Customer.OnServedIncorrect -= OnCustomerServedIncorrect;
	}
}