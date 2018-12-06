using UnityEngine;
using System.Collections;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]
public class Player : LivingEntity {

	public float moveSpeed = 5;

	protected Camera viewCamera;
	PlayerController controller;
	GunController gunController;

	public string horizontal { get; set;}
	public string vertical { get; set;}

	public string fire1 { get; set;}
	public string fire2 { get; set;}

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

		// Weapon input
		if (Input.GetButton(fire1)) {
			if (aimbot) {
				// Look input
				int enemyLayer = 1 << LayerMask.NameToLayer ("Enemy");
				float minDist = Mathf.Infinity;
				Vector3 closest = transform.position + moveVelocity;
				foreach (Collider hit in Physics.OverlapSphere (transform.position, 10f, enemyLayer)) {
					float dist = Vector3.Distance (hit.transform.position, transform.position);
					if (dist < minDist) {
						minDist = dist;
						closest = hit.transform.position;
					}
				}
				controller.Move (Vector3.ClampMagnitude(closest - transform.position, 0.01f));
			}
			gunController.OnTriggerHold();
		}
		if (Input.GetButtonUp(fire1)) {
			gunController.OnTriggerRelease();
		}
		if (Input.GetButtonDown(fire2)) {
			gunController.Reload();
		}
	}

	void OnNewWave(int waveNumber) {
		if (waveNumber != 1) startingHealth = (int)(startingHealth * 1.2f);
		health = startingHealth;
		gunController.EquipGun (waveNumber - 1);
	}

	public Gun getGun() {
		return gunController.getGun ();
	}

	public Gun getGunWithIndex(int gunIndex) {
		return gunController.getGunWithIndex (gunIndex);
	}

	public override void Die () {
		AudioManager.instance.PlaySound ("Player Death", transform.position);
		base.Die ();
	}
}
