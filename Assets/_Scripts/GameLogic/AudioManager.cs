using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    //[SerializeField] private float masterVolume, musicVolume, soundVolume;
    // [SerializeField] private AudioSource musicSource;

    [SerializeField] private AK.Wwise.RTPC masterVolRTPC, musicVolRTPC, sfxVolRTPC;

    /*
    public float MusicVolume => masterVolume * musicVolume;
    public float SFXVolume => masterVolume * soundVolume;

    public void PlayMusic(AudioClip clip) {
        musicSource.volume = MusicVolume;
        if (musicSource.clip != clip) {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void FadeMusic(float duration) {
        StopAllCoroutines();
        StartCoroutine(IFadeMusic(duration));
    }

    private IEnumerator IFadeMusic(float duration) {
        float lerpVal, timer = 0,
              currVolume = MusicVolume;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            lerpVal = timer / duration;
            musicSource.volume = Mathf.Lerp(currVolume, 0, lerpVal);
            yield return null;
        }
    }
    */

    private void Start() {
        AkSoundEngine.PostEvent("Music_Play", gameObject);
        UpdateVolumes();
    }

    public void UpdateVolumes(float master = 1, float music = 1, float sfx = 1) {
        masterVolRTPC.SetGlobalValue(master * 100);
        musicVolRTPC.SetGlobalValue(music * 100);
        sfxVolRTPC.SetGlobalValue(sfx * 100);
    }
}