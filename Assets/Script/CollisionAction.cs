using UnityEngine;

public class CollisionAction : MonoBehaviour
{
    private SnakeController _snakeController;

    private int _cubeLayer;
    private int _botBodyLayer;


    private void Awake()
    {
        _snakeController =
            GetComponent<SnakeController>();


        _cubeLayer =
            LayerMask.NameToLayer("Cube");

        _botBodyLayer =
            LayerMask.NameToLayer("BotBody");
    }


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


        bool isWorldCube =
            targetLayer == _cubeLayer;


        bool isBotBody =
            targetLayer == _botBodyLayer;


        // Player can collect:
        //
        // World Cube
        // Bot Body
        if (!isWorldCube &&
            !isBotBody)
        {
            return;
        }


        NumberCube numberCube =
            target.GetComponent<NumberCube>();


        if (numberCube == null)
            return;


        int value =
            numberCube.CubeValue;


        Debug.Log(
            $"Player hit cube: {value}"
        );


        bool collected =
            _snakeController.CollectValue(
                value,
                target
            );


        if (!collected)
        {
            return;
        }


        Rigidbody rb =
            target.GetComponent<Rigidbody>();


        if (rb != null)
        {
            rb.isKinematic = true;

            rb.useGravity = false;
        }


        // This cube now belongs to PLAYER.
        target.layer =
            LayerMask.NameToLayer("PlayerBody");
    }
}