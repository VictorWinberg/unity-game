using System.Collections;
using UnityEngine;

public static class Utility {

	/** Fisher-Yates shuffle method to randomize the positions */
	public static T[] ShuffleArray<T> (T[] array, int seed) {
		System.Random r = new System.Random (seed);

		for (int i = 0; i < array.Length - 1; i++) {
			int randomIndex = r.Next (i, array.Length);
			T temp = array[randomIndex];
			array[randomIndex] = array[i];
			array[i] = temp;
		}

		return array;
	}

	public static DistDot DistDotXZ (Collider hit, Transform transform) {
		Vector2 hitPos = new Vector2 (hit.transform.position.x, hit.transform.position.z);
		Vector2 myPos = new Vector2 (transform.position.x, transform.position.z);
		Vector2 forward = new Vector2 (transform.forward.x, transform.forward.z);
		float dist = Vector2.Distance (hitPos, myPos);
		float dot = Vector2.Dot (forward, (hitPos - myPos).normalized);
		return new DistDot (dist, dot);
	}

	public class DistDot {
		public float dist { get; private set; }
		public float dot { get; private set; }
		public float distDot { get; private set; }
		public DistDot (float dist, float dot) {
			this.dist = dist;
			this.dot = dot;
			this.distDot = dist / dot;
		}
	}
}