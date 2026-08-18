using System;
using TMPro;
using UnityEngine;

public class CollusionAction : MonoBehaviour
{
    private TextMeshPro _textMeshPro;
    private int _currentValue = 0; // Updating HeadCube Value
    private float _cubeSize = 1.2f; // Head Cube Increasing Staring Size
    
    public int _startingValue = 2; // Head Starting Value
    
    //
    private NumberCube _cube;
    private int _obstacleCubeValue;

    private void Awake()
    {
        _currentValue = _startingValue;
        _textMeshPro = GetComponentInChildren<TextMeshPro>();
    }

    private void Start()
    {
        _textMeshPro.text = _startingValue.ToString();
    }

    //
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Cube"))
        {
            Debug.Log("Cube will Hit");
            
            // GET OBSTACLE VALUE
            _cube = other.gameObject.GetComponent<NumberCube>();
            _obstacleCubeValue = _cube.cubeValue;
            
            //
            AddSameValue(other);
        }
    }

    private void AddSameValue(Collision other)
    {
        // IF THE SAME VALUE IT WILL BE MULTIPLE
        if (_obstacleCubeValue == _currentValue)
        {
            // INCREASE CUBE SIZE & UPDATE CUBE SIZE
            transform.localScale = Vector3.one * _cubeSize;
            // 
            _cubeSize += 0.2f;
                
            // ADD THE SAME VALUE 
            _currentValue += _currentValue;
            _textMeshPro.text = _currentValue.ToString();
                
            //
            Destroy(other.gameObject);
        }
    }
    
}
