using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour, IInteractable, IContainable {

    private Item item;

    public virtual bool Place (Item item) {
        if (this.item != null) return false;

        item.transform.parent = transform;
        this.item = item;
        return true;
    }

    public virtual void Remove () {
        item = null;
    }

    public virtual void Interact () {
        if (item == null) return;

        item.ready = true;
        this.item = null;
    }
}