using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCube : MonoBehaviour
{
    [Header("Spawn GameObjects")]
    [SerializeField] private List<GameObject> spawnCubes =
        new List<GameObject>();

    [Header("Spawn Settings")]
    [SerializeField] private int startingSpawnCount = 50;
    [SerializeField] private int minimumCubeCount = 30;
    [SerializeField] private int respawnAmount = 10;

    [SerializeField] private float spawnHeightOffset = 0.1f;

    [Header("Check Settings")]
    [SerializeField] private float checkInterval = 1f;

    [Header("Plane")]
    [SerializeField] private Transform plane;


    private Renderer _planeRenderer;

    private bool _isSpawning;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (plane == null)
        {
            Debug.LogError("Plane is not assigned.");
            return;
        }

        _planeRenderer = plane.GetComponent<Renderer>();

        if (_planeRenderer == null)
        {
            Debug.LogError("Plane needs a Renderer.");
        }
    }


    private void Start()
    {
        // Spawn the initial 50 cubes.
        SpawnCubes(startingSpawnCount);

        // Start continuously checking the cube count.
        StartCoroutine(CheckCubeCountRoutine());
    }


    // =========================================================
    // CHECK CUBE COUNT
    // =========================================================

    private IEnumerator CheckCubeCountRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            CheckCubeCount();
        }
    }


    // =========================================================
    // CHECK
    // =========================================================

    private void CheckCubeCount()
    {
        int currentCubeCount = CountWorldCubes();

        Debug.Log(
            $"World Cubes: {currentCubeCount}"
        );


        // -----------------------------------------------------
        // If cubes are below minimum,
        // spawn another batch.
        // -----------------------------------------------------

        if (currentCubeCount < minimumCubeCount)
        {
            Debug.Log(
                $"Cube count is low ({currentCubeCount}). " +
                $"Spawning {respawnAmount} cubes."
            );

            SpawnCubes(respawnAmount);
        }
    }


    // =========================================================
    // COUNT WORLD CUBES
    // =========================================================

    private int CountWorldCubes()
    {
        /*
         * Find all objects with the Cube tag.
         *
         * This counts only cubes currently existing
         * in the scene.
         */

        GameObject[] cubes =
            GameObject.FindGameObjectsWithTag("Cube");

        return cubes.Length;
    }


    // =========================================================
    // SPAWN CUBES
    // =========================================================

    private void SpawnCubes(int amount)
    {
        if (_isSpawning)
        {
            return;
        }

        if (spawnCubes.Count == 0)
        {
            Debug.LogError(
                "No cubes assigned to Spawn Cubes list."
            );

            return;
        }

        if (_planeRenderer == null)
        {
            Debug.LogError(
                "Plane Renderer is missing."
            );

            return;
        }


        _isSpawning = true;


        for (int i = 0; i < amount; i++)
        {
            SpawnRandomCube();
        }


        _isSpawning = false;
    }


    // =========================================================
    // SPAWN RANDOM CUBE
    // =========================================================

    private void SpawnRandomCube()
    {
        int randomIndex =
            Random.Range(
                0,
                spawnCubes.Count
            );


        GameObject cubeToSpawn =
            spawnCubes[randomIndex];


        SpawnCubeAtRandomPosition(
            cubeToSpawn
        );
    }


    // =========================================================
    // SPAWN POSITION
    // =========================================================

    private void SpawnCubeAtRandomPosition(
        GameObject cubeToSpawn)
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


        Vector3 spawnPosition =
            new Vector3(
                randomX,
                bounds.max.y + spawnHeightOffset,
                randomZ
            );


        Instantiate(
            cubeToSpawn,
            spawnPosition,
            Quaternion.identity
        );
    }
}