using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour {

	public Rigidbody body;
	public float forceMin, forceMax;

	float lifetime = 4;
	float fadetime = 2;

	void Start () {
		float force = Random.Range (forceMin, forceMax);
		body.AddForce (transform.right * force);
		body.AddTorque (Random.insideUnitSphere * force);

		StartCoroutine (Fade ());
	}
	
	IEnumerator Fade() {
		yield return new WaitForSeconds(lifetime);

		float percent = 0;
		float fadeSpeed = 1 / fadetime;
		Material material = GetComponent<Renderer> ().material;
		Color initialColor = material.color;

		while (percent < 1) {
			percent += Time.deltaTime * fadeSpeed;
			material.color = Color.Lerp(initialColor, Color.clear, percent);
			yield return null;
		}

		Destroy (gameObject);
	}
}
