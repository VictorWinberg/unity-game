using System.Collections;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]
[RequireComponent (typeof (ItemController))]
public class Player : LivingEntity {

	public float moveSpeed = 5;

	protected Camera viewCamera;
	PlayerController controller;
	GunController gunController;
	ItemController itemController;

	public string horizontal { get; set; }
	public string vertical { get; set; }

	public string fire1 { get; set; }
	public string fire2 { get; set; }

	private GameObject item;
	public bool aimbot = false;

	void Start () {
		controller = GetComponent<PlayerController> ();
		gunController = GetComponent<GunController> ();
		itemController = GetComponent<ItemController> ();
		viewCamera = Camera.main;
		FindObjectOfType<Spawner> ().OnNewWave += OnNewWave;
	}

	void Update () {
		// Movement input
		Vector3 moveInput = new Vector3 (Input.GetAxisRaw (horizontal), 0, Input.GetAxisRaw (vertical));
		Vector3 moveVelocity = moveInput.normalized * moveSpeed;
		controller.Move (moveVelocity);

		// Interaction input
		if (Input.GetButtonDown (fire1)) {
			itemController.Interact ();
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