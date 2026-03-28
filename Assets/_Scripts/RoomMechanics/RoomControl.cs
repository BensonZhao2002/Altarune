using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum RoomTag { Lab, F1, F2, F3, C1, F4, C2, F5, F6, F7, C3, F8 }

public class RoomControl : MonoBehaviour {

    [SerializeField] private RoomTag roomTag;
    [SerializeField] private AudioClip musicTrack;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [SerializeField] private SpawnPoint[] spawnPoints;
    [SerializeField] private Transform defaultSpawnPoint;
    public Transform SpawnPoint => defaultSpawnPoint;

    [SerializeField] private RoomCompletionListener listener;
    [SerializeField] private DoorToggle[] doors;

    [SerializeField] private AK.Wwise.State roomMusicState;
    [SerializeField] private AK.Wwise.Event musicEvent;

    private readonly Dictionary<RoomTag, Transform> spawnPointMap = new();
    private readonly float doorCollapseTimer = 3;

    void OnEnable() {
        if (GM.Instance) Init();
        else StartCoroutine(DelayInit());
    }

    private void Init() {
        GM.Instance.OnPlayerInit += GM_OnPlayerInit;
        listener.OnRoomCleared += Listener_OnRoomCleared;
        listener.OnEntityPerish += Listener_OnEntityPerish;

        //if (musicTrack) GM.AudioManager.PlayMusic(musicTrack);
        //else GM.AudioManager.FadeMusic(1);

        // --- RoomTag mapping to MusicState
        string targetState = "Story"; // Default

        switch (roomTag) {
            case RoomTag.Lab:
                targetState = "Story";
                break;
            case RoomTag.C1:
            case RoomTag.C2:
            case RoomTag.C3:
            case RoomTag.F4:
            case RoomTag.F8:
                targetState = "Checkpoint";
                break;
            case RoomTag.F1:
            case RoomTag.F2:
            case RoomTag.F3:
                targetState = "Combat_Catacomb";
                break;
            case RoomTag.F5:
            case RoomTag.F6:
            case RoomTag.F7:
                targetState = "Combat_Brimstone";
                break;
        }

        AkSoundEngine.SetState("Music", targetState);

        foreach (SpawnPoint spawnPoint in spawnPoints) {
            spawnPointMap[spawnPoint.originTag] = spawnPoint.spawnTransform;
        }

        GM.RunManager.AddClearedExit(roomTag, GM.RoomManager.SourceRoomTag);

        HashSet<RoomTag> exits = GM.RunManager.GetClearedExits(roomTag);
        foreach (DoorToggle door in doors) {
            bool isOpen = exits.Contains(door.ExitTag);
            door.ToggleImmediate(isOpen);
        }

        listener.Init(roomTag);
        if (!GM.RunManager.IsComplete(roomTag)) StartCoroutine(ISealRoom());
    }

    private void GM_OnPlayerInit() {
        RoomTag sourceRoomTag = GM.RoomManager.SourceRoomTag;
        Transform spawnPoint = GetSpawnPoint(sourceRoomTag);
        GM.Player.TryTeleport(spawnPoint.position);
        GM.Player.OnTeleportEnd += Player_OnTeleportEnd;
    }

    private void Player_OnTeleportEnd() {
        GM.Player.OnTeleportEnd -= Player_OnTeleportEnd;
        GM.RoomManager.CurrentRoom = this;
        GM.RoomManager.FinalizeRoomTransition();
        virtualCamera.Follow = playerController.CameraTarget;
    }

    private Transform GetSpawnPoint(RoomTag roomTag) {
        if (spawnPointMap.TryGetValue(roomTag, out Transform spawnPoint)) {
            return spawnPoint;
        } else return defaultSpawnPoint;
    }

    private void Listener_OnRoomCleared() {
        StopAllCoroutines();
        foreach (DoorToggle door in doors) {
            door.Toggle(true);
            GM.RunManager.AddClearedExit(roomTag, door.ExitTag);
        }
        GM.RunManager.CompleteRoom(roomTag);
    }

    private void Listener_OnEntityPerish(BaseObject baseObject) {
        Transform t = baseObject.transform;
        PerishInfo info = new() {
            name = t.gameObject.name,
            position = t.position,
            rotation = t.rotation
        };
        GM.RunManager.AddPerishInfo(roomTag, info);
    }

    public void ForceCompletion() => listener.CompleteRoom();

    private IEnumerator ISealRoom() {
        yield return new WaitForSeconds(doorCollapseTimer);
        foreach (DoorToggle door in doors) {
            door.Toggle(false);
        }
    }

    private IEnumerator DelayInit() {
        yield return new WaitForEndOfFrame();
        Init();
    }
}

[System.Serializable]
public class SpawnPoint {
    public RoomTag originTag;
    public Transform spawnTransform;
} 