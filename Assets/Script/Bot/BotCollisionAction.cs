using UnityEngine;

public class BotCollisionAction : MonoBehaviour
{
    private SnakeController _snakeController;
    private BotTargetDetector _targetDetector;


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

        _targetDetector =
            GetComponent<BotTargetDetector>();


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
    // COLLISION
    // =====================================================

    private void OnCollisionEnter(
        Collision collision)
    {
        GameObject target =
            collision.gameObject;


        // Don't collect own body.
        if (target.transform.IsChildOf(transform))
        {
            return;
        }


        int targetLayer =
            target.layer;


        // =================================================
        // PLAYER HEAD
        // =================================================

        if (targetLayer == _playerLayer)
        {
            TryCollectHead(target);

            return;
        }


        // =================================================
        // OTHER BOT HEAD
        // =================================================

        if (targetLayer == _botLayer)
        {
            // Don't collide with itself.
            if (target == gameObject)
                return;


            TryCollectHead(target);

            return;
        }


        // =================================================
        // NORMAL CUBE / BODY
        // =================================================

        bool isWorldCube =
            targetLayer == _cubeLayer;

        bool isPlayerBody =
            targetLayer == _playerBodyLayer;

        bool isBotBody =
            targetLayer == _botBodyLayer;


        if (isWorldCube ||
            isPlayerBody ||
            isBotBody)
        {
            TryCollectCube(target);
        }
    }


    // =====================================================
    // COLLECT NORMAL CUBE
    // =====================================================

    private void TryCollectCube(
        GameObject target)
    {
        NumberCube numberCube =
            target.GetComponent<NumberCube>();


        if (numberCube == null)
            return;


        int value =
            numberCube.CubeValue;


        bool collected =
            _snakeController.CollectValue(
                value,
                target
            );


        if (!collected)
        {
            ClearTarget();

            return;
        }


        PrepareCollectedObject(
            target
        );


        ClearTarget();
    }


    // =====================================================
    // COLLECT PLAYER / BOT HEAD
    // =====================================================

    private void TryCollectHead(GameObject targetHead)
    {
        SnakeController targetSnake =
            targetHead.GetComponent<SnakeController>();

        if (targetSnake == null)
            return;


        int myValue =
            _snakeController.HeadValue;

        int targetValue =
            targetSnake.HeadValue;


        // =============================================
        // SAME VALUE
        // =============================================

        if (myValue == targetValue)
        {
            Debug.Log("Same value. Cannot collect.");

            ClearTarget();

            return;
        }


        // =============================================
        // BOT IS WEAKER
        // =============================================

        if (myValue < targetValue)
        {
            Debug.Log("Bot is weaker.");

            ClearTarget();

            return;
        }


        // =============================================
        // BOT IS STRONGER
        // =============================================

        Debug.Log(
            $"Bot collecting enemy head: {targetValue}"
        );


        // ---------------------------------------------
        // IMPORTANT
        // Disable enemy gameplay scripts FIRST.
        // ---------------------------------------------
        targetSnake.ReleaseBodyOnDefeat(_cubeLayer);
        
        DisableEnemySystems(targetHead);

        // made by Bhargav
        if (targetHead.layer == _botLayer && DynamicBotSpawner.Instance != null) DynamicBotSpawner.Instance.ReportBotDefeated(targetHead);


        // ---------------------------------------------
        // Collect the EXACT enemy head.
        // ---------------------------------------------

        bool collected =
            _snakeController.CollectValue(
                targetValue,
                targetHead
            );


        if (!collected)
        {
            return;
        }


        // ---------------------------------------------
        // Prepare it as bot body.
        // ---------------------------------------------

        PrepareCollectedObject(targetHead);


        ClearTarget();

        Debug.Log(
            "Enemy head successfully collected!"
        );
    }
    
    //
    private void DisableEnemySystems(
        GameObject targetHead)
    {
        // Player movement
        PlayerMovementControl playerMovement =
            targetHead.GetComponent<PlayerMovementControl>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }


        // Bot movement
        BotMovement botMovement =
            targetHead.GetComponent<BotMovement>();

        if (botMovement != null)
        {
            botMovement.enabled = false;
        }


        // Bot target system
        BotTargetDetector detector =
            targetHead.GetComponent<BotTargetDetector>();

        if (detector != null)
        {
            detector.enabled = false;
        }


        // Bot collision
        BotCollisionAction botCollision =
            targetHead.GetComponent<BotCollisionAction>();

        if (botCollision != null)
        {
            botCollision.enabled = false;
        }


        // Player collision
        CollisionAction playerCollision =
            targetHead.GetComponent<CollisionAction>();

        if (playerCollision != null)
        {
            playerCollision.enabled = false;
        }


        // Enemy snake controller
        SnakeController enemySnake =
            targetHead.GetComponent<SnakeController>();

        if (enemySnake != null)
        {
            enemySnake.enabled = false;
        }
    }


    // =====================================================
    // DEFEAT SNAKE
    // =====================================================

    private void DefeatSnake(
        GameObject targetHead)
    {
        // Disable enemy movement.

        PlayerMovementControl playerMovement =
            targetHead.GetComponent<PlayerMovementControl>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }


        BotMovement botMovement =
            targetHead.GetComponent<BotMovement>();

        if (botMovement != null)
        {
            botMovement.enabled = false;
        }


        // Disable target detector.

        BotTargetDetector detector =
            targetHead.GetComponent<BotTargetDetector>();

        if (detector != null)
        {
            detector.enabled = false;
        }


        // Disable collision action.

        BotCollisionAction botCollision =
            targetHead.GetComponent<BotCollisionAction>();

        if (botCollision != null)
        {
            botCollision.enabled = false;
        }


        CollisionAction playerCollision =
            targetHead.GetComponent<CollisionAction>();

        if (playerCollision != null)
        {
            playerCollision.enabled = false;
        }


        // Disable snake controller.

        SnakeController targetSnake =
            targetHead.GetComponent<SnakeController>();

        if (targetSnake != null)
        {
            targetSnake.enabled = false;
        }


        // For testing.

        targetHead.SetActive(false);


        Debug.Log(
            "TARGET DEFEATED!"
        );
    }


    // =====================================================
    // PREPARE COLLECTED CUBE
    // =====================================================

    private void PrepareCollectedObject(
        GameObject target)
    {
        Rigidbody rb =
            target.GetComponent<Rigidbody>();


        if (rb != null)
        {
            rb.isKinematic = true;

            rb.useGravity = false;
        }


        target.layer =
            _botBodyLayer;
    }


    // =====================================================
    // CLEAR TARGET
    // =====================================================

    private void ClearTarget()
    {
        if (_targetDetector != null)
        {
            _targetDetector.ClearTarget();
        }
    }
}