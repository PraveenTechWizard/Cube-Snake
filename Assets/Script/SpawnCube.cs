using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SpawnCube : MonoBehaviourPunCallbacks
{
    [Header("Spawn GameObjects")]
    [SerializeField]
    private List<GameObject> spawnCubes = new List<GameObject>();


    [Header("Spawn Settings")]
    [SerializeField]
    private int startingSpawnCount = 50;

    [SerializeField]
    private int minimumCubeCount = 30;

    [SerializeField]
    private int respawnAmount = 10;

    [SerializeField]
    private float spawnHeightOffset = 0.1f;


    [Header("Check Settings")]
    [SerializeField]
    private float checkInterval = 1f;


    [Header("Plane")]
    [SerializeField]
    private Transform plane;


    private Renderer _planeRenderer;
    private Coroutine _checkRoutine;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (plane == null)
        {
            Debug.LogError(
                "SpawnCube: Plane is not assigned."
            );

            return;
        }


        _planeRenderer =
            plane.GetComponent<Renderer>();


        if (_planeRenderer == null)
        {
            Debug.LogError(
                "SpawnCube: Plane needs a Renderer component."
            );
        }
    }


    private void Start()
    {
        // Must be inside a Photon room.
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning(
                "SpawnCube: Player is not inside a Photon room."
            );

            return;
        }


        // Only Master Client controls
        // world cube spawning.
        if (PhotonNetwork.IsMasterClient)
        {
            StartSpawning();
        }
    }


    // =========================================================
    // START SPAWNING
    // =========================================================

    private void StartSpawning()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }


        if (_planeRenderer == null)
        {
            Debug.LogError(
                "SpawnCube: Plane Renderer is missing."
            );

            return;
        }


        Debug.Log(
            "MASTER CLIENT: Starting cube spawning."
        );


        // Initial cubes.
        SpawnCubes(
            startingSpawnCount
        );


        // Start checking cube count.
        if (_checkRoutine == null)
        {
            _checkRoutine =
                StartCoroutine(
                    CheckCubeCountRoutine()
                );
        }
    }


    // =========================================================
    // CHECK CUBE COUNT
    // =========================================================

    private IEnumerator CheckCubeCountRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                checkInterval
            );


            // Only Master Client manages
            // the world cube population.
            if (!PhotonNetwork.IsMasterClient)
            {
                continue;
            }


            CheckCubeCount();
        }
    }


    private void CheckCubeCount()
    {
        int currentCubeCount =
            CountNetworkCubes();


        Debug.Log(
            $"Network Cubes: {currentCubeCount}"
        );


        if (currentCubeCount <
            minimumCubeCount)
        {
            Debug.Log(
                $"Cube count is low ({currentCubeCount}). " +
                $"Spawning {respawnAmount} cubes."
            );


            SpawnCubes(
                respawnAmount
            );
        }
    }


    // =========================================================
    // COUNT NETWORK CUBES
    // =========================================================

    private int CountNetworkCubes()
    {
        int count = 0;


        NumberCube[] allCubes =
            FindObjectsByType<NumberCube>(
                FindObjectsSortMode.None
            );


        int cubeLayer =
            LayerMask.NameToLayer("Cube");


        foreach (NumberCube cube in allCubes)
        {
            if (cube == null)
            {
                continue;
            }


            if (cube.gameObject.layer ==
                cubeLayer)
            {
                count++;
            }
        }


        return count;
    }


    // =========================================================
    // SPAWN CUBES
    // =========================================================

    private void SpawnCubes(int amount)
    {
        // Safety check.
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }


        if (spawnCubes == null ||
            spawnCubes.Count == 0)
        {
            Debug.LogError(
                "SpawnCube: No cube prefabs assigned."
            );

            return;
        }


        if (_planeRenderer == null)
        {
            Debug.LogError(
                "SpawnCube: Plane Renderer is missing."
            );

            return;
        }


        for (int i = 0; i < amount; i++)
        {
            SpawnRandomCube();
        }
    }


    // =========================================================
    // SPAWN RANDOM CUBE
    // =========================================================

    private void SpawnRandomCube()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }


        int randomIndex =
            Random.Range(
                0,
                spawnCubes.Count
            );


        GameObject cubePrefab =
            spawnCubes[randomIndex];


        if (cubePrefab == null)
        {
            Debug.LogWarning(
                "SpawnCube: Cube prefab is null."
            );

            return;
        }


        // Make sure prefab has PhotonView.
        PhotonView photonView =
            cubePrefab.GetComponent<PhotonView>();


        if (photonView == null)
        {
            Debug.LogError(
                $"SpawnCube: '{cubePrefab.name}' " +
                "does not have a PhotonView."
            );

            return;
        }


        Vector3 spawnPosition =
            GetRandomSpawnPosition();


        // =====================================================
        // PHOTON ROOM OBJECT
        // =====================================================

        PhotonNetwork.InstantiateRoomObject(
            cubePrefab.name,
            spawnPosition,
            Quaternion.identity
        );
    }


    // =========================================================
    // RANDOM SPAWN POSITION
    // =========================================================

    private Vector3 GetRandomSpawnPosition()
    {
        Bounds bounds =
            _planeRenderer.bounds;


        float randomX =
            Random.Range(
                bounds.min.x,
                bounds.max.x
            );


        float randomZ =
            Random.Range(
                bounds.min.z,
                bounds.max.z
            );


        return new Vector3(
            randomX,
            bounds.max.y +
            spawnHeightOffset,
            randomZ
        );
    }


    // =========================================================
    // MASTER CLIENT SWITCH
    // =========================================================

    public override void OnMasterClientSwitched(
        Player newMasterClient)
    {
        Debug.Log(
            "Master Client switched to: " +
            newMasterClient.NickName
        );


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log(
                "I am now the Master Client. " +
                "Starting cube management."
            );


            if (_checkRoutine == null)
            {
                _checkRoutine =
                    StartCoroutine(
                        CheckCubeCountRoutine()
                    );
            }
        }
        else
        {
            StopCubeManagement();
        }
    }


    // =========================================================
    // STOP CUBE MANAGEMENT
    // =========================================================

    private void StopCubeManagement()
    {
        if (_checkRoutine == null)
        {
            return;
        }


        StopCoroutine(
            _checkRoutine
        );


        _checkRoutine = null;
    }


    // =========================================================
    // LEFT ROOM
    // =========================================================

    public override void OnLeftRoom()
    {
        StopCubeManagement();
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        StopCubeManagement();
    }
}