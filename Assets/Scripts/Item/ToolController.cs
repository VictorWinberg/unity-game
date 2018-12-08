using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour, IInteractable, IContainable {

    private Item item;

    public virtual bool Place (Item item) {
        if (this.item != null) return false;
        item.transform.parent = transform;
        Destroy (item.GetComponent<Rigidbody> ());
        this.item = item;
        return true;
    }

    public virtual void Interact () {
        item.ready = true;
        item.gameObject.AddComponent<Rigidbody> ();
        this.item = null;
    }
}