using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour {
    public string name;
    public enum State { Liquid, Solid }
    public State state;
    public bool ready = true;
    public Item next;

    public Item (string name) {
        this.name = name;
    }
}