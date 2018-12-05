using System.Collections;

public static class Utility {

	/** Fisher-Yates shuffle method to randomize the positions */
	public static T[] ShuffleArray<T>(T[] array, int seed) {
		System.Random r = new System.Random (seed);

		for (int i = 0; i < array.Length - 1; i++) {
			int randomIndex = r.Next(i, array.Length);
			T temp = array[randomIndex];
			array[randomIndex] = array[i];
			array[i] = temp;
		}

		return array;
	}
}
