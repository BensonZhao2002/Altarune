using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointAltar : MonoBehaviour {

    [SerializeField] private RoomTag checkpointTag;
    [SerializeField] private LoopingSystemController healEffectController;
    [SerializeField] private float healTickInterval;

    private void Update() {
        //if (Input.GetKeyDown(KeyCode.P)) {
        //    GM.Player.TryDamage(1);
        //}
    }

    void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out Player player)) {
            GM.LeveStateManager.AddRoom(checkpointTag);
            StartCoroutine(IHealPlayer(player));

            AkSoundEngine.PostEvent("Checkpoint_Heal", gameObject);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.TryGetComponent(out Player _)) {
            StopAllCoroutines();
            healEffectController.Disable();

            AkSoundEngine.PostEvent("Checkpoint_Heal_Stop", gameObject);
        }
    }

    private IEnumerator IHealPlayer(Player player) {
        healEffectController.Enable();
        while (player.Health < player.MaxHealth) {
            AkSoundEngine.PostEvent("Checkpoint_Heal_Loop", gameObject);
            yield return new WaitForSeconds(healTickInterval);
            player.TryHeal(1);
        }

        healEffectController.Disable();

        AkSoundEngine.PostEvent("Checkpoint_Heal_Stop", gameObject);
    }
}