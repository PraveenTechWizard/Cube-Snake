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
    private int _playerLayer;
    private int _playerBodyLayer;
    private int _botLayer;
    private int _botBodyLayer;


    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        _snakeController =
            GetComponent<SnakeController>();

        _botMovement =
            GetComponent<BotMovement>();


        _cubeLayer =
            LayerMask.NameToLayer("Cube");

        _playerLayer =
            LayerMask.NameToLayer("Player");

        _playerBodyLayer =
            LayerMask.NameToLayer("PlayerBody");

        _botLayer =
            LayerMask.NameToLayer("Bot");

        _botBodyLayer =
            LayerMask.NameToLayer("BotBody");
    }


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        ChooseNewRandomDirection();
    }


    // =====================================================
    // UPDATE
    // =====================================================

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
            GameObject target =
                current.gameObject;


            // ---------------------------------------------
            // DON'T TARGET SELF
            // ---------------------------------------------

            if (target == gameObject)
                continue;


            // ---------------------------------------------
            // DON'T TARGET OWN BODY
            // ---------------------------------------------

            if (target.transform.IsChildOf(transform))
                continue;


            // ---------------------------------------------
            // GET LAYER
            // ---------------------------------------------

            int targetLayer =
                target.layer;


            bool isWorldCube =
                targetLayer == _cubeLayer;

            bool isPlayerHead =
                targetLayer == _playerLayer;

            bool isPlayerBody =
                targetLayer == _playerBodyLayer;

            bool isBotHead =
                targetLayer == _botLayer;

            bool isBotBody =
                targetLayer == _botBodyLayer;


            // ---------------------------------------------
            // ONLY TARGET VALID OBJECTS
            // ---------------------------------------------

            if (!isWorldCube &&
                !isPlayerHead &&
                !isPlayerBody &&
                !isBotHead &&
                !isBotBody)
            {
                continue;
            }


            // =================================================
            // PLAYER HEAD OR OTHER BOT HEAD
            // =================================================

            if (isPlayerHead || isBotHead)
            {
                SnakeController targetSnake =
                    target.GetComponent<SnakeController>();


                if (targetSnake == null)
                    continue;


                // Must be strictly stronger.
                //
                // Bot = 8
                // Target = 4  -> YES
                //
                // Bot = 8
                // Target = 8  -> NO
                //
                // Bot = 4
                // Target = 8  -> NO

                if (_snakeController.HeadValue <=
                    targetSnake.HeadValue)
                {
                    continue;
                }
            }


            // =================================================
            // NORMAL BODY / WORLD CUBE
            // =================================================

            else
            {
                NumberCube numberCube =
                    target.GetComponent<NumberCube>();


                if (numberCube == null)
                    continue;


                if (numberCube.CubeValue >
                    _snakeController.HeadValue)
                {
                    continue;
                }
            }


            // =================================================
            // DISTANCE
            // =================================================

            float distance =
                Vector3.Distance(
                    transform.position,
                    target.transform.position
                );


            if (distance < bestDistance)
            {
                bestDistance =
                    distance;

                bestTarget =
                    target;
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


        if (target == gameObject)
            return false;


        // Don't target own body.
        if (target.transform.IsChildOf(transform))
            return false;


        int targetLayer =
            target.layer;


        bool isWorldCube =
            targetLayer == _cubeLayer;

        bool isPlayerHead =
            targetLayer == _playerLayer;

        bool isPlayerBody =
            targetLayer == _playerBodyLayer;

        bool isBotHead =
            targetLayer == _botLayer;

        bool isBotBody =
            targetLayer == _botBodyLayer;


        // =================================================
        // HEAD TARGET
        // =================================================

        if (isPlayerHead ||
            isBotHead)
        {
            SnakeController targetSnake =
                target.GetComponent<SnakeController>();


            if (targetSnake == null)
                return false;


            return _snakeController.HeadValue >
                   targetSnake.HeadValue;
        }


        // =================================================
        // BODY / WORLD CUBE
        // =================================================

        if (isWorldCube ||
            isPlayerBody ||
            isBotBody)
        {
            NumberCube numberCube =
                target.GetComponent<NumberCube>();


            if (numberCube == null)
                return false;


            return numberCube.CubeValue <=
                   _snakeController.HeadValue;
        }


        return false;
    }


    // =====================================================
    // MOVE TOWARDS TARGET
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