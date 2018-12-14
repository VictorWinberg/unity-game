using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour {
	public SceneFader fader;
	public Transform content;

	void Awake () {
		Instantiate (Resources.Load ("AudioManager"), Vector3.zero, Quaternion.identity);
	}

	void Start () {
		int[] levels = PlayerPrefsX.GetIntArray ("Levels", 0, 1);

		for (int i = 0; i < levels.Length + 1; i++) {
			int level = i + 1;
			GameObject go = (GameObject) Instantiate (Resources.Load ("LevelButton"), Vector3.zero, Quaternion.identity);
			go.transform.SetParent (content);
			go.transform.GetChild (0).GetComponent<Text> ().text = level.ToString ();

			Button levelButton = go.GetComponent<Button> ();
			if (i < levels.Length) {
				levelButton.onClick.AddListener (() => Select (level));
				go.GetComponent<StarController> ().StarCount = levels[i];
			} else {
				levelButton.interactable = false;
			}
		}
	}

	public void Select (int level) {
		Debug.Log ("Level " + level);
		fader.FadeTo ("Game");
	}
}