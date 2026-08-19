using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class SpawnCube : MonoBehaviour
{
    [Header("Spawn GameObject")]
    [SerializeField] private GameObject cube;
    [SerializeField] private int spawnCount = 10;
    
    [SerializeField] float spawnHeightOffset = 0.1f;
    
    [Header("Plane")]
    [SerializeField] Transform plane;
    
    //
    private void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnRandom();
        }
    }
    
    //
    void SpawnRandom()
    {
        MeshFilter meshFilter = plane.GetComponent<MeshFilter>();
        Bounds bounds = meshFilter.sharedMesh.bounds;
        
        float width = bounds.size.x;
        float depth = bounds.size.y;
        
        float randomX = Random.Range(-width / 2, width / 2);
        float randomZ = Random.Range(-depth / 2, depth / 2);
        
        Vector3 localPosition = new Vector3(randomX, 0, randomZ);
        
        Vector3 worldPosition = plane.TransformPoint(localPosition);
        worldPosition.y +=  spawnHeightOffset;

        Instantiate(cube, worldPosition, Quaternion.identity);
    }
}
