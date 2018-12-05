using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SoundLibrary))]
public class AudioManager : MonoBehaviour {

	public enum AudioChannel {Master, SFX, Music};

	public float masterVolume { get; private set; }
	public float sfxVolume { get; private set; }
	public float musicVolume { get; private set; }

	AudioSource sfxSource2D;
	AudioSource[] musicSources;
	int musicSourceIndex;

	GameObject listener, player;

	SoundLibrary soundLibrary;

	public static AudioManager instance;

	void Awake() {
		if (instance != null) {
			Destroy (gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad (gameObject);

		listener = transform.GetComponentInChildren<AudioListener> ().gameObject;
		soundLibrary = transform.GetComponent<SoundLibrary> ();

		musicSources = new AudioSource[2];
		for (int i = 0; i < musicSources.Length; i++) {
			GameObject newMusicSource = new GameObject ("Music source " + (i + 1));
			musicSources [i] = newMusicSource.AddComponent<AudioSource> ();
			//musicSources [i].loop = true;
			newMusicSource.transform.parent = transform;
		}

		GameObject newSource2D = new GameObject ("SFX source 2D");
		sfxSource2D = newSource2D.AddComponent<AudioSource> ();
		newSource2D.transform.parent = transform;

		masterVolume = PlayerPrefs.GetFloat ("MasterVolume", 1);
		musicVolume = PlayerPrefs.GetFloat ("MusicVolume", 1);
		sfxVolume = PlayerPrefs.GetFloat ("SFXVolume", 1);
	}

	public void SetPlayer(GameObject player) {
		this.player = player;
	}

	void Update() {
		if (player != null) {
			listener.transform.position = player.transform.position;
		}
	}

	public void SetVolume(float volume, AudioChannel channel) {
		switch (channel) {
		case AudioChannel.Master:
			masterVolume = volume;
			break;
		case AudioChannel.Music:
			musicVolume = volume;
			break;
		case AudioChannel.SFX:
			sfxVolume = volume;
			break;
		}

		musicSources [0].volume = musicVolume * masterVolume;
		//musicSources [1].volume = musicVolume * masterVolume;

		PlayerPrefs.SetFloat ("MasterVolume", masterVolume);
		PlayerPrefs.SetFloat ("MusicVolume", musicVolume);
		PlayerPrefs.SetFloat ("SFXVolume", sfxVolume);
		PlayerPrefs.Save ();
	}

	public void PlayMusic(AudioClip clip, float fadeDuration = 1) {
		musicSourceIndex = 1 - musicSourceIndex;
		musicSources [musicSourceIndex].clip = clip;
		musicSources [musicSourceIndex].Play ();

		StartCoroutine (AnimateMusicCrossfade(fadeDuration));
	}

	public void PlaySound(AudioClip clip, Vector3 pos) {
		if(clip != null)
			AudioSource.PlayClipAtPoint (clip, pos, sfxVolume * masterVolume);
	}

	public void PlaySound(string title, Vector3 pos) {
		PlaySound (soundLibrary.getClipFromTitle (title), pos);
	}

	public void PlaySound(string title) {
		sfxSource2D.PlayOneShot (soundLibrary.getClipFromTitle (title), sfxVolume * masterVolume);
	}

	IEnumerator AnimateMusicCrossfade(float duration) {
		float percent = 0;

		while (percent < 1) {
			percent += Time.deltaTime * 1 / duration;
			musicSources [musicSourceIndex].volume = Mathf.Lerp (0, musicVolume * masterVolume, percent);
			musicSources [1 - musicSourceIndex].volume = Mathf.Lerp (musicVolume * masterVolume, 0, percent);
			yield return null;
		}
	}
}
