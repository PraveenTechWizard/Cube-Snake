using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("Starting Settings")]
    [SerializeField] private int startingValue = 2;

    [Header("Body Settings")]
    [SerializeField] private float bodySpacing = 1.2f;

    [Header("Merge Settings")]
    [SerializeField] private float mergeDelay = 0.3f;

    [Header("Head")]
    [SerializeField] private TextMeshPro headText;

    // ---------------------------------------------------------
    // Snake data
    // ---------------------------------------------------------

    // Example:
    // [8][4][2][2]
    private readonly List<int> _values = new List<int>();

    // The ACTUAL GameObjects.
    //
    // Example:
    // [Head][4Cube][2Cube][2Cube]
    private readonly List<GameObject> _bodyCubes =
        new List<GameObject>();

    private bool _isMerging;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        // Player itself is the head.
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
        // Keep player at fixed Y height.
        transform.position = new Vector3(
            transform.position.x,
            0.5f,
            transform.position.z
        );
    }


    // =========================================================
    // COLLECT
    // =========================================================

    public bool CollectValue(
        int value,
        GameObject collectedCube)
    {
        // Safety check.
        if (collectedCube == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // A cube greater than the HEAD cannot be collected.
        // -----------------------------------------------------

        if (value > _values[0])
        {
            Debug.Log(
                $"Cannot collect {value}. " +
                $"Head value = {_values[0]}"
            );

            return false;
        }


        // -----------------------------------------------------
        // Add the value.
        // -----------------------------------------------------

        _values.Add(value);


        // -----------------------------------------------------
        // IMPORTANT:
        //
        // We add the EXACT cube that was hit.
        //
        // We are NOT creating a new cube.
        // -----------------------------------------------------

        _bodyCubes.Add(collectedCube);


        // -----------------------------------------------------
        // Make the collected cube a child of the player.
        // -----------------------------------------------------

        collectedCube.transform.SetParent(transform);


        // -----------------------------------------------------
        // Sort both lists together.
        // -----------------------------------------------------

        SortSnakeByValue();


        PrintSnake();


        // -----------------------------------------------------
        // Start merge process.
        // -----------------------------------------------------

        if (!_isMerging)
        {
            StartCoroutine(MergeRoutine());
        }


        return true;
    }


    // =========================================================
    // SORT SNAKE
    // =========================================================

    private void SortSnakeByValue()
    {
        /*
         * Head stays at index 0.
         *
         * Body is sorted:
         *
         * [8][4][2][2]
         *
         * greater → smaller
         */

        for (int i = 1; i < _values.Count - 1; i++)
        {
            for (int j = i + 1; j < _values.Count; j++)
            {
                // Greater value should be closer to the head.
                if (_values[j] > _values[i])
                {
                    // ------------------------------------------------
                    // Swap VALUE
                    // ------------------------------------------------

                    int tempValue = _values[i];

                    _values[i] = _values[j];
                    _values[j] = tempValue;


                    // ------------------------------------------------
                    // Swap ACTUAL GAMEOBJECT
                    // ------------------------------------------------

                    GameObject tempCube = _bodyCubes[i];

                    _bodyCubes[i] = _bodyCubes[j];
                    _bodyCubes[j] = tempCube;
                }
            }
        }


        // Put the actual cubes in their new positions.
        UpdateBodyPositions();
    }


    // =========================================================
    // UPDATE BODY POSITIONS
    // =========================================================

    private void UpdateBodyPositions()
    {
        if (_bodyCubes.Count <= 1)
        {
            return;
        }


        // Player's forward direction.
        Vector3 direction = transform.forward;

        // We only move on X/Z.
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        direction.Normalize();


        // -----------------------------------------------------
        // Start from the first body cube.
        // -----------------------------------------------------

        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            GameObject previousCube =
                _bodyCubes[i - 1];

            GameObject currentCube =
                _bodyCubes[i];


            if (previousCube == null ||
                currentCube == null)
            {
                continue;
            }


            // Put current cube behind previous cube.
            Vector3 targetPosition =
                previousCube.transform.position -
                direction * bodySpacing;


            // Keep same Y.
            targetPosition.y =
                previousCube.transform.position.y;


            currentCube.transform.position =
                targetPosition;


            // Same rotation.
            currentCube.transform.rotation =
                previousCube.transform.rotation;
        }
    }


    // =========================================================
    // MERGE ROUTINE
    // =========================================================

    private IEnumerator MergeRoutine()
    {
        _isMerging = true;


        bool merged;


        do
        {
            merged = false;


            /*
             * Example:
             *
             * [8][4][2][2]
             *
             * Start from the tail.
             *
             *        ↓
             * [8][4][2][2]
             *
             * 2 + 2
             */

            for (int i = _values.Count - 1; i > 0; i--)
            {
                int currentValue =
                    _values[i];

                int previousValue =
                    _values[i - 1];


                // ------------------------------------------------
                // Same adjacent values.
                // ------------------------------------------------

                if (currentValue == previousValue)
                {
                    Debug.Log(
                        $"Merge: {previousValue} + {currentValue}"
                    );


                    // Wait so the merge can be seen.
                    yield return new WaitForSeconds(
                        mergeDelay
                    );


                    MergeAtIndex(i - 1);


                    merged = true;


                    // Restart checking from the tail.
                    break;
                }
            }

        } while (merged);


        PrintSnake();

        _isMerging = false;
    }


    // =========================================================
    // MERGE
    // =========================================================

    private void MergeAtIndex(int firstIndex)
    {
        int secondIndex =
            firstIndex + 1;


        // -----------------------------------------------------
        // Double the first cube's value.
        // -----------------------------------------------------

        _values[firstIndex] *= 2;


        // -----------------------------------------------------
        // Get second cube.
        // -----------------------------------------------------

        GameObject cubeToDestroy =
            _bodyCubes[secondIndex];


        // -----------------------------------------------------
        // Destroy ONLY the second cube.
        // -----------------------------------------------------

        if (cubeToDestroy != null &&
            cubeToDestroy != gameObject)
        {
            Destroy(cubeToDestroy);
        }


        // -----------------------------------------------------
        // Remove second value.
        // -----------------------------------------------------

        _values.RemoveAt(secondIndex);


        // -----------------------------------------------------
        // Remove second GameObject.
        // -----------------------------------------------------

        _bodyCubes.RemoveAt(secondIndex);


        // -----------------------------------------------------
        // Update remaining cube number.
        // -----------------------------------------------------

        UpdateSegmentVisual(firstIndex);


        // -----------------------------------------------------
        // If HEAD merged, update head text.
        // -----------------------------------------------------

        if (firstIndex == 0)
        {
            UpdateHeadVisual();
        }


        // -----------------------------------------------------
        // Sort again.
        //
        // Example:
        //
        // [8][4][4][2]
        //
        // becomes:
        //
        // [8][8][2]
        //
        // then:
        //
        // [16][2]
        // -----------------------------------------------------

        SortSnakeByValue();
    }


    // =========================================================
    // UPDATE SEGMENT VISUAL
    // =========================================================

    private void UpdateSegmentVisual(int index)
    {
        if (index < 0 ||
            index >= _bodyCubes.Count)
        {
            return;
        }


        GameObject cube =
            _bodyCubes[index];


        if (cube == null)
        {
            return;
        }


        TextMeshPro text =
            cube.GetComponentInChildren<TextMeshPro>();


        if (text != null)
        {
            text.text =
                _values[index].ToString();
        }
    }


    // =========================================================
    // UPDATE HEAD VISUAL
    // =========================================================

    private void UpdateHeadVisual()
    {
        if (headText != null)
        {
            headText.text =
                _values[0].ToString();
        }
    }


    // =========================================================
    // DEBUG
    // =========================================================

    private void PrintSnake()
    {
        string result = "Snake: ";

        foreach (int value in _values)
        {
            result += $"[{value}]";
        }

        Debug.Log(result);
    }
}