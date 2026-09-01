using UnityEngine;

public class BotTargetDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 30f;
    [SerializeField] private float targetRefreshTime = 0.2f;

    [Header("Random Movement")]
    [SerializeField] private float randomDirectionTime = 2f;

    [Header("Anti Circle")]
    [SerializeField] private float minimumTargetDistance = 1.5f;
    [SerializeField] private float progressCheckTime = 0.6f;
    [SerializeField] private float minimumProgress = 0.25f;
    [SerializeField] private float ignoredTargetDuration = 1.5f;
    [SerializeField] private float escapeTurnAngle = 70f;

    [Header("Performance")]
    [SerializeField] private int detectionBufferSize = 64;

    private SnakeController _snakeController;
    private BotMovement _botMovement;

    private GameObject _currentTarget;

    private float _nextTargetCheck;
    private float _nextRandomDirection;

    private int _cubeLayer;
    private int _playerLayer;
    private int _playerBodyLayer;
    private int _botLayer;
    private int _botBodyLayer;

    // made by Bhargav
    private Collider[] _detectionBuffer;

    // made by Bhargav
    private GameObject _ignoredTarget;

    // made by Bhargav
    private float _ignoredTargetUntil;

    // made by Bhargav
    private float _lastTargetDistance = Mathf.Infinity;

    // made by Bhargav
    private float _nextProgressCheck;

    private void Awake()
    {
        _snakeController = GetComponent<SnakeController>();
        _botMovement = GetComponent<BotMovement>();

        _cubeLayer = LayerMask.NameToLayer("Cube");
        _playerLayer = LayerMask.NameToLayer("Player");
        _playerBodyLayer = LayerMask.NameToLayer("PlayerBody");
        _botLayer = LayerMask.NameToLayer("Bot");
        _botBodyLayer = LayerMask.NameToLayer("BotBody");

        // made by Bhargav
        _detectionBuffer = new Collider[Mathf.Max(16, detectionBufferSize)];
    }

    private void Start()
    {
        ChooseNewRandomDirection();
    }

    private void Update()
    {
        if (Time.time >= _nextTargetCheck)
        {
            _nextTargetCheck = Time.time + targetRefreshTime;
            FindBestTarget();
        }

        if (_currentTarget == null)
        {
            RandomMovement();
            return;
        }

        if (!IsTargetValid(_currentTarget))
        {
            ClearTarget();
            return;
        }

        // made by Bhargav
        if (CheckTargetProgress()) return;

        MoveTowardsTarget();
    }

    private void FindBestTarget()
    {
        // made by Bhargav
        int objectCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, _detectionBuffer);

        GameObject bestTarget = null;

        float bestDistanceSqr = Mathf.Infinity;

        for (int i = 0; i < objectCount; i++)
        {
            Collider current = _detectionBuffer[i];

            if (current == null) continue;

            GameObject target = current.gameObject;

            if (target == gameObject) continue;
            if (target.transform.IsChildOf(transform)) continue;

            // made by Bhargav
            if (target == _ignoredTarget && Time.time < _ignoredTargetUntil) continue;

            int targetLayer = target.layer;

            bool isWorldCube = targetLayer == _cubeLayer;
            bool isPlayerHead = targetLayer == _playerLayer;
            bool isPlayerBody = targetLayer == _playerBodyLayer;
            bool isBotHead = targetLayer == _botLayer;
            bool isBotBody = targetLayer == _botBodyLayer;

            if (!isWorldCube && !isPlayerHead && !isPlayerBody && !isBotHead && !isBotBody) continue;

            if (isPlayerHead || isBotHead)
            {
                SnakeController targetSnake = target.GetComponent<SnakeController>();

                if (targetSnake == null) continue;
                if (_snakeController.HeadValue <= targetSnake.HeadValue) continue;
            }
            else
            {
                NumberCube numberCube = target.GetComponent<NumberCube>();

                if (numberCube == null) continue;
                if (numberCube.CubeValue > _snakeController.HeadValue) continue;
            }

            Vector3 difference = target.transform.position - transform.position;

            difference.y = 0f;

            float distanceSqr = difference.sqrMagnitude;

            if (distanceSqr >= bestDistanceSqr) continue;

            bestDistanceSqr = distanceSqr;
            bestTarget = target;
        }

        // made by Bhargav
        if (_currentTarget != bestTarget)
        {
            _currentTarget = bestTarget;
            _lastTargetDistance = Mathf.Infinity;
            _nextProgressCheck = Time.time + progressCheckTime;
        }
    }

    private bool IsTargetValid(GameObject target)
    {
        if (target == null) return false;
        if (target == gameObject) return false;
        if (target.transform.IsChildOf(transform)) return false;
        if (target == _ignoredTarget && Time.time < _ignoredTargetUntil) return false;

        int targetLayer = target.layer;

        bool isWorldCube = targetLayer == _cubeLayer;
        bool isPlayerHead = targetLayer == _playerLayer;
        bool isPlayerBody = targetLayer == _playerBodyLayer;
        bool isBotHead = targetLayer == _botLayer;
        bool isBotBody = targetLayer == _botBodyLayer;

        if (isPlayerHead || isBotHead)
        {
            SnakeController targetSnake = target.GetComponent<SnakeController>();

            if (targetSnake == null) return false;

            return _snakeController.HeadValue > targetSnake.HeadValue;
        }

        if (isWorldCube || isPlayerBody || isBotBody)
        {
            NumberCube numberCube = target.GetComponent<NumberCube>();

            if (numberCube == null) return false;

            return numberCube.CubeValue <= _snakeController.HeadValue;
        }

        return false;
    }

    private void MoveTowardsTarget()
    {
        if (_currentTarget == null) return;

        Vector3 direction = _currentTarget.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        _botMovement.SetMoveDirection(direction.normalized);
    }

    // made by Bhargav
    private bool CheckTargetProgress()
    {
        if (_currentTarget == null) return false;

        Vector3 difference = _currentTarget.transform.position - transform.position;

        difference.y = 0f;

        float currentDistance = difference.magnitude;

        if (currentDistance <= minimumTargetDistance)
        {
            EscapeTarget();
            return true;
        }

        if (Time.time < _nextProgressCheck) return false;

        _nextProgressCheck = Time.time + progressCheckTime;

        if (_lastTargetDistance < Mathf.Infinity && currentDistance >= _lastTargetDistance - minimumProgress)
        {
            EscapeTarget();
            return true;
        }

        _lastTargetDistance = currentDistance;

        return false;
    }

    // made by Bhargav
    private void EscapeTarget()
    {
        if (_currentTarget != null)
        {
            _ignoredTarget = _currentTarget;
            _ignoredTargetUntil = Time.time + ignoredTargetDuration;
        }

        float turnDirection = Random.value < 0.5f ? -1f : 1f;

        Vector3 escapeDirection = Quaternion.Euler(0f, escapeTurnAngle * turnDirection, 0f) * transform.forward;

        _currentTarget = null;
        _lastTargetDistance = Mathf.Infinity;
        _nextProgressCheck = 0f;

        _botMovement.SetMoveDirection(escapeDirection.normalized);

        _nextRandomDirection = Time.time + randomDirectionTime;
    }

    private void RandomMovement()
    {
        if (Time.time >= _nextRandomDirection) ChooseNewRandomDirection();
    }

    private void ChooseNewRandomDirection()
    {
        // made by Bhargav
        float randomAngle = Random.Range(-65f, 65f);

        // made by Bhargav
        Vector3 direction = Quaternion.Euler(0f, randomAngle, 0f) * transform.forward;

        _botMovement.SetMoveDirection(direction.normalized);

        _nextRandomDirection = Time.time + randomDirectionTime;
    }

    public void ClearTarget()
    {
        _currentTarget = null;

        // made by Bhargav
        _lastTargetDistance = Mathf.Infinity;
        _nextProgressCheck = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (_currentTarget != null) Gizmos.DrawLine(transform.position, _currentTarget.transform.position);
    }
}