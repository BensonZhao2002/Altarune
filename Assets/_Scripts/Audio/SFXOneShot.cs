using System.Collections;
using UnityEngine;

public class SFXOneShot : MonoBehaviour {
    
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip clip;
    [SerializeField] private Vector2 pitchVariation;
    [SerializeField] private float volumeMult = 1;

    public AudioClip Clip => clip;
    /*
    public void Play() {
        // source.volume = GM.AudioManager.SFXVolume * volumeMult;
        source.volume = volumeMult;
        source.pitch = 1 + Random.Range(pitchVariation.x, pitchVariation.y);
        source.PlayOneShot(clip);
    }

    public void Stop(float duration = 0.1f) {
        StartCoroutine(IStop(duration));
    }

    private IEnumerator IStop(float duration) {
        // float lerpVal, timer = 0, currVolume = GM.AudioManager.SFXVolume;
        float lerpVal, timer = 0, currVolume = 1f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            lerpVal = timer / duration;
            source.volume = Mathf.Lerp(currVolume, 0, lerpVal);
            yield return null;
        }
    }
    */

    [SerializeField] private AK.Wwise.Event shotEvent;

    public void Play() {
        shotEvent.Post(gameObject);
    }

    public void Stop(float duration) {
        shotEvent.Stop(gameObject, (int)(duration * 1000));
    }
}