using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteLevel : MonoBehaviour {

    public SceneFader sceneFader;

    public void Complete () {
        PlayerPrefs.SetInt ("levelReached", 1);
        sceneFader.FadeTo ("LevelSelect");
    }
}