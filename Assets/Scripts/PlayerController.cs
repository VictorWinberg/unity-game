using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour {

	Vector3 velocity;
	Rigidbody body;

	void Start () {
		body = GetComponent<Rigidbody> ();
	}

	void FixedUpdate () {
		body.MovePosition (body.position + velocity * Time.fixedDeltaTime);
		
	}

	public void Move(Vector3 velocity) {
		this.velocity = velocity;
	}

	public void LookAt (Vector3 lookPoint) {
		Vector3 heightCorrectedPoint = new Vector3 (lookPoint.x, transform.position.y, lookPoint.z);
		transform.LookAt (heightCorrectedPoint);
	}
}
