using System.Collections;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour {

	Vector3 velocity;
	Rigidbody body;

	void Start () {
		body = GetComponent<Rigidbody> ();
	}

	void FixedUpdate () {
		if (velocity != Vector3.zero) {
			body.MovePosition (body.position + velocity * Time.fixedDeltaTime);
			transform.rotation = Quaternion.Slerp (
				transform.rotation,
				Quaternion.LookRotation (velocity),
				20f * Time.deltaTime
			);
		}
	}

	public void Move (Vector3 velocity) {
		this.velocity = velocity;
	}
}