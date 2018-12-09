using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerController : MonoBehaviour, IContainable, IInteractable {

	public Transform level;
	private List<Item> items;
	private bool hasFluid = false;

	void Start () {
		items = new List<Item> ();
	}

	public virtual bool Place (Item item) {
		if (!item.ready) return false;

		if (item.state == Item.State.Liquid) {
			Fill (item);
		} else if (item.state == Item.State.Solid) {
			Put (item);
		}
		items.Add (item);
		return true;
	}

	public virtual void Remove () { }

	public virtual void Interact () {
		StartCoroutine (Shake ());
	}

	void Fill (Item item) {
		Material contentMaterial = new Material (level.GetComponent<Renderer> ().sharedMaterial);
		Material itemMaterial = new Material (item.GetComponent<Renderer> ().sharedMaterial);

		if (hasFluid) {
			contentMaterial.color = Color.Lerp (contentMaterial.color, itemMaterial.color, 0.5f);
		} else {
			contentMaterial = itemMaterial;
			hasFluid = true;
		}

		level.GetComponent<Renderer> ().sharedMaterial = contentMaterial;

		level.localPosition += Vector3.up * 1 / 8;
		level.localScale += Vector3.up * 1 / 4;
		Destroy (item.gameObject);
	}

	void Put (Item item) {
		item.transform.parent = transform;
		item.gameObject.layer = 0;
		Destroy (item.GetComponent<Rigidbody> ());
	}

	IEnumerator Shake () {
		yield return new WaitForSeconds (.2f);

		float percent = 0;
		Vector3 initialRotation = transform.localEulerAngles;
		float maxAngle = 30;

		while (percent < 1) {
			percent += Time.deltaTime * 1f / .3f;
			float interpolation = (-Mathf.Pow (percent, 2) + percent) * 4;
			float angle = Mathf.Lerp (0, maxAngle, interpolation);
			transform.localEulerAngles = initialRotation + Vector3.left * angle;

			yield return null;
		}
	}
}