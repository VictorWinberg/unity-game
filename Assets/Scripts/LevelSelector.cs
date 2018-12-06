using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour {
	public SceneFader fader;
	public Transform content;

	void Start () {
		int levelReached = PlayerPrefs.GetInt ("levelReached", 1);
		int levels = levelReached + 2;

		for (int i = 0; i < levels; i++) {
			int level = i + 1;
			GameObject go = (GameObject) Instantiate (Resources.Load ("LevelButton"), Vector3.zero, Quaternion.identity);
			go.transform.SetParent (content);
			go.transform.GetChild (0).GetComponent<Text> ().text = level.ToString ();

			Button levelButton = go.GetComponent<Button> ();
			levelButton.onClick.AddListener (() => Select (level));
			if (i + 1 > levelReached) {
				levelButton.interactable = false;
			}
		}
	}

	public void Select (int level) {
		Debug.Log ("Level " + level);
		fader.FadeTo ("Game");
	}
}