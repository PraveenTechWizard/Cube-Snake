using Photon.Pun;
using UnityEngine;

public class BotCollisionAction : MonoBehaviourPun
{
    private SnakeController _snakeController;

    private int _playerLayer;
    private int _botLayer;
    private int _cubeLayer;
    private int _playerBodyLayer;
    private int _botBodyLayer;

    private void Awake()
    {
        _snakeController = GetComponent<SnakeController>();

        _playerLayer = LayerMask.NameToLayer("Player");

        _botLayer = LayerMask.NameToLayer("Bot");

        _cubeLayer = LayerMask.NameToLayer("Cube");

        _playerBodyLayer = LayerMask.NameToLayer("PlayerBody");

        _botBodyLayer = LayerMask.NameToLayer("BotBody");

        Debug.Log(
            $"[BOT] {name} | " +
            $"PhotonView={photonView.ViewID} | " +
            $"Layer={LayerMask.LayerToName(gameObject.layer)}"
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Bot AI/collision is processed only by owner.
        if (!photonView.IsMine)
            return;

        GameObject target = collision.gameObject;

        Debug.Log(
            $"[BOT COLLISION] " +
            $"{name} collided with {target.name} | " +
            $"Target Layer={LayerMask.LayerToName(target.layer)}"
        );

       
        // PLAYER HEAD
        if (target.layer == _playerLayer)
        {
            CollectPlayerHead(target);
            return;
        }

        
        // OTHER BOT HEAD

        if (target.layer == _botLayer)
        {
            if (target == gameObject)
                return;

            CollectBotHead(target);
            return;
        }

        
        // CUBES / BODIES
        
        if (target.layer == _cubeLayer || target.layer == _playerBodyLayer || target.layer == _botBodyLayer)
        {
            CollectCube(target);
        }
    }

    
    // PLAYER HEAD

    private void CollectPlayerHead(GameObject playerHead)
    {
        SnakeController playerSnake = playerHead.GetComponent<SnakeController>();

        if (playerSnake == null)
        {
            Debug.LogError($"[BOT] Player head {playerHead.name} " + $"does NOT have SnakeController!");
            return;
        }

        int botValue = _snakeController.HeadValue;

        int playerValue = playerSnake.HeadValue;

        Debug.Log($"[BOT VS PLAYER] " + $"Bot={botValue} | Player={playerValue}");

        // Bot must be stronger.
        if (botValue <= playerValue)
        {
            Debug.Log("[BOT] Cannot collect player. " + "Bot is not stronger.");

            return;
        }

        Debug.Log($"[BOT] STRONGER! " + $"Collecting player {playerValue}");

        bool result = _snakeController.CollectValue(playerValue, playerHead);

        Debug.Log($"[BOT] CollectValue result = {result}");
    }

    
    // OTHER BOT HEAD
    private void CollectBotHead(GameObject target)
    {
        SnakeController targetSnake = target.GetComponent<SnakeController>();

        if (targetSnake == null)
            return;

        int myValue = _snakeController.HeadValue;

        int targetValue = targetSnake.HeadValue;

        if (myValue <= targetValue)
            return;

        _snakeController.CollectValue(targetValue, target);
    }

  
    // CUBE
    private void CollectCube(GameObject target)
    {
        NumberCube cube = target.GetComponent<NumberCube>();

        if (cube == null)
            return;

        int cubeValue =
            cube.CubeValue;

        if (cubeValue > _snakeController.HeadValue)
            return;

        _snakeController.CollectValue(cubeValue, target);
    }
}