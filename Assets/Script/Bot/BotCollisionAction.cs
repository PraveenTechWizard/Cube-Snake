using Photon.Pun;
using UnityEngine;

public class BotCollisionAction : MonoBehaviourPun
{
    private SnakeController _snakeController;
    private BotTargetDetector _targetDetector;

    private int _cubeLayer;
    private int _playerLayer;
    private int _playerBodyLayer;
    private int _botLayer;
    private int _botBodyLayer;
    

    private void Awake()
    {
        _snakeController = GetComponent<SnakeController>();
        
        _targetDetector = GetComponent<BotTargetDetector>();

        _cubeLayer = LayerMask.NameToLayer("Cube");

        _playerLayer = LayerMask.NameToLayer("Player");

        _playerBodyLayer = LayerMask.NameToLayer("PlayerBody");

        _botLayer = LayerMask.NameToLayer("Bot");

        _botBodyLayer = LayerMask.NameToLayer("BotBody");
    }

    

    private void OnCollisionEnter(
        Collision collision)
    {
        // Only the owner controls this bot
        if (!photonView.IsMine)
            return;


        GameObject target = collision.gameObject;


        if (target == null)
            return;


        // ---------------------------------------------
        // Don't collect own body
        // ---------------------------------------------

        if (target.transform.IsChildOf(transform))
            return;


        int targetLayer = target.layer;

  
        // PLAYER HEAD
        if (targetLayer == _playerLayer)
        {
            TryCollectHead(target);

            return;
        }
        
        // OTHER BOT HEAD
        if (targetLayer == _botLayer)
        {
            if (target == gameObject)
                return;

            TryCollectHead(target);

            return;
        }
        
       
        // NORMAL CUBE / BODY
        bool isWorldCube = targetLayer == _cubeLayer;

        bool isPlayerBody = targetLayer == _playerBodyLayer;

        bool isBotBody = targetLayer == _botBodyLayer;


        if (isWorldCube || isPlayerBody || isBotBody)
        {
            TryCollectCube(target);
        }
    }


   
    // COLLECT CUBE
    private void TryCollectCube(GameObject target)
    {
        if (target == null)
            return;

        NumberCube numberCube = target.GetComponent<NumberCube>();
        
        if (numberCube == null)
            return;

        int value = numberCube.CubeValue;
        
        
        // LOCAL CHECK FIRST
        if (value > _snakeController.HeadValue)
        {
            ClearTarget();

            return;
        }
        
        // ---------------------------------------------
        // REQUEST COLLECTION
        //
        // SnakeController sends the request to Master.
        // Do NOT prepare the object here.
        // The RPC should handle the actual collection.
        // ---------------------------------------------

        bool requested = _snakeController.CollectValue(value, target);
        
        if (!requested)
        {
            ClearTarget();

            return;
        }
        
        ClearTarget();
    }
    
    // COLLECT ENEMY HEAD
    private void TryCollectHead(GameObject targetHead)
    {
        if (targetHead == null)
            return;
        
        SnakeController targetSnake = targetHead.GetComponent<SnakeController>();
        
        if (targetSnake == null)
            return;

        // Don't collect yourself
        if (targetSnake == _snakeController)
            return;
        
        int myValue = _snakeController.HeadValue;

        int targetValue = targetSnake.HeadValue;
        
        // SAME VALUE

        if (myValue == targetValue)
        {
            Debug.Log("Same value. Cannot collect.");

            ClearTarget();

            return;
        }
        
        
        // BOT IS WEAKER
        if (myValue < targetValue)
        {
            Debug.Log("Bot is weaker.");

            ClearTarget();

            return;
        }


       
        // BOT IS STRONGER

        Debug.Log($"Bot requesting enemy head: {targetValue}");
        
        // ---------------------------------------------
        // IMPORTANT
        //
        // DO NOT disable the enemy locally here.
        //
        // Do NOT call ReleaseBodyOnDefeat here.
        //
        // First request the network collection.
        // ---------------------------------------------

        bool requested = _snakeController.CollectValue(targetValue, targetHead);
        
        if (!requested)
        {
            ClearTarget();

            return;
        }
        
        ClearTarget();
    }
    
    // =====================================================
    // PREPARE COLLECTED OBJECT
    //
    // Keep this method if SnakeController needs it later.
    // But do not call it directly from collision.
    // =====================================================

    public void PrepareCollectedObject(
        GameObject target)
    {
        if (target == null)
            return;
        
        Rigidbody rb = target.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;

            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;

            rb.useGravity = false;
        }
        
        target.layer = _botBodyLayer;
    }


    // =====================================================
    // DISABLE ENEMY SYSTEMS
    //
    // This should be called through a network RPC,
    // not directly from collision.
    // =====================================================

    public void DisableEnemySystems(GameObject targetHead)
    {
        if (targetHead == null)
            return;


        PlayerMovementControl playerMovement = targetHead.GetComponent<PlayerMovementControl>();


        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }


        BotMovement botMovement = targetHead.GetComponent<BotMovement>();


        if (botMovement != null)
        {
            botMovement.enabled = false;
        }


        BotTargetDetector detector = targetHead.GetComponent<BotTargetDetector>();


        if (detector != null)
        {
            detector.enabled = false;
        }
        
        BotCollisionAction botCollision = targetHead.GetComponent<BotCollisionAction>();
        
        if (botCollision != null)
        {
            botCollision.enabled = false;
        }


        CollisionAction playerCollision = targetHead.GetComponent<CollisionAction>();
        
        if (playerCollision != null)
        {
            playerCollision.enabled = false;
        }
        
        SnakeController enemySnake = targetHead.GetComponent<SnakeController>();


        if (enemySnake != null)
        {
            enemySnake.enabled = false;
        }
    }


    
    // CLEAR TARGET
    private void ClearTarget()
    {
        if (_targetDetector != null)
        {
            _targetDetector.ClearTarget();
        }
    }
}