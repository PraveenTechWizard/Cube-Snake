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

    [Header("Snake Follow Settings")]
    [SerializeField] private float pathPointDistance = 0.08f;
    [SerializeField] private float bodyFollowSpeed = 18f;
    [SerializeField] private float bodyRotationSpeed = 14f;
    [SerializeField] private int pathBufferPoints = 20;

    private readonly List<int> _values = new List<int>();
    private readonly List<GameObject> _bodyCubes = new List<GameObject>();

    // made by Bhargav
    private readonly List<Vector3> _movementPath = new List<Vector3>(256);

    private bool _isMerging;

    // made by Bhargav
    private Vector3 _lastRecordedPosition;

    // made by Bhargav
    private int _pathTrimThreshold = 64;

    public int HeadValue
    {
        get
        {
            if (_values.Count == 0) return 0;

            return _values[0];
        }
    }

    private void Awake()
    {
        _values.Add(startingValue);

        _bodyCubes.Add(gameObject);

        // made by Bhargav
        _movementPath.Add(transform.position);
        _lastRecordedPosition = transform.position;
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

    // made by Bhargav
    private void LateUpdate()
    {
        RecordHeadPath();
        FollowHeadPath();
    }

    public bool CollectValue(int value, GameObject collectedCube)
    {
        if (collectedCube == null) return false;

        if (value > HeadValue)
        {
            Debug.Log($"Cannot collect {value}. Head = {HeadValue}");

            return false;
        }

        SnakeController oldOwner = GetOwnerOfCube(collectedCube);

        if (oldOwner != null && oldOwner != this)
        {
            bool removed = oldOwner.RemoveBodyCube(collectedCube);

            if (!removed) return false;
        }

        if (_bodyCubes.Contains(collectedCube)) return false;

        _values.Add(value);

        _bodyCubes.Add(collectedCube);

        collectedCube.transform.SetParent(transform, true);

        SortSnakeByValue();

        UpdateAllVisuals();

        PrintSnake();

        if (!_isMerging) StartCoroutine(MergeRoutine());

        return true;
    }

    private SnakeController GetOwnerOfCube(GameObject cube)
    {
        Transform currentParent = cube.transform.parent;

        while (currentParent != null)
        {
            SnakeController snake = currentParent.GetComponent<SnakeController>();

            if (snake != null) return snake;

            currentParent = currentParent.parent;
        }

        return null;
    }

    public bool RemoveBodyCube(GameObject cube)
    {
        int index = _bodyCubes.IndexOf(cube);

        if (index == -1) return false;

        if (index == 0) return false;

        _bodyCubes.RemoveAt(index);

        _values.RemoveAt(index);

        cube.transform.SetParent(null, true);

        SortSnakeByValue();

        UpdateAllVisuals();

        PrintSnake();

        return true;
    }

    private void SortSnakeByValue()
    {
        for (int i = 1; i < _values.Count - 1; i++)
        {
            for (int j = i + 1; j < _values.Count; j++)
            {
                if (_values[j] > _values[i])
                {
                    int tempValue = _values[i];

                    _values[i] = _values[j];

                    _values[j] = tempValue;

                    GameObject tempCube = _bodyCubes[i];

                    _bodyCubes[i] = _bodyCubes[j];

                    _bodyCubes[j] = tempCube;
                }
            }
        }

        UpdateBodyPositions();
    }

    private void UpdateBodyPositions()
    {
        // made by Bhargav
        if (_bodyCubes.Count <= 1 || _movementPath.Count == 0) return;

        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            GameObject cube = _bodyCubes[i];

            if (cube == null) continue;

            Vector3 targetPosition;
            Vector3 targetDirection;

            GetFollowPoint(i, out targetPosition, out targetDirection);

            targetPosition.y = 0.5f;

            cube.transform.position = targetPosition;

            targetDirection.y = 0f;

            if (targetDirection.sqrMagnitude > 0.001f) cube.transform.rotation = Quaternion.LookRotation(targetDirection);
        }
    }

    // made by Bhargav
    private void RecordHeadPath()
    {
        if (pathPointDistance <= 0.001f) pathPointDistance = 0.001f;

        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - _lastRecordedPosition;

        movement.y = 0f;

        float distance = movement.magnitude;

        if (distance < pathPointDistance) return;

        Vector3 direction = movement / distance;

        while (distance >= pathPointDistance)
        {
            _lastRecordedPosition += direction * pathPointDistance;
            _lastRecordedPosition.y = currentPosition.y;

            _movementPath.Add(_lastRecordedPosition);

            distance -= pathPointDistance;
        }

        TrimMovementPath();
    }

    // made by Bhargav
    private void TrimMovementPath()
    {
        float requiredDistance = (_bodyCubes.Count + pathBufferPoints) * bodySpacing;

        int requiredPoints = Mathf.CeilToInt(requiredDistance / pathPointDistance) + 2;

        int excessPoints = _movementPath.Count - requiredPoints;

        if (excessPoints < _pathTrimThreshold) return;

        _movementPath.RemoveRange(0, excessPoints);
    }

    // made by Bhargav
    private void FollowHeadPath()
    {
        if (_bodyCubes.Count <= 1 || _movementPath.Count < 2) return;

        float positionLerp = Mathf.Clamp01(bodyFollowSpeed * Time.deltaTime);
        float rotationLerp = Mathf.Clamp01(bodyRotationSpeed * Time.deltaTime);

        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            GameObject cube = _bodyCubes[i];

            if (cube == null) continue;

            Vector3 targetPosition;
            Vector3 targetDirection;

            GetFollowPoint(i, out targetPosition, out targetDirection);

            targetPosition.y = 0.5f;

            cube.transform.position = Vector3.Lerp(cube.transform.position, targetPosition, positionLerp);

            targetDirection.y = 0f;

            if (targetDirection.sqrMagnitude <= 0.001f) continue;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            cube.transform.rotation = Quaternion.Slerp(cube.transform.rotation, targetRotation, rotationLerp);
        }
    }

    // made by Bhargav
    private void GetFollowPoint(int bodyIndex, out Vector3 targetPosition, out Vector3 targetDirection)
    {
        float distanceBehind = bodySpacing * bodyIndex;
        float pointOffset = distanceBehind / pathPointDistance;

        int newestIndex = _movementPath.Count - 1;

        float targetIndex = newestIndex - pointOffset;

        if (targetIndex <= 0f)
        {
            targetPosition = _movementPath[0];

            if (_movementPath.Count > 1) targetDirection = _movementPath[1] - _movementPath[0];
            else targetDirection = transform.forward;

            return;
        }

        int lowerIndex = Mathf.FloorToInt(targetIndex);
        int upperIndex = lowerIndex + 1;

        if (upperIndex > newestIndex) upperIndex = newestIndex;

        float interpolation = targetIndex - lowerIndex;

        Vector3 lowerPoint = _movementPath[lowerIndex];
        Vector3 upperPoint = _movementPath[upperIndex];

        targetPosition = Vector3.Lerp(lowerPoint, upperPoint, interpolation);

        targetDirection = upperPoint - lowerPoint;

        if (targetDirection.sqrMagnitude <= 0.001f) targetDirection = transform.forward;
    }

    public List<GameObject> GetBodyCubes()
    {
        return _bodyCubes;
    }

    // made by Bhargav
    public void ReleaseBodyOnDefeat(int worldCubeLayer)
    {
        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            GameObject cube = _bodyCubes[i];

            if (cube == null) continue;

            cube.transform.SetParent(null, true);
            cube.layer = worldCubeLayer;

            Rigidbody rb = cube.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        if (_bodyCubes.Count > 1) _bodyCubes.RemoveRange(1, _bodyCubes.Count - 1);
        if (_values.Count > 1) _values.RemoveRange(1, _values.Count - 1);
    }
    private IEnumerator MergeRoutine()
    {
        _isMerging = true;

        while (true)
        {
            bool foundMerge = false;

            for (int i = _values.Count - 1; i > 0; i--)
            {
                if (_values[i] != _values[i - 1]) continue;

                foundMerge = true;

                GameObject firstCube = _bodyCubes[i - 1];

                GameObject secondCube = _bodyCubes[i];

                Debug.Log($"Merge: {_values[i - 1]} + {_values[i]}");

                yield return new WaitForSeconds(mergeDelay);

                int firstIndex = _bodyCubes.IndexOf(firstCube);

                int secondIndex = _bodyCubes.IndexOf(secondCube);

                if (firstIndex == -1 || secondIndex == -1) break;

                if (secondIndex != firstIndex + 1) break;

                if (_values[firstIndex] != _values[secondIndex]) break;

                MergeAtIndex(firstIndex);

                break;
            }

            if (!foundMerge) break;
        }

        UpdateAllVisuals();

        PrintSnake();

        _isMerging = false;
    }

    private void MergeAtIndex(int firstIndex)
    {
        int secondIndex = firstIndex + 1;

        if (firstIndex < 0 || secondIndex >= _values.Count) return;

        _values[firstIndex] *= 2;

        GameObject cubeToDestroy = _bodyCubes[secondIndex];

        _values.RemoveAt(secondIndex);

        _bodyCubes.RemoveAt(secondIndex);

        if (cubeToDestroy != null && cubeToDestroy != gameObject) Destroy(cubeToDestroy);

        SortSnakeByValue();

        UpdateAllVisuals();

        UpdateBodyPositions();
    }

    private void UpdateAllVisuals()
    {
        UpdateHeadVisual();

        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            UpdateSegmentVisual(i);
        }
    }

    private void UpdateSegmentVisual(int index)
    {
        if (index < 0 || index >= _bodyCubes.Count) return;

        GameObject cube = _bodyCubes[index];

        if (cube == null) return;

        TextMeshPro text = cube.GetComponentInChildren<TextMeshPro>();

        if (text != null) text.text = _values[index].ToString();

        NumberCube numberCube = cube.GetComponent<NumberCube>();

        if (numberCube != null) numberCube.SetValue(_values[index]);
    }

    private void UpdateHeadVisual()
    {
        if (_values.Count == 0) return;

        if (headText != null) headText.text = _values[0].ToString();

        NumberCube numberCube = GetComponent<NumberCube>();

        if (numberCube != null) numberCube.SetValue(_values[0]);
    }

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