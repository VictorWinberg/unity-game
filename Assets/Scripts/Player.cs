using System.Collections;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]
public class Player : LivingEntity {

	public float moveSpeed = 5;

	protected Camera viewCamera;
	PlayerController controller;
	GunController gunController;

	public string horizontal { get; set; }
	public string vertical { get; set; }

	public string fire1 { get; set; }
	public string fire2 { get; set; }

	private GameObject item;
	public bool aimbot = false;

	void Start () {
		controller = GetComponent<PlayerController> ();
		gunController = GetComponent<GunController> ();
		viewCamera = Camera.main;
		FindObjectOfType<Spawner> ().OnNewWave += OnNewWave;
	}

	void Update () {
		// Movement input
		Vector3 moveInput = new Vector3 (Input.GetAxisRaw (horizontal), 0, Input.GetAxisRaw (vertical));
		Vector3 moveVelocity = moveInput.normalized * moveSpeed;
		controller.Move (moveVelocity);

		// Interaction input
		if (Input.GetButton (fire1)) {
			if (this.item == null) {
				int pickableLayer = 1 << LayerMask.NameToLayer ("Pickable");
				float maxDist = 2f;
				float minDist = Mathf.Infinity;
				GameObject item = null;
				foreach (Collider hit in Physics.OverlapSphere (transform.position, maxDist, pickableLayer)) {
					float dist = Vector3.Distance (hit.transform.position, transform.position);
					float dot = Vector3.Dot (transform.forward, (hit.transform.position - transform.position).normalized);
					if (dist < minDist && dot > 0.7f) {
						minDist = dist;
						item = hit.gameObject;
					}
				}
				if (item != null) {
					PickUp (item);
				}
			} else {
				Drop ();
			}
		}
		if (Input.GetButtonUp (fire1)) {
			// gunController.OnTriggerRelease ();
		}
		if (Input.GetButtonDown (fire2)) {
			// gunController.Reload ();
		}
	}

	void OnNewWave (int waveNumber) {
		if (waveNumber != 1) startingHealth = (int) (startingHealth * 1.2f);
		health = startingHealth;
		gunController.EquipGun (waveNumber - 1);
	}

	void PickUp (GameObject item) {
		item.transform.parent = transform;
		item.GetComponent<Rigidbody> ().isKinematic = true;
		this.item = item;
	}

	void Drop () {
		item.transform.parent = null;
		item.GetComponent<Rigidbody> ().isKinematic = false;
		this.item = null;
	}

	public Gun getGun () {
		return gunController.getGun ();
	}

	public Gun getGunWithIndex (int gunIndex) {
		return gunController.getGunWithIndex (gunIndex);
	}

	public override void Die () {
		AudioManager.instance.PlaySound ("Player Death", transform.position);
		base.Die ();
	}
}