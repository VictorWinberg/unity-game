using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour {

    public Transform itemHold;
    private GameObject item;

    public void Move () {
        float maxDist = 2f;
        float minDotDist = Mathf.Infinity;

        if (this.item == null) {
            int pickableLayer = 1 << LayerMask.NameToLayer ("Pickable");
            GameObject item = null;
            foreach (Collider hit in Physics.OverlapSphere (transform.position, maxDist, pickableLayer)) {
                Utility.DistDot dd = Utility.DistDotXZ (hit, transform);
                if (dd.distDot < minDotDist && dd.dot > 0.5f) {
                    minDotDist = dd.distDot;
                    item = hit.gameObject;
                }
            }
            if (item != null) {
                PickUp (item);
            }
        } else {
            GameObject container = null;
            foreach (Collider hit in Physics.OverlapSphere (transform.position, maxDist)) {
                Utility.DistDot dd = Utility.DistDotXZ (hit, transform);
                if (dd.distDot < minDotDist && dd.dot > 0.5f && hit.gameObject.tag == "Interactable") {
                    minDotDist = dd.distDot;
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
            Utility.DistDot dd = Utility.DistDotXZ (hit, transform);
            if (dd.distDot < minDotDist && dd.dot > 0.5f && hit.gameObject.tag == "Interactable") {
                minDotDist = dd.distDot;
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