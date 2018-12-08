using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerController : MonoBehaviour, IInteractable {

	private List<GameObject> items;

	void Start () {
		items = new List<GameObject> ();
	}

	public virtual void Interact (GameObject item) {
		items.Add (item);
		item.transform.parent = transform;
		Destroy (item.GetComponent<Rigidbody> ());
	}
}