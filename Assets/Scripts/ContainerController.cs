using System.Collections;
using UnityEngine;

public class ContainerController : MonoBehaviour, IInteractable {

	public virtual void Interact (GameObject item) {
		item.transform.parent = transform;
		Destroy (item.GetComponent<Rigidbody> ());
	}
}