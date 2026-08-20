using System.Collections.Generic;
using UnityEngine;

public class SpawnCube : MonoBehaviour
{
    [Header("Spawn GameObjects")]
    [SerializeField] private List<GameObject> spawnCubes = new List<GameObject>();

    [SerializeField] private int spawnCount = 10;
    [SerializeField] private float spawnHeightOffset = 0.1f;

    [Header("Plane")]
    [SerializeField] private Transform plane;


    private void Start()
    {
        SpawnCubesRandomly();
    }


    private void SpawnCubesRandomly()
    {
        if (spawnCubes.Count == 0)
        {
            Debug.LogError("No cubes assigned to Spawn Cubes list.");
            return;
        }

        Renderer planeRenderer = plane.GetComponent<Renderer>();

        if (planeRenderer == null)
        {
            Debug.LogError("Plane needs a Renderer.");
            return;
        }

        // Create a copy so the Inspector list order doesn't change.
        List<GameObject> shuffledCubes =
            new List<GameObject>(spawnCubes);

        // Shuffle the cube list.
        ShuffleList(shuffledCubes);


        for (int i = 0; i < spawnCount; i++)
        {
            // If spawnCount is greater than list size,
            // start again from a random position in the shuffled list.
            GameObject cubeToSpawn =
                shuffledCubes[i % shuffledCubes.Count];

            SpawnCubeAtRandomPosition(
                cubeToSpawn,
                planeRenderer
            );
        }
    }


    private void SpawnCubeAtRandomPosition(
        GameObject cubeToSpawn,
        Renderer planeRenderer)
    {
        Bounds bounds = planeRenderer.bounds;

        float randomX = Random.Range(
            bounds.min.x,
            bounds.max.x
        );

        float randomZ = Random.Range(
            bounds.min.z,
            bounds.max.z
        );

        Vector3 spawnPosition = new Vector3(
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


    private void ShuffleList(List<GameObject> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            GameObject temporary = list[i];

            list[i] = list[randomIndex];

            list[randomIndex] = temporary;
        }
    }
}