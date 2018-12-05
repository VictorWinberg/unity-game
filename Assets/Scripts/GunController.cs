using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour {

	public Transform weaponHold;
	public Gun[] guns;
	private Gun gun;
	private int gunIndex;

	public void EquipGun(Gun gunToEquip) {
		if (gun != null) {
			Destroy(gun.gameObject);
		}
		gun = (Gun)Instantiate (gunToEquip, weaponHold.position, weaponHold.rotation);
		gun.transform.parent = weaponHold;
	}

	public void EquipGun(int gunIndex) {
		this.gunIndex = gunIndex;
		EquipGun (guns [gunIndex % guns.Length]);
	}

	public void OnTriggerHold (){
		if (gun != null) {
			gun.OnTriggerHold ();
		}
	}

	public void OnTriggerRelease() {
		if (gun != null) {
			gun.OnTriggerRelease ();
		}
	}

	public float GunHeight {
		get {
			return weaponHold.position.y;
		}
	}

	public void Aim(Vector3 aimPoint) {
		if (gun != null) {
			gun.Aim (aimPoint);
		}
	}

	public void Reload() {
		if (gun != null) {
			gun.Reload ();
		}
	}
	
	public Gun getGun() {
		return guns[gunIndex % guns.Length];
	}

	public Gun getGunWithIndex(int gunIndex) {
		return guns[gunIndex % guns.Length];
	}
}
