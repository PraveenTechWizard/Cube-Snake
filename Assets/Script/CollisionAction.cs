using Photon.Pun;
using UnityEngine;

public class CollisionAction : MonoBehaviourPun
{
    private SnakeController _snakeController;

    private int _botLayer;
    private int _cubeLayer;
    private int _botBodyLayer;

    private void Awake()
    {
        _snakeController = GetComponent<SnakeController>();

        _botLayer = LayerMask.NameToLayer("Bot");
        _cubeLayer = LayerMask.NameToLayer("Cube");
        _botBodyLayer = LayerMask.NameToLayer("BotBody");

        Debug.Log(
            $"[PLAYER] {name} | " +
            $"PhotonView={photonView.ViewID} | " +
            $"Layer={LayerMask.LayerToName(gameObject.layer)}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine)
            return;

        GameObject target = collision.gameObject;

        Debug.Log(
            $"[PLAYER COLLISION] " +
            $"{name} collided with {target.name} | " +
            $"Target Layer={LayerMask.LayerToName(target.layer)}");

       
        // BOT HEAD
        if (target.layer == _botLayer)
        {
            CollectBotHead(target);
            return;
        }
        
        // CUBE
        if (target.layer == _cubeLayer ||
            target.layer == _botBodyLayer)
        {
            CollectCube(target);
        }
    }
    
    // BOT HEAD
    private void CollectBotHead(GameObject botHead)
    {
        SnakeController botSnake =
            botHead.GetComponent<SnakeController>();

        if (botSnake == null)
        {
            Debug.LogError($"[PLAYER] Bot head {botHead.name} " + $"does NOT have SnakeController!");

            return;
        }

        int playerValue = _snakeController.HeadValue;

        int botValue = botSnake.HeadValue;

        Debug.Log($"[PLAYER VS BOT] " + $"Player={playerValue} | Bot={botValue}");

        // Player must be stronger.
        if (playerValue <= botValue)
        {
            Debug.Log("[PLAYER] Cannot collect bot. " + "Player is not stronger.");

            return;
        }

        Debug.Log($"[PLAYER] STRONGER! " + $"Collecting bot {botValue}");

        bool result = _snakeController.CollectValue(botValue, botHead);

        Debug.Log($"[PLAYER] CollectValue result = {result}");
    }

   
    // NORMAL CUBE
    private void CollectCube(GameObject target)
    {
        NumberCube cube = target.GetComponent<NumberCube>();

        if (cube == null)
            return;

        int cubeValue = cube.CubeValue;

        if (cubeValue > _snakeController.HeadValue)
            return;

        _snakeController.CollectValue(cubeValue, target);
    }
}