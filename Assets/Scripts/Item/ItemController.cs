using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour {

    public Transform itemHold;
    private GameObject item;

    public void Move () {
        if (this.item == null) {
            int pickableLayer = 1 << LayerMask.NameToLayer ("Pickable");
            float maxDist = 2f;
            float minDist = Mathf.Infinity;
            GameObject item = null;
            foreach (Collider hit in Physics.OverlapSphere (transform.position, maxDist, pickableLayer)) {
                float dist = Vector3.Distance (hit.transform.position, transform.position);
                float dot = Vector3.Dot (transform.forward, (hit.transform.position - transform.position).normalized);
                if (dist < minDist && dot > 0.5f && hit.GetComponent<Rigidbody> () != null) {
                    minDist = dist * dot;
                    item = hit.gameObject;
                }
            }
            if (item != null) {
                PickUp (item);
            }
        } else {
            float maxDist = 2f;
            float minDist = Mathf.Infinity;
            GameObject container = null;
            foreach (Collider hit in Physics.OverlapSphere (transform.position, maxDist)) {
                float dist = Vector3.Distance (hit.transform.position, transform.position);
                float dot = Vector3.Dot (transform.forward, (hit.transform.position - transform.position).normalized);
                if (dist < minDist && dot > 0.5f && hit.gameObject.tag == "Interactable") {
                    minDist = dist * dot;
                    container = hit.gameObject;
                }
            }
            if (container == null) {
                Drop ();
            } else {
                DropInto (container);
            }
        }
    }

    public void Interact () {

    }

    void PickUp (GameObject item) {
        item.transform.position = itemHold.position;
        item.transform.parent = transform;
        Rigidbody body = item.GetComponent<Rigidbody> ();
        body.isKinematic = true;
        body.detectCollisions = false;
        this.item = item;
    }

    void Drop () {
        item.transform.parent = null;
        Rigidbody body = item.GetComponent<Rigidbody> ();
        body.isKinematic = false;
        body.detectCollisions = true;
        item = null;
    }

    void DropInto (GameObject container) {
        if (item.GetComponent<Item> () == null) {
            Drop ();
            return;
        }
        item.transform.parent = null;
        if (container.GetComponent<IContainable> ().Place (item.GetComponent<Item> ())) {
            Rigidbody body = item.GetComponent<Rigidbody> ();
            body.isKinematic = false;
            body.detectCollisions = true;
        }
        item = null;
    }
}