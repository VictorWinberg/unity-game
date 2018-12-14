using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour {

	private int starCount;
	public int StarCount {
		get { return starCount; }
		set {
			starCount = value;
			UpdateStars ();
		}
	}
	public GameObject[] stars;

	void UpdateStars () {
		for (int i = 0; i < stars.Length; i++) {
			stars[i].SetActive (i < starCount);
		}
	}
}