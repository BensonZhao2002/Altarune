using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTrigger : MonoBehaviour {
    /*
    [SerializeField] private AudioClip clip;

    public void Play(float delay = 0) {
        if (delay <= 0) {
            GM.AudioManager.PlayMusic(clip);
        } else {
            StartCoroutine(IPlayMusic(delay));
        }
    }

    private IEnumerator IPlayMusic(float delay) {
        yield return new WaitForSeconds(delay);
        GM.AudioManager.PlayMusic(clip);
    }
    */

    [SerializeField] private AK.Wwise.State musicState;

    public void Play(float delay = 0) {
        if (delay <= 0) {
            SetMusicState();
        }
        else {
            Invoke(nameof(SetMusicState), delay);
        }
    }

    private void SetMusicState() {
        if (musicState.IsValid()) {
            musicState.SetValue();
            Debug.Log($"Music State Switched to: {musicState.Name}");
        }
    }
}