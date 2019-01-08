using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterController : MonoBehaviour, IContainable {
	private Item item;

	public virtual bool Place (GameObject gameObject) {
		Item item = gameObject.GetComponent<Item> ();
		if (this.item != null || item == null || item.state != Item.State.Solid)
			return false;

		item.transform.parent = transform;
		Vector3 pos = item.transform.localPosition;
		item.transform.localPosition = Vector3.up * pos.y;
		this.item = item;
		return true;
	}

	public virtual void Remove () { item = null; }
	public virtual List<Item> getItems () { return null; }

}