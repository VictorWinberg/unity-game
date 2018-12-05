using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {

	public enum FireMode {Auto, Burst, Single};
	public FireMode fireMode;

	public Transform[] projectileSpawns;
	public Projectile projectile;
	public float msBetweenShots = 100;
	public float muzzleVelocity = 35;
	public int burstCount = 3;
	public int magSize = 60;
	public float reloadTime = .3f;

	[Header("Recoil")]
	public Vector2 kickbackMinMax = new Vector2(.05f, .2f);
	public Vector2 recoilAngleMinMax = new Vector2(3,5);
	public float recoilRecoverTime = .1f;

	[Header("Effects")]
	public Transform shell;
	public Transform shellEjection;
	public AudioClip shootAudio, reloadAudio;
	MuzzleFlash muzzleflash;

	float nextShotTime;

	bool triggerReleased;
	int burstShotsRemaining;
	int bulletsRemainingInMag;
	bool reloading;

	Vector3 recoilSmoothDampVelocity;
	float recoilRotationSmoothDampVelocity;
	float recoilAngle;

	void Start() {
		muzzleflash = GetComponent<MuzzleFlash> ();
		burstShotsRemaining = burstCount;
		bulletsRemainingInMag = magSize;
		reloading = false;
	}

	void LateUpdate() {
		// Animate the recoil
		transform.localPosition = Vector3.SmoothDamp (transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilRecoverTime);
		recoilAngle = Mathf.SmoothDamp (recoilAngle, 0, ref recoilRotationSmoothDampVelocity, recoilRecoverTime);
		transform.localEulerAngles += Vector3.left * recoilAngle;

		if (!reloading && bulletsRemainingInMag == 0)
			Reload ();
	}

	void Shoot() {
		if(!reloading && Time.time > nextShotTime && bulletsRemainingInMag > 0) {
			if(fireMode == FireMode.Burst) {
				if(burstShotsRemaining == 0)
					return;

				burstShotsRemaining--;
			} else if(fireMode == FireMode.Single) {
				if(!triggerReleased)
					return;
			}

			for (int i = 0; i < projectileSpawns.Length; i++) {
				if(bulletsRemainingInMag == 0)
					break;
				bulletsRemainingInMag--;
				nextShotTime = Time.time + msBetweenShots / 1000;
				Projectile newProjectile = Instantiate (projectile, projectileSpawns[i].position, projectileSpawns[i].rotation) as Projectile;
				newProjectile.SetSpeed (muzzleVelocity);
			}

			Instantiate(shell, shellEjection.position, shellEjection.rotation);
			muzzleflash.Activate();
			transform.localPosition -= Vector3.forward * Random.Range(kickbackMinMax.x, kickbackMinMax.y);
			recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
			recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

			AudioManager.instance.PlaySound (shootAudio, transform.position); 
		}
	}

	public void Reload() {
		if (!reloading && bulletsRemainingInMag != magSize) {
			StartCoroutine (AnimateReload());
			AudioManager.instance.PlaySound (reloadAudio, transform.position); 
		}
	}

	IEnumerator AnimateReload() {
		reloading = true;
		yield return new WaitForSeconds (.2f);

		float reloadSpeed = 1f / reloadTime;
		float percent = 0;
		Vector3 initialRotation = transform.localEulerAngles;
		float maxReloadAngle = 60;

		while (percent < 1) {
			percent += Time.deltaTime * reloadSpeed;
			float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
			float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
			transform.localEulerAngles = initialRotation + Vector3.left * reloadAngle;

			yield return null;
		}

		reloading = false;
		bulletsRemainingInMag = magSize;
	}

	public void OnTriggerHold() {
		Shoot ();
		triggerReleased = false;
	}

	public void OnTriggerRelease() {
		triggerReleased = true;
		burstShotsRemaining = burstCount;
	}

	public void Aim (Vector3 aimPoint){
		if(!reloading)
			transform.LookAt (aimPoint);
	}
}
