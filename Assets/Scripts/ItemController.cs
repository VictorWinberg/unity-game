using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour {

    public Transform itemHold;
    private GameObject item;

    public void Interact () {
        if (this.item == null) {
            int pickableLayer = 1 << LayerMask.NameToLayer ("Pickable");
            float maxDist = 2f;
            float minDist = Mathf.Infinity;
            GameObject item = null;
            foreach (Collider hit in Physics.OverlapSphere (transform.position, maxDist, pickableLayer)) {
                float dist = Vector3.Distance (hit.transform.position, transform.position);
                float dot = Vector3.Dot (transform.forward, (hit.transform.position - transform.position).normalized);
                if (dist < minDist && dot > 0.5f) {
                    minDist = dist * dot;
                    item = hit.gameObject;
                }
            }
            if (item != null) {
                PickUp (item);
            }
        } else {
            Drop ();
        }
    }

    void PickUp (GameObject item) {
        item.transform.position = itemHold.position;
        item.transform.rotation = itemHold.rotation;
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
}