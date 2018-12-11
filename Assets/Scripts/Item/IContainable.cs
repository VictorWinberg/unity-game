using System.Collections.Generic;
using UnityEngine;

public interface IContainable {
	bool Place (GameObject gameObject);
	void Remove ();
	List<Item> getItems ();
}