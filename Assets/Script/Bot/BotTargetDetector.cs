using UnityEngine;

public class BotTargetDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 30f;
    [SerializeField] private float targetRefreshTime = 0.2f;

    [Header("Random Movement")]
    [SerializeField] private float randomDirectionTime = 2f;

    private SnakeController _snakeController;
    private BotMovement _botMovement;

    private GameObject _currentTarget;

    private float _nextTargetCheck;
    private float _nextRandomDirection;

    private void Awake()
    {
        _snakeController = GetComponent<SnakeController>();
        _botMovement = GetComponent<BotMovement>();
    }

    private void Start()
    {
        ChooseNewRandomDirection();
    }

    private void Update()
    {
        // Check target frequently.
        if (Time.time >= _nextTargetCheck)
        {
            _nextTargetCheck =
                Time.time + targetRefreshTime;

            FindBestTarget();
        }

        // If there is no valid target,
        // move randomly around the map.
        if (_currentTarget == null)
        {
            RandomMovement();
            return;
        }

        // Make sure target is still valid.
        if (!IsTargetValid(_currentTarget))
        {
            _currentTarget = null;
            return;
        }

        // Move toward target.
        MoveTowardsTarget();
    }
    
    //
    public void ClearTarget()
    {
        _currentTarget = null;
    }

    // =========================================================
    // FIND TARGET
    // =========================================================

    private void FindBestTarget()
    {
        Collider[] objects =
            Physics.OverlapSphere(
                transform.position,
                detectionRadius
            );

        GameObject bestTarget = null;

        float bestDistance = Mathf.Infinity;

        foreach (Collider current in objects)
        {
            if (!current.CompareTag("Cube"))
                continue;

            NumberCube numberCube =
                current.GetComponent<NumberCube>();

            if (numberCube == null)
                continue;

            int cubeValue =
                numberCube.CubeValue;

            // VERY IMPORTANT:
            // Bot cannot collect a bigger cube.
            if (cubeValue > _snakeController.HeadValue)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    current.transform.position
                );

            // Find nearest valid cube.
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = current.gameObject;
            }
        }

        _currentTarget = bestTarget;
    }

    // =========================================================
    // TARGET VALIDATION
    // =========================================================

    private bool IsTargetValid(GameObject target)
    {
        if (target == null)
            return false;

        if (!target.CompareTag("Cube"))
            return false;

        NumberCube numberCube =
            target.GetComponent<NumberCube>();

        if (numberCube == null)
            return false;

        // Target became too powerful after bot's state changed.
        if (numberCube.CubeValue >
            _snakeController.HeadValue)
        {
            return false;
        }

        return true;
    }

    // =========================================================
    // MOVE TO TARGET
    // =========================================================

    private void MoveTowardsTarget()
    {
        Vector3 direction =
            _currentTarget.transform.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.1f)
            return;

        _botMovement.SetMoveDirection(
            direction.normalized
        );
    }

    // =========================================================
    // RANDOM MOVEMENT
    // =========================================================

    private void RandomMovement()
    {
        if (Time.time >= _nextRandomDirection)
        {
            ChooseNewRandomDirection();
        }
    }

    private void ChooseNewRandomDirection()
    {
        Vector2 random =
            Random.insideUnitCircle.normalized;

        Vector3 direction =
            new Vector3(
                random.x,
                0f,
                random.y
            );

        _botMovement.SetMoveDirection(direction);

        _nextRandomDirection =
            Time.time + randomDirectionTime;
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius
        );

        if (_currentTarget != null)
        {
            Gizmos.DrawLine(
                transform.position,
                _currentTarget.transform.position
            );
        }
    }
}