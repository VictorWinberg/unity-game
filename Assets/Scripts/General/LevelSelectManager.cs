using UnityEngine;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour {
	public Transform content;

	void Awake () {
		Instantiate (Resources.Load ("AudioManager"), Vector3.zero, Quaternion.identity);
		Instantiate (Resources.Load ("Level"), Vector3.zero, Quaternion.identity);
		Instantiate (Resources.Load ("SceneFader"), Vector3.zero, Quaternion.identity);
	}

	void Start () {
		int[] levels = PlayerPrefsX.GetIntArray ("Levels");

		for (int i = 0; i < levels.Length + 2; i++) {
			GameObject go = (GameObject) Instantiate (Resources.Load ("LevelButton"), Vector3.zero, Quaternion.identity);
			go.transform.SetParent (content);
			go.transform.GetChild (0).GetComponent<Text> ().text = (i + 1).ToString ();

			Button levelButton = go.GetComponent<Button> ();
			if (i < levels.Length + 1) {
				int level = i;
				levelButton.onClick.AddListener (() => Select (level));
				go.GetComponent<StarController> ().StarCount = (i < levels.Length) ? levels[i] : 0;
			} else {
				levelButton.interactable = false;
			}
		}
	}

	public void Select (int level) {
		Level.instance.level = level;
		SceneFader.instance.FadeTo ("Game");
	}
}