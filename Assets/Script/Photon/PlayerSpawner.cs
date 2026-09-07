using System.Collections;
using Photon.Pun;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player")]
    [SerializeField]
    private string playerPrefabName = "PlayerHead";
    
    [Header("Spawn Points")]
    [SerializeField]
    private Transform[] spawnPoints;
    
    [Header("Camera")]
    [SerializeField]
    private CinemachineCamera cinemachineCamera;
    
    private bool _hasSpawned;

    
    // 
    private void Start()
    {
        StartCoroutine(
            WaitAndSpawnPlayer()
        );
    }

    
    // WAIT FOR PHOTON ROOM
    private IEnumerator WaitAndSpawnPlayer()
    {
        Debug.Log("Waiting to join Photon room...");
        
        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }
        
        Debug.Log("Photon room ready. Spawning player...");
        
        // Give Photon and scene a moment to initialize.
        yield return new WaitForSeconds(0.2f);

        SpawnPlayer();
    }

    
    // SPAWN PLAYER
    private void SpawnPlayer()
    {
        // Prevent duplicate spawning.
        if (_hasSpawned)
        {
            Debug.Log("Player already spawned.");

            return;
        }
        
        // Safety check.
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Cannot spawn player. " + "Not inside a Photon room.");
            return;
        }
        
        // SPAWN POINT CHECK
        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogError("PlayerSpawner: " + "No spawn points assigned!");

            return;
        }

        
        // PLAYER INDEX
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        playerIndex = Mathf.Clamp(
                playerIndex,
                0,
                spawnPoints.Length - 1);

        Transform spawnPoint = spawnPoints[playerIndex];

        if (spawnPoint == null)
        {
            Debug.LogError(
                "PlayerSpawner: " +
                "Spawn point is null!");
            return;
        }
        
        // PREFAB CHECK
        if (string.IsNullOrEmpty(playerPrefabName))
        {
            Debug.LogError("Player prefab name is empty!");

            return;
        }
        
        
        // PHOTON INSTANTIATE
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, spawnPoint.rotation);

        if (player == null)
        {
            Debug.LogError("Failed to spawn player!");

            return;
        }

        
        // SPAWN COMPLETE
        
        _hasSpawned = true;
        
        Debug.Log(
            "PLAYER SPAWNED!\n" +
            "Name: " +
            PhotonNetwork.NickName +
            "\nActor Number: " +
            PhotonNetwork.LocalPlayer.ActorNumber +
            "\nSpawn Point: " +
            playerIndex);
        
      
        // SET LOCAL CAMERA
        SetCamera(player.transform);
    }
    
    // SET CAMERA

    private void SetCamera(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            Debug.LogError("SetCamera: " + "Player transform is null!");
            return;
        }
        
        
        // FIND CINEMACHINE CAMERA
        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }


        if (cinemachineCamera == null)
        {
            Debug.LogError("No CinemachineCamera found!");
            return;
        }


       
        // ASSIGN LOCAL PLAYER
        cinemachineCamera.Target.TrackingTarget = playerTransform;
        Debug.Log("Cinemachine camera assigned to LOCAL player: " + playerTransform.name);
    }


   
    // JOINED ROOM
    public override void OnJoinedRoom()
    {
        Debug.Log("PlayerSpawner detected room join.");

        if (!_hasSpawned)
        {
            StartCoroutine(WaitAndSpawnPlayer());
        }
    }
}