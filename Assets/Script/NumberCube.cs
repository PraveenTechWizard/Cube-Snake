using System;
using TMPro;
using UnityEngine;

public class NumberCube : MonoBehaviour
{
    //
    [SerializeField] private Cubes cubes;
    [SerializeField] private TextMeshPro textMesh;
    //
    public int cubeValue;
    
    private Renderer _renderer;
    
    //
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        cubeValue = cubes.cubeValue;
        //
        _renderer.material = cubes.cubeColor;
        textMesh.text = cubes.cubeValue.ToString();
    }
}
