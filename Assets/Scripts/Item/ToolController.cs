using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour, IInteractable, IContainable {

    public Item.State from, to;
    private Item item;

    public virtual bool Place (GameObject gameObject) {
        Item item = gameObject.GetComponent<Item> ();
        if (this.item != null || item == null || item.state != from) return false;

        item.transform.parent = transform;
        this.item = item;
        return true;
    }

    public virtual void Remove () { item = null; }
    public virtual List<Item> getItems () { return null; }

    public virtual void Interact () {
        if (item == null) return;

        item.ready = true;
        if (item.next != null && item.next.state == to) {
            Instantiate (item.next, item.transform.position, item.transform.rotation);
            Destroy (item.gameObject);
        }
        this.item = null;
    }
}