using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {
    public int level;

    public static Level instance;

    void Awake () {
        if (instance != null) {
			Destroy (gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad (gameObject);
    }

    public void Complete () {
        List<int> levels = new List<int>(PlayerPrefsX.GetIntArray ("Levels"));
        if(level > levels.Count - 1) {
            levels.Add(1);
        } else {
            levels[level] += 1;
        }
        PlayerPrefsX.SetIntArray ("Levels", levels.ToArray());
        SceneFader.instance.FadeTo ("LevelSelect");
    }
}