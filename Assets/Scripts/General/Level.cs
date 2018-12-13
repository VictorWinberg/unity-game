using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {
    public int level { get; private set; }
    public SceneFader sceneFader;

    void Awake () {
        level = PlayerPrefs.GetInt ("levelReached", 1);
    }

    public void Complete () {
        PlayerPrefs.SetInt ("levelReached", level + 1);
        PlayerPrefs.Save ();
        sceneFader.FadeTo ("LevelSelect");
    }
}