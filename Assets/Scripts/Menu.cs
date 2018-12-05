using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

	public GameObject mainMenuHolder, optionsMenuHolder;

	public Slider[] volumeSliders;
	public Toggle[] resolutionToggles;
	public Toggle fullscreenToggle;
	public int[] screenWidths;

	private int resolutionIndex;

	void Awake() {
		GameObject go = Instantiate (Resources.Load ("AudioManager"), Vector3.zero, Quaternion.identity) as GameObject;

		map = MapGenerator.Create ();
		map.GenerateMap ();
	}

	MapGenerator map;
	float starttime = 3f;
	float timer = 0;

	void Update() {
		map.transform.RotateAround (Vector3.zero, Vector3.up, .3f);

		timer += Time.deltaTime;
		if (timer > starttime) {
			timer = 0;
			map.mapIndex++;
			map.transform.rotation = Quaternion.identity;
			map.GenerateMap ();
		}

	}

	void Start() {
		resolutionIndex = PlayerPrefs.GetInt ("Resolution index");
		bool isFullscreen = (PlayerPrefs.GetInt ("Fullscreen") == 1) ? true : false;

		volumeSliders [0].value = AudioManager.instance.masterVolume;
		volumeSliders [1].value = AudioManager.instance.musicVolume;
		volumeSliders [2].value = AudioManager.instance.sfxVolume;

		for (int i = 0; i < resolutionToggles.Length; i++) {
			resolutionToggles [i].isOn = i == resolutionIndex;
		}

		fullscreenToggle.isOn = isFullscreen;
	}

	public void Play() {
		SceneManager.LoadScene ("Game");
	}
	
	public void MainMenu() {
		mainMenuHolder.SetActive (true);
		optionsMenuHolder.SetActive (false);
	}

	public void OptionsMenu() {
		mainMenuHolder.SetActive (false);
		optionsMenuHolder.SetActive (true);
	}

	public void SetMasterVolume(float volume) {
		AudioManager.instance.SetVolume (volume, AudioManager.AudioChannel.Master);
	}

	public void SetMusicVolume(float volume) {
		AudioManager.instance.SetVolume (volume, AudioManager.AudioChannel.Music);
	}

	public void SetSfxVolume(float volume) {
		AudioManager.instance.SetVolume (volume, AudioManager.AudioChannel.SFX);
	}

	public void SetScreenResolution(int i) {
		if (resolutionToggles [i].isOn) {
			resolutionIndex = i;
			float aspectRatio = 16 / 9f;
			Screen.SetResolution(screenWidths[i], (int)(screenWidths[i]/aspectRatio), false);
			PlayerPrefs.SetInt ("Resolution index", resolutionIndex);
			PlayerPrefs.Save ();
		}
	}

	public void SetFullscreen(bool isFullscreen) {
		for (int i = 0; i < resolutionToggles.Length; i++) {
			resolutionToggles [i].interactable = !isFullscreen;
		}

		if (isFullscreen) {
			Resolution[] allResolutions = Screen.resolutions;
			Resolution maxResolution = allResolutions [allResolutions.Length - 1];
			Screen.SetResolution (maxResolution.width, maxResolution.height, true);
		} else {
			SetScreenResolution (resolutionIndex);
		}

		PlayerPrefs.SetInt ("Fullscreen", isFullscreen ? 1 : 0);
		PlayerPrefs.Save ();
	}

	public void Quit() {
		Application.Quit ();
	}
}
