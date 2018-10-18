using UnityEngine;

//
// MusicPlayer.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Dynamically starts and stops planet music
//

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]

public class MusicPlayer : MonoBehaviour {

    [SerializeField] float maxVolume = 0.5f;
    [SerializeField] float fadeSpeed = 0.1f;

    AudioSource source;
    int fadeDirection = 0;

	void Start () {
        source = GetComponent<AudioSource>();
	}
	
	void Update () {
        if (fadeDirection == -1) FadeOut();
        else if (fadeDirection == 1) FadeIn();
	}

    void OnTriggerEnter(Collider other) {
        if (other.GetComponentInChildren<Camera>()) {
            if (source.isPlaying) {
                fadeDirection = 1;
            } else {
                source.volume = maxVolume;
                source.Play();
            }

            Debug.Log("Music Player: Music zone entered");
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.GetComponentInChildren<Camera>()) {
            Debug.Log("Music Player: Music zone exited");
            fadeDirection = -1;
        }
    }

    void FadeOut() {
        source.volume -= Time.deltaTime * fadeSpeed;
        if (source.volume <= 0f) {
            source.Stop();
            fadeDirection = 0;
        }
    }

    void FadeIn() {
        source.volume += Time.deltaTime * fadeSpeed;
        if (source.volume >= maxVolume) {
            fadeDirection = 0;
        }
    }
}
