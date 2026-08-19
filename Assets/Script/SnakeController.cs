using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("Starting Settings")]
    [SerializeField] private int startingValue = 8;

    [Header("Body Settings")]
    public GameObject bodyPrefab;
    [SerializeField] private float bodySpacing = 1.2f;

    [Header("Merge Settings")]
    [SerializeField] private float mergeDelay = 0.3f;

    [Header("Head")]
    [SerializeField] private TextMeshPro headText;

    private List<int> _values = new List<int>();
    private List<GameObject> _bodyCubes = new List<GameObject>();

    private bool _isMerging;


    private void Awake()
    {
        // Player is the head.
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
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
    }


    // ============================================================
    // COLLECT VALUE
    // ============================================================

    public bool CollectValue(int value)
    {
        // Cannot collect a value greater than the head.
        if (value > _values[0])
        {
            Debug.Log(
                $"Cannot collect {value}. Head = {_values[0]}"
            );

            return false;
        }

        // Insert value in correct position.
        AddValueInOrder(value);

        PrintSnake();

        // Start merging.
        if (!_isMerging)
        {
            StartCoroutine(MergeRoutine());
        }

        return true;
    }


    // ============================================================
    // ADD VALUE IN DESCENDING ORDER
    // ============================================================

    private void AddValueInOrder(int value)
    {
        int insertIndex = _values.Count;

        // Start from 1 because index 0 is the player/head.
        for (int i = 1; i < _values.Count; i++)
        {
            if (value > _values[i])
            {
                insertIndex = i;
                break;
            }
        }

        // Add value to list.
        _values.Insert(insertIndex, value);

        // Create visual cube.
        GameObject newCube = CreateBodyCube(value);

        // Add visual cube at same index.
        _bodyCubes.Insert(insertIndex, newCube);

        // Rearrange all body cubes.
        UpdateBodyPositions();
    }


    // ============================================================
    // CREATE BODY CUBE
    // ============================================================

    private GameObject CreateBodyCube(int value)
    {
        GameObject newCube = Instantiate(
            bodyPrefab,
            transform.position,
            transform.rotation
        );

        // Make body a child of Player/Head
        newCube.transform.SetParent(transform);

        Rigidbody rb = newCube.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        Collider collider = newCube.GetComponent<Collider>();

        if (collider != null)
        {
            collider.enabled = false;
        }

        TextMeshPro text =
            newCube.GetComponentInChildren<TextMeshPro>();

        if (text != null)
        {
            text.text = value.ToString();
        }

        return newCube;
    }


    // ============================================================
    // UPDATE BODY POSITIONS
    // ============================================================

    private void UpdateBodyPositions()
    {
        Vector3 direction = transform.forward;

        // Make sure movement is only on X/Z plane.
        direction.y = 0f;
        direction.Normalize();

        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            GameObject previousCube = _bodyCubes[i - 1];
            GameObject currentCube = _bodyCubes[i];

            if (previousCube == null || currentCube == null)
            {
                continue;
            }

            // Put this cube behind the previous cube.
            Vector3 targetPosition =
                previousCube.transform.position -
                direction * bodySpacing;

            // Keep the same Y height.
            targetPosition.y = previousCube.transform.position.y;

            currentCube.transform.position = targetPosition;

            // Same rotation as the previous cube.
            currentCube.transform.rotation =
                previousCube.transform.rotation;
        }
    }


    // ============================================================
    // MERGE ROUTINE
    // ============================================================

    private IEnumerator MergeRoutine()
    {
        _isMerging = true;

        bool merged;

        do
        {
            merged = false;

            // Start checking from the tail.
            for (int i = _values.Count - 1; i > 0; i--)
            {
                int currentValue = _values[i];
                int previousValue = _values[i - 1];

                // Same adjacent values.
                if (currentValue == previousValue)
                {
                    Debug.Log(
                        $"Merge: {previousValue} + {currentValue}"
                    );

                    // Wait so we can see the merge.
                    yield return new WaitForSeconds(mergeDelay);

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


    // ============================================================
    // MERGE TWO SEGMENTS
    // ============================================================

    private void MergeAtIndex(int firstIndex)
    {
        int secondIndex = firstIndex + 1;

        // Double first value.
        _values[firstIndex] *= 2;

        // Cube that will disappear.
        GameObject cubeToDestroy =
            _bodyCubes[secondIndex];

        if (cubeToDestroy != null &&
            cubeToDestroy != gameObject)
        {
            Destroy(cubeToDestroy);
        }

        // Remove second value.
        _values.RemoveAt(secondIndex);

        // Remove second visual cube.
        _bodyCubes.RemoveAt(secondIndex);

        // Update remaining cube number.
        UpdateSegmentVisual(firstIndex);

        // Update head number if head merged.
        if (firstIndex == 0)
        {
            UpdateHeadVisual();
        }

        // Rearrange remaining body.
        UpdateBodyPositions();
    }


    // ============================================================
    // UPDATE SEGMENT TEXT
    // ============================================================

    private void UpdateSegmentVisual(int index)
    {
        if (index < 0 ||
            index >= _bodyCubes.Count)
        {
            return;
        }

        GameObject cube = _bodyCubes[index];

        if (cube == null)
        {
            return;
        }

        TextMeshPro text =
            cube.GetComponentInChildren<TextMeshPro>();

        if (text != null)
        {
            text.text = _values[index].ToString();
        }
    }


    // ============================================================
    // UPDATE HEAD TEXT
    // ============================================================

    private void UpdateHeadVisual()
    {
        if (headText != null)
        {
            headText.text = _values[0].ToString();
        }
    }


    // ============================================================
    // DEBUG
    // ============================================================

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