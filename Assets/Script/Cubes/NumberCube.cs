using TMPro;
using UnityEngine;

public class NumberCube : MonoBehaviour
{
    [Header("Cube Data")]
    [SerializeField] private Cubes cubes;

    [Header("Cube Value Text")]
    [SerializeField] private TextMeshPro textMesh;

    private Renderer _renderer;

    // Runtime value of this specific cube.
    private int _cubeValue;

    // Other scripts can read this value.
    public int CubeValue => _cubeValue;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        InitializeCube();
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

    private void InitializeCube()
    {
        if (cubes == null)
        {
            Debug.LogError(
                $"Cube data is missing on {gameObject.name}"
            );

            return;
        }

        // Get starting value from ScriptableObject.
        _cubeValue = cubes.cubeValue;

        UpdateVisual();
    }


    // =========================================================
    // SET VALUE
    // =========================================================

    public void SetValue(int value)
    {
        _cubeValue = value;

        UpdateText();
    }


    // =========================================================
    // GET VALUE
    // =========================================================

    public int GetCubeValue()
    {
        return _cubeValue;
    }


    // =========================================================
    // UPDATE VISUAL
    // =========================================================

    public void UpdateVisual()
    {
        UpdateText();

        if (_renderer != null && cubes != null)
        {
            _renderer.material = cubes.cubeColor;
        }
    }


    // =========================================================
    // UPDATE TEXT
    // =========================================================

    private void UpdateText()
    {
        if (textMesh != null)
        {
            textMesh.text = _cubeValue.ToString();
        }
    }
}