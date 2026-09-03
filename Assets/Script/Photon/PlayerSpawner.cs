using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player")]
    [SerializeField] private string playerPrefabName = "PlayerHead";


    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;


    private bool _hasSpawned;


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        StartCoroutine(WaitAndSpawnPlayer());
    }


    // =====================================================
    // WAIT FOR PHOTON ROOM
    // =====================================================

    private IEnumerator WaitAndSpawnPlayer()
    {
        Debug.Log("Waiting to join Photon room...");


        // Wait until this client is inside a room.
        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }


        Debug.Log(
            "Photon room ready. Spawning player..."
        );


        // Small delay to make sure the Game scene is ready.
        yield return new WaitForSeconds(0.2f);


        SpawnPlayer();
    }


    // =====================================================
    // SPAWN PLAYER
    // =====================================================

    private void SpawnPlayer()
    {
        // Prevent duplicate spawning.
        if (_hasSpawned)
        {
            Debug.Log(
                "Player already spawned."
            );

            return;
        }


        // Safety check.
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning(
                "Cannot spawn player. " +
                "Still not inside a Photon room."
            );

            return;
        }


        // Check spawn points.
        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogError(
                "No spawn points assigned!"
            );

            return;
        }


        // =================================================
        // GET SPAWN INDEX
        // =================================================

        int playerIndex =
            PhotonNetwork.LocalPlayer.ActorNumber - 1;


        // Keep index inside spawn point array.
        playerIndex =
            Mathf.Clamp(
                playerIndex,
                0,
                spawnPoints.Length - 1
            );


        Transform spawnPoint =
            spawnPoints[playerIndex];


        // =================================================
        // CHECK PREFAB
        // =================================================

        if (string.IsNullOrEmpty(playerPrefabName))
        {
            Debug.LogError(
                "Player Prefab Name is empty!"
            );

            return;
        }


        // =================================================
        // PHOTON INSTANTIATE
        // =================================================

        GameObject player =
            PhotonNetwork.Instantiate(
                playerPrefabName,
                spawnPoint.position,
                spawnPoint.rotation
            );


        if (player == null)
        {
            Debug.LogError(
                "Failed to spawn player!"
            );

            return;
        }


        _hasSpawned = true;


        Debug.Log(
            "PLAYER SPAWNED!\n" +
            "Name: " + PhotonNetwork.NickName +
            "\nActor Number: " +
            PhotonNetwork.LocalPlayer.ActorNumber +
            "\nSpawn Point: " +
            playerIndex
        );
    }


    // =====================================================
    // JOINED ROOM CALLBACK
    // =====================================================

    public override void OnJoinedRoom()
    {
        Debug.Log(
            "PlayerSpawner detected room join."
        );


        if (!_hasSpawned)
        {
            StartCoroutine(
                WaitAndSpawnPlayer()
            );
        }
    }
}