using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour {

	public GameObject holder;
	public Sprite[] sprites;
	public SpriteRenderer[] spriteRenderers;

	public float flashTime;

	void Start() {
		Deactivate ();
	}

	public void Activate() {
		holder.SetActive (true);

		int spriteIndex = Random.Range (0, sprites.Length);
		for (int i = 0; i < spriteRenderers.Length; i++) {
			spriteRenderers[i].sprite = sprites[spriteIndex];
		}

		Invoke ("Deactivate", flashTime);
	}

	public void Deactivate() {
		holder.SetActive (false);
	}
}
