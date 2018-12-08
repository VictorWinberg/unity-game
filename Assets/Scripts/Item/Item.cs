using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour {
    public enum State { Liquid, Solid }
    public State state;
    public bool ready = true;
}