using System;
using TMPro;
using UnityEngine;

public class NumberCube : MonoBehaviour
{
    //
    [Header("Cube Data")]
    [SerializeField] private Cubes cubes;
    
    [Header("Cube Value in Text")]
    [SerializeField] private TextMeshPro textMesh;
    //
    private Renderer _renderer;
    public int CubeValue => cubes.cubeValue;
    
    
    //
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        UpdateVisual();
    }
    
    //
    public void SetValue(int value)
    {
        textMesh.text = value.ToString();
    }

    public void UpdateVisual()
    {
        if (cubes == null)
        {
            Debug.LogError("Cube object is null");
            return;
        }
        //
        textMesh.text = cubes.cubeValue.ToString();
        //
        _renderer.material = cubes.cubeColor;
    }
}
