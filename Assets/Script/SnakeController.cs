using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    // =========================================================
    // INSPECTOR
    // =========================================================

    [Header("Starting Settings")]
    [SerializeField] private int startingValue = 2;

    [Header("Body Settings")]
    [SerializeField] private float bodySpacing = 1.2f;

    [Header("Merge Settings")]
    [SerializeField] private float mergeDelay = 0.3f;

    [Header("Head")]
    [SerializeField] private TextMeshPro headText;


    // =========================================================
    // SNAKE DATA
    // =========================================================

    // Index 0 = Head.
    //
    // Example:
    // [8][4][2][2]
    private readonly List<int> _values =
        new List<int>();


    // Index 0 = Head GameObject.
    //
    // Example:
    // [Head][4Cube][2Cube][2Cube]
    private readonly List<GameObject> _bodyCubes =
        new List<GameObject>();


    private bool _isMerging;


    // =========================================================
    // PUBLIC
    // =========================================================

    public int HeadValue
    {
        get
        {
            if (_values.Count == 0)
                return 0;

            return _values[0];
        }
    }


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        // Add the head.
        _values.Add(startingValue);

        _bodyCubes.Add(gameObject);
    }


    private void Start()
    {
        UpdateHeadVisual();

        UpdateBodyPositions();

        PrintSnake();
    }


    private void Update()
    {
        // Keep snake head at fixed height.
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
    }


    // =========================================================
    // COLLECT CUBE
    // =========================================================

    public bool CollectValue(
        int value,
        GameObject collectedCube)
    {
        if (collectedCube == null)
            return false;


        // -----------------------------------------------------
        // Cannot collect a greater cube.
        // -----------------------------------------------------

        if (value > HeadValue)
        {
            Debug.Log(
                $"Cannot collect {value}. " +
                $"Head = {HeadValue}"
            );

            return false;
        }


        // -----------------------------------------------------
        // REMOVE FROM OLD SNAKE
        // -----------------------------------------------------

        SnakeController oldOwner =
            GetOwnerOfCube(collectedCube);


        if (oldOwner != null &&
            oldOwner != this)
        {
            bool removed = oldOwner.RemoveBodyCube(collectedCube);
            
            // Could be the opponent head.
            if (!removed)
            {
                return false;
            }
        }


        // -----------------------------------------------------
        // Prevent duplicate cube.
        // -----------------------------------------------------

        if (_bodyCubes.Contains(collectedCube))
        {
            return false;
        }


        // -----------------------------------------------------
        // ADD EXACT SAME CUBE
        // -----------------------------------------------------

        _values.Add(value);

        _bodyCubes.Add(collectedCube);


        // -----------------------------------------------------
        // CHANGE OWNER
        // -----------------------------------------------------

        collectedCube.transform.SetParent(transform, true);


        // -----------------------------------------------------
        // Sort greater → smaller.
        // -----------------------------------------------------

        SortSnakeByValue();


        // -----------------------------------------------------
        // Update all cube values.
        // -----------------------------------------------------

        UpdateAllVisuals();


        PrintSnake();


        // -----------------------------------------------------
        // START MERGING
        // -----------------------------------------------------

        if (!_isMerging)
        {
            StartCoroutine(MergeRoutine());
        }


        return true;
    }


    // =========================================================
    // GET OLD OWNER
    // =========================================================

    private SnakeController GetOwnerOfCube(
        GameObject cube)
    {
        Transform currentParent = cube.transform.parent;
        
        while (currentParent != null)
        {
            SnakeController snake = currentParent.GetComponent<SnakeController>();

            if (snake != null)
            {
                return snake;
            }

            currentParent = currentParent.parent;
        }
        
        return null;
    }


    // =========================================================
    // REMOVE BODY CUBE
    // =========================================================

    public bool RemoveBodyCube(GameObject cube)
    {
        int index = _bodyCubes.IndexOf(cube);

        // Cube is not part of this snake.
        if (index == -1)
            return false;
        
        // Index 0 is the head.
        // Cannot directly collect head with this method.
        if (index == 0)
            return false;


        // -----------------------------------------------------
        // REMOVE FROM LISTS
        // -----------------------------------------------------

        _bodyCubes.RemoveAt(index);

        _values.RemoveAt(index);


        // -----------------------------------------------------
        // REMOVE PARENT
        // -----------------------------------------------------

        cube.transform.SetParent(null, true);
        
        // -----------------------------------------------------
        // SORT OLD SNAKE AGAIN
        // -----------------------------------------------------

        SortSnakeByValue();

        UpdateAllVisuals();

        PrintSnake();


        return true;
    }


    // =========================================================
    // SORT SNAKE
    // =========================================================

    private void SortSnakeByValue()
    {
        // Head always stays at index 0.
        //
        // Body:
        // Greater → Smaller
        //
        // [8][4][2][2]

        for (int i = 1; i < _values.Count - 1; i++)
        {
            for (int j = i + 1; j < _values.Count; j++)
            {
                if (_values[j] > _values[i])
                {
                    // Swap values.
                    int tempValue = _values[i];

                    _values[i] = _values[j];

                    _values[j] = tempValue;

                    // Swap GameObjects.
                    GameObject tempCube = _bodyCubes[i];

                    _bodyCubes[i] = _bodyCubes[j];

                    _bodyCubes[j] = tempCube;
                }
            }
        }
        
        //
        UpdateBodyPositions();
    }

    // =========================================================
    // UPDATE BODY POSITIONS
    // =========================================================

    private void UpdateBodyPositions()
    {
        if (_bodyCubes.Count <= 1)
            return;


        Vector3 direction = transform.forward;

        direction.y = 0f;


        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }


        direction.Normalize();


        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            GameObject previousCube = _bodyCubes[i - 1];

            GameObject currentCube = _bodyCubes[i];


            if (previousCube == null ||
                currentCube == null)
            {
                continue;
            }


            Vector3 targetPosition = previousCube.transform.position - direction * bodySpacing;

            targetPosition.y = previousCube.transform.position.y;
            
            currentCube.transform.position = targetPosition;

            currentCube.transform.rotation = previousCube.transform.rotation;
        }
    }


    // =========================================================
    // MERGE ROUTINE
    // =========================================================

    private IEnumerator MergeRoutine()
    {
        _isMerging = true;


        while (true)
        {
            bool foundMerge = false;


            // Start from tail.
            for (int i = _values.Count - 1; i > 0; i--)
            {
                if (_values[i] != _values[i - 1])
                {
                    continue;
                }
                
                foundMerge = true;
                
                // Store actual cubes before waiting.
                GameObject firstCube = _bodyCubes[i - 1];

                GameObject secondCube = _bodyCubes[i];


                Debug.Log($"Merge: {_values[i - 1]} + " + $"{_values[i]}");

                yield return new WaitForSeconds(mergeDelay);
                
                // -------------------------------------------------
                // Find indexes again.
                //
                // During the delay, another snake may have
                // collected one of these cubes.
                // -------------------------------------------------

                int firstIndex = _bodyCubes.IndexOf(firstCube);

                int secondIndex =
                    _bodyCubes.IndexOf(secondCube);

                // Cube was removed/transferred.
                if (firstIndex == -1 || secondIndex == -1)
                {
                    break;
                }


                // Must still be adjacent.
                if (secondIndex != firstIndex + 1)
                {
                    break;
                }


                // Must still have same value.
                if (_values[firstIndex] != _values[secondIndex])
                {
                    break;
                }


                MergeAtIndex(firstIndex);


                // Restart from tail.
                break;
            }


            // No merge exists.
            if (!foundMerge)
            {
                break;
            }
        }


        UpdateAllVisuals();

        PrintSnake();

        _isMerging = false;
    }


    // =========================================================
    // MERGE TWO CUBES
    // =========================================================

    private void MergeAtIndex(
        int firstIndex)
    {
        int secondIndex = firstIndex + 1;


        if (firstIndex < 0 || secondIndex >= _values.Count)
        {
            return;
        }


        // -----------------------------------------------------
        // Double first value.
        // -----------------------------------------------------

        _values[firstIndex] *= 2;


        // -----------------------------------------------------
        // Cube that disappears.
        // -----------------------------------------------------

        GameObject cubeToDestroy = _bodyCubes[secondIndex];


        // -----------------------------------------------------
        // Remove from lists.
        // -----------------------------------------------------

        _values.RemoveAt(secondIndex);

        _bodyCubes.RemoveAt(secondIndex);


        // -----------------------------------------------------
        // Destroy second cube.
        // -----------------------------------------------------

        if (cubeToDestroy != null && cubeToDestroy != gameObject)
        {
            Destroy(cubeToDestroy);
        }
        
        // -----------------------------------------------------
        // Sort again.
        // -----------------------------------------------------

        SortSnakeByValue();
        
        // -----------------------------------------------------
        // Update all visuals and actual values.
        // -----------------------------------------------------

        UpdateAllVisuals();

        UpdateBodyPositions();
    }
    
    // =========================================================
    // UPDATE ALL VISUALS
    // =========================================================

    private void UpdateAllVisuals()
    {
        // Update head.
        UpdateHeadVisual();
        
        // Update body cubes.
        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            UpdateSegmentVisual(i);
        }
    }

    // =========================================================
    // UPDATE BODY CUBE VISUAL
    // =========================================================

    private void UpdateSegmentVisual(
        int index)
    {
        if (index < 0 || index >= _bodyCubes.Count)
        {
            return;
        }


        GameObject cube = _bodyCubes[index];
        
        if (cube == null)
            return;
        
        // -----------------------------------------------------
        // UPDATE TEXT
        // -----------------------------------------------------

        TextMeshPro text =
            cube.GetComponentInChildren<
                TextMeshPro>();


        if (text != null)
        {
            text.text = _values[index].ToString();
        }

        // -----------------------------------------------------
        // UPDATE ACTUAL VALUE
        // -----------------------------------------------------

        NumberCube numberCube = cube.GetComponent<NumberCube>();

        if (numberCube != null)
        {
            numberCube.SetValue(_values[index]);
        }
    }

    // =========================================================
    // UPDATE HEAD VISUAL
    // =========================================================

    private void UpdateHeadVisual()
    {
        if (_values.Count == 0)
            return;


        // Update head text.
        if (headText != null)
        {
            headText.text =
                _values[0].ToString();
        }


        // Update NumberCube if the head has one.
        NumberCube numberCube =
            GetComponent<NumberCube>();


        if (numberCube != null)
        {
            numberCube.SetValue(_values[0]);
        }
    }
    
    // =========================================================
    // DEBUG
    // =========================================================

    private void PrintSnake()
    {
        string result = $"{gameObject.name} Snake: ";

        foreach (int value in _values)
        {
            result += $"[{value}]";
        }

        Debug.Log(result);
    }
}