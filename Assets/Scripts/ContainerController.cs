using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerController : MonoBehaviour, IInteractable {

	public Transform level;
	private List<Item> items;

	void Start () {
		items = new List<Item> ();
	}

	public virtual void Interact (Item item) {
		items.Add (item);
		if (item.state == Item.State.Liquid) {
			Fill (item);
		} else if (item.state == Item.State.Solid) {
			Place (item);
		}

	}

	void Fill (Item item) {
		Material itemMaterial = new Material (item.GetComponent<Renderer> ().sharedMaterial);
		level.GetComponent<Renderer> ().sharedMaterial = itemMaterial;

		level.localPosition += Vector3.up * 1 / 8;
		level.localScale += Vector3.up * 1 / 4;
		Destroy (item.gameObject);
	}

	void Place (Item item) {
		item.transform.parent = transform;
		Destroy (item.GetComponent<Rigidbody> ());
	}
}