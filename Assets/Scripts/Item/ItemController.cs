using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour {

    public Transform itemHold;
    private GameObject item;

    public void Move () {
        if (this.item == null) {
            int pickableLayer = 1 << LayerMask.NameToLayer ("Pickable");
            float maxDist = 2f;
            float minDotDist = Mathf.Infinity;
            GameObject item = null;
            foreach (Collider hit in Physics.OverlapSphere (transform.position, maxDist, pickableLayer)) {
                Vector2 hitPos = new Vector2 (hit.transform.position.x, hit.transform.position.z);
                Vector2 myPos = new Vector2 (transform.position.x, transform.position.z);
                Vector2 forward = new Vector2 (transform.forward.x, transform.forward.z);
                float dist = Vector2.Distance (hitPos, myPos);
                float dot = Vector2.Dot (forward, (hitPos - myPos).normalized);
                if (dist < minDotDist && dot > 0.5f) {
                    minDotDist = dist * dot;
                    item = hit.gameObject;
                }
            }
            if (item != null) {
                PickUp (item);
            }
        } else {
            float maxDist = 2f;
            float minDotDist = Mathf.Infinity;
            GameObject container = null;
            foreach (Collider hit in Physics.OverlapSphere (transform.position, maxDist)) {
                Vector2 hitPos = new Vector2 (hit.transform.position.x, hit.transform.position.z);
                Vector2 myPos = new Vector2 (transform.position.x, transform.position.z);
                Vector2 forward = new Vector2 (transform.forward.x, transform.forward.z);
                float dist = Vector2.Distance (hitPos, myPos);
                float dot = Vector2.Dot (forward, (hitPos - myPos).normalized);
                if (dist * dot < minDotDist && dot > 0.5f && hit.gameObject.tag == "Interactable") {
                    minDotDist = dist * dot;
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
        float maxDist = 2f;
        float minDotDist = Mathf.Infinity;
        GameObject item = null;
        foreach (Collider hit in Physics.OverlapSphere (transform.position, maxDist)) {
            float dist = Vector3.Distance (hit.transform.position, transform.position);
            float dot = Vector3.Dot (transform.forward, (hit.transform.position - transform.position).normalized);
            if (dist * dot < minDotDist && dot > 0.5f && hit.gameObject.tag == "Interactable") {
                minDotDist = dist * dot;
                item = hit.gameObject;
            }
        }
        if (item != null) {
            item.GetComponent<IInteractable> ().Interact ();
        }
    }

    void PickUp (GameObject item) {
        if (item.transform.parent != null)
            item.transform.parent.GetComponent<IContainable> ().Remove ();
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
        if (container.GetComponent<IContainable> ().Place (item)) {
            Rigidbody body = item.GetComponent<Rigidbody> ();
            body.isKinematic = false;
            body.detectCollisions = true;
            item = null;
        }
    }
}