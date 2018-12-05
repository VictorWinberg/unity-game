using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour {

	public AudioClip menuTheme, mainTheme, weirdTheme;
	private string sceneName;

	// Use this for initialization
	void Start () {
		SceneManager.sceneLoaded += SceneLoaded;
		SceneLoaded (SceneManager.GetActiveScene (), LoadSceneMode.Additive);
	}
	
	void SceneLoaded(Scene scene, LoadSceneMode m) {
		string newSceneName = scene.name;
		if (newSceneName != sceneName) {
			sceneName = newSceneName;
			Invoke ("PlayMusic", .2f);
		}
	}

	void PlayMusic() {
		AudioClip clipToPlay = null;

		switch (sceneName) {
		case "Menu":
			clipToPlay = menuTheme;
			break;
		case "Game":
			clipToPlay = mainTheme;
			break;
		default:
			clipToPlay = weirdTheme;
			break;
		}
		if (clipToPlay != null) {
			AudioManager.instance.PlayMusic (clipToPlay, 2);
			Invoke ("PlayMusic", clipToPlay.length - 10f);
		}
	}
}
