using System;
using System.Collections;
using UnityEngine;

public class TowerPlague : Summon {

    [SerializeField] private PlagueArea plagueArea;
    [SerializeField] private float launchInterval;
    private float timer;

    void Awake() {
        plagueArea.EntityID = GetInstanceID();

        AkSoundEngine.PostEvent("Plague_Bubble", gameObject);
    }

    void Update() {
        timer -= Time.deltaTime;
        if (timer <= 0) {
            timer = launchInterval;
            plagueArea.DoWave();

            AkSoundEngine.PostEvent("Plague_Shot", gameObject);
        }
    }

    private void OnDestroy() {
        AkSoundEngine.StopAll(gameObject);
    }
}