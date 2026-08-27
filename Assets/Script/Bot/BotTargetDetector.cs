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


    private int _cubeLayer;
    private int _playerBodyLayer;
    private int _botLayer;
    private int _botBodyLayer;


    private void Awake()
    {
        _snakeController =
            GetComponent<SnakeController>();

        _botMovement =
            GetComponent<BotMovement>();


        _cubeLayer =
            LayerMask.NameToLayer("Cube");

        _playerBodyLayer =
            LayerMask.NameToLayer("PlayerBody");

        _botLayer =
            LayerMask.NameToLayer("Bot");

        _botBodyLayer =
            LayerMask.NameToLayer("BotBody");
    }


    private void Start()
    {
        ChooseNewRandomDirection();
    }


    private void Update()
    {
        if (Time.time >= _nextTargetCheck)
        {
            _nextTargetCheck =
                Time.time + targetRefreshTime;

            FindBestTarget();
        }


        if (_currentTarget == null)
        {
            RandomMovement();
            return;
        }


        if (!IsTargetValid(_currentTarget))
        {
            _currentTarget = null;
            return;
        }


        MoveTowardsTarget();
    }


    // =====================================================
    // FIND BEST TARGET
    // =====================================================

    private void FindBestTarget()
    {
        Collider[] objects =
            Physics.OverlapSphere(
                transform.position,
                detectionRadius
            );


        GameObject bestTarget = null;

        float bestDistance =
            Mathf.Infinity;


        foreach (Collider current in objects)
        {
            GameObject targetObject =
                current.gameObject;


            // ---------------------------------------------
            // NEVER TARGET YOURSELF
            // ---------------------------------------------

            if (targetObject == gameObject)
                continue;


            // Don't target anything inside this bot.
            if (targetObject.transform.IsChildOf(transform))
                continue;


            int targetLayer =
                targetObject.layer;


            // ---------------------------------------------
            // ONLY TARGET:
            //
            // World Cube
            // Player Body
            // ---------------------------------------------

            bool isWorldCube =
                targetLayer == _cubeLayer;

            bool isPlayerBody =
                targetLayer == _playerBodyLayer;


            if (!isWorldCube &&
                !isPlayerBody)
            {
                continue;
            }


            // ---------------------------------------------
            // GET NUMBER VALUE
            // ---------------------------------------------

            NumberCube numberCube =
                targetObject.GetComponent<NumberCube>();


            if (numberCube == null)
                continue;


            int cubeValue =
                numberCube.CubeValue;


            // ---------------------------------------------
            // BOT CANNOT COLLECT BIGGER VALUE
            // ---------------------------------------------

            if (cubeValue >
                _snakeController.HeadValue)
            {
                continue;
            }


            // ---------------------------------------------
            // FIND NEAREST
            // ---------------------------------------------

            float distance =
                Vector3.Distance(
                    transform.position,
                    targetObject.transform.position
                );


            if (distance < bestDistance)
            {
                bestDistance =
                    distance;

                bestTarget =
                    targetObject;
            }
        }


        _currentTarget =
            bestTarget;
    }


    // =====================================================
    // TARGET VALIDATION
    // =====================================================

    private bool IsTargetValid(
        GameObject target)
    {
        if (target == null)
            return false;


        // Don't target own body.
        if (target.transform.IsChildOf(transform))
            return false;


        int targetLayer =
            target.layer;


        bool isWorldCube =
            targetLayer == _cubeLayer;


        bool isPlayerBody =
            targetLayer == _playerBodyLayer;


        if (!isWorldCube &&
            !isPlayerBody)
        {
            return false;
        }


        NumberCube numberCube =
            target.GetComponent<NumberCube>();


        if (numberCube == null)
            return false;


        // Check value again.
        if (numberCube.CubeValue >
            _snakeController.HeadValue)
        {
            return false;
        }


        return true;
    }


    // =====================================================
    // MOVE TO TARGET
    // =====================================================

    private void MoveTowardsTarget()
    {
        if (_currentTarget == null)
            return;


        Vector3 direction =
            _currentTarget.transform.position -
            transform.position;


        direction.y = 0f;


        if (direction.sqrMagnitude < 0.01f)
            return;


        _botMovement.SetMoveDirection(
            direction.normalized
        );
    }


    // =====================================================
    // RANDOM MOVEMENT
    // =====================================================

    private void RandomMovement()
    {
        if (Time.time >=
            _nextRandomDirection)
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


        _botMovement.SetMoveDirection(
            direction
        );


        _nextRandomDirection =
            Time.time +
            randomDirectionTime;
    }


    // =====================================================
    // CLEAR TARGET
    // =====================================================

    public void ClearTarget()
    {
        _currentTarget = null;
    }


    // =====================================================
    // DEBUG
    // =====================================================

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