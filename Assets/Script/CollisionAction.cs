using Photon.Pun;
using UnityEngine;

public class CollisionAction : MonoBehaviourPun
{
    private SnakeController _snakeController;

    private int _cubeLayer;
    private int _botLayer;
    private int _botBodyLayer;
    private int _playerBodyLayer;


    private void Awake()
    {
        _snakeController =
            GetComponent<SnakeController>();


        _cubeLayer =
            LayerMask.NameToLayer("Cube");

        _botLayer =
            LayerMask.NameToLayer("Bot");

        _botBodyLayer =
            LayerMask.NameToLayer("BotBody");

        _playerBodyLayer =
            LayerMask.NameToLayer("PlayerBody");
    }


    private void OnCollisionEnter(
        Collision collision)
    {
        // =================================================
        // PHOTON OWNERSHIP CHECK
        // =================================================
        //
        // Only the player who owns this object
        // can process its collisions.
        //
        if (!photonView.IsMine)
            return;


        GameObject target =
            collision.gameObject;


        // =================================================
        // DON'T COLLECT OWN BODY
        // =================================================

        if (target.transform.IsChildOf(transform))
            return;


        // =================================================
        // BOT HEAD
        // =================================================

        if (target.layer == _botLayer)
        {
            TryCollectBotHead(target);

            return;
        }


        // =================================================
        // WORLD CUBE / BOT BODY
        // =================================================

        bool isWorldCube =
            target.layer == _cubeLayer;


        bool isBotBody =
            target.layer == _botBodyLayer;


        if (!isWorldCube &&
            !isBotBody)
        {
            return;
        }


        TryCollectCube(target);
    }


    // =========================================================
    // COLLECT NORMAL CUBE
    // =========================================================

    private void TryCollectCube(
        GameObject target)
    {
        NumberCube numberCube =
            target.GetComponent<NumberCube>();


        if (numberCube == null)
            return;


        bool collected =
            _snakeController.CollectValue(
                numberCube.CubeValue,
                target
            );


        if (!collected)
            return;


        PrepareCollectedObject(
            target
        );
    }


    // =========================================================
    // COLLECT BOT HEAD
    // =========================================================

    private void TryCollectBotHead(
        GameObject botHead)
    {
        SnakeController botSnake =
            botHead.GetComponent<SnakeController>();


        if (botSnake == null)
        {
            Debug.LogWarning(
                "Bot head does not have SnakeController!"
            );

            return;
        }


        int playerValue =
            _snakeController.HeadValue;


        int botValue =
            botSnake.HeadValue;


        Debug.Log(
            $"PLAYER = {playerValue} | " +
            $"BOT = {botValue}"
        );


        // =================================================
        // SAME VALUE
        // =================================================

        if (playerValue == botValue)
        {
            Debug.Log(
                "Same value. Cannot collect."
            );

            return;
        }


        // =================================================
        // BOT IS STRONGER
        // =================================================

        if (playerValue < botValue)
        {
            Debug.Log(
                "Bot is stronger. " +
                "Player cannot collect."
            );

            return;
        }


        // =================================================
        // PLAYER IS STRONGER
        // =================================================

        Debug.Log(
            "PLAYER COLLECTED BOT HEAD!"
        );


        // Release bot body first.
        ReleaseSnakeBody(
            botHead
        );


        // Disable bot gameplay scripts.
        DisableBotSystems(
            botHead
        );


        // Collect exact bot head.
        bool collected =
            _snakeController.CollectValue(
                botValue,
                botHead
            );


        if (!collected)
            return;


        PrepareCollectedObject(
            botHead
        );
    }


    // =========================================================
    // RELEASE BOT BODY
    // =========================================================

    private void ReleaseSnakeBody(
        GameObject snakeHead)
    {
        SnakeController snake =
            snakeHead.GetComponent<
                SnakeController
            >();


        if (snake == null)
            return;


        Transform[] children =
            snakeHead.GetComponentsInChildren<
                Transform
            >();


        foreach (Transform child in children)
        {
            if (child == snakeHead.transform)
                continue;


            NumberCube numberCube =
                child.GetComponent<
                    NumberCube
                >();


            if (numberCube != null)
            {
                child.SetParent(null);
            }
        }
    }


    // =========================================================
    // DISABLE BOT SYSTEMS
    // =========================================================

    private void DisableBotSystems(
        GameObject botHead)
    {
        BotMovement movement =
            botHead.GetComponent<
                BotMovement
            >();


        if (movement != null)
        {
            movement.enabled = false;
        }


        BotTargetDetector detector =
            botHead.GetComponent<
                BotTargetDetector
            >();


        if (detector != null)
        {
            detector.enabled = false;
        }


        BotCollisionAction collisionAction =
            botHead.GetComponent<
                BotCollisionAction
            >();


        if (collisionAction != null)
        {
            collisionAction.enabled =
                false;
        }


        SnakeController snakeController =
            botHead.GetComponent<
                SnakeController
            >();


        if (snakeController != null)
        {
            snakeController.enabled =
                false;
        }
    }


    // =========================================================
    // PREPARE COLLECTED OBJECT
    // =========================================================

    private void PrepareCollectedObject(
        GameObject target)
    {
        Rigidbody rb =
            target.GetComponent<
                Rigidbody
            >();


        if (rb != null)
        {
            rb.isKinematic =
                true;

            rb.useGravity =
                false;
        }


        target.layer =
            _playerBodyLayer;
    }
}