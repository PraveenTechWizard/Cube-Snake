using Photon.Pun;
using TMPro;
using UnityEngine;

public class NumberCube : MonoBehaviourPun, IPunObservable
{
    [Header("Cube Data")]
    [SerializeField] private Cubes cubes;

    [Header("Cube Value Text")]
    [SerializeField] private TextMeshPro textMesh;

    private Renderer _renderer;

    private int _cubeValue;

    public int CubeValue => _cubeValue;


    private void Awake()
    {
        _renderer = GetComponent<Renderer>();

        // Initialize only once
        if (cubes != null)
        {
            _cubeValue = cubes.cubeValue;
        }

        UpdateVisual();
    }


    // =====================================================
    // SET VALUE
    // =====================================================

    public void SetValue(int value)
    {
        _cubeValue = value;

        UpdateVisual();
    }


    // =====================================================
    // UPDATE VISUAL
    // =====================================================

    private void UpdateVisual()
    {
        if (textMesh != null)
        {
            textMesh.text = _cubeValue.ToString();
        }

        if (_renderer != null &&
            cubes != null &&
            cubes.cubeColor != null)
        {
            _renderer.material = cubes.cubeColor;
        }
    }


    // =====================================================
    // PHOTON SYNCHRONIZATION
    // =====================================================

    public void OnPhotonSerializeView(
        PhotonStream stream,
        PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_cubeValue);
        }
        else
        {
            _cubeValue = (int)stream.ReceiveNext();

            UpdateVisual();
        }
    }
}