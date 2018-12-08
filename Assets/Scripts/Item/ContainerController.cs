using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerController : MonoBehaviour, IContainable {

	public Transform level;
	private List<Item> items;

	void Start () {
		items = new List<Item> ();
	}

	public virtual bool Place (Item item) {
		if (!item.ready) return false;

		items.Add (item);
		if (item.state == Item.State.Liquid) {
			Fill (item);
		} else if (item.state == Item.State.Solid) {
			Put (item);
		}
		return true;
	}

	public virtual void Remove () { }

	void Fill (Item item) {
		Material itemMaterial = new Material (item.GetComponent<Renderer> ().sharedMaterial);
		level.GetComponent<Renderer> ().sharedMaterial = itemMaterial;

		level.localPosition += Vector3.up * 1 / 8;
		level.localScale += Vector3.up * 1 / 4;
		Destroy (item.gameObject);
	}

	void Put (Item item) {
		item.transform.parent = transform;
		item.gameObject.layer = 0;
		Destroy (item.GetComponent<Rigidbody> ());
	}
}