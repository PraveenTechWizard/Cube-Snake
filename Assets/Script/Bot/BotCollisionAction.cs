using UnityEngine;

public class BotCollisionAction : MonoBehaviour
{
    private SnakeController _snakeController;
    private BotTargetDetector _targetDetector;

    private int _cubeLayer;
    private int _playerBodyLayer;


    private void Awake()
    {
        _snakeController =
            GetComponent<SnakeController>();

        _targetDetector =
            GetComponent<BotTargetDetector>();


        _cubeLayer =
            LayerMask.NameToLayer("Cube");

        _playerBodyLayer =
            LayerMask.NameToLayer("PlayerBody");
    }


    private void OnCollisionEnter(
        Collision collision)
    {
        GameObject target =
            collision.gameObject;


        // ---------------------------------------------
        // NEVER COLLECT OWN BODY
        // ---------------------------------------------

        if (target.transform.IsChildOf(transform))
        {
            return;
        }


        int targetLayer =
            target.layer;


        bool isWorldCube =
            targetLayer == _cubeLayer;


        bool isPlayerBody =
            targetLayer == _playerBodyLayer;


        // ---------------------------------------------
        // BOT CAN COLLECT ONLY:
        //
        // World Cube
        // Player Body
        // ---------------------------------------------

        if (!isWorldCube &&
            !isPlayerBody)
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
            $"Bot hit value: {value}"
        );


        bool collected =
            _snakeController.CollectValue(
                value,
                target
            );


        // ---------------------------------------------
        // FAILED TO COLLECT
        // ---------------------------------------------

        if (!collected)
        {
            if (_targetDetector != null)
            {
                _targetDetector.ClearTarget();
            }

            return;
        }


        // ---------------------------------------------
        // SUCCESSFULLY COLLECTED
        // ---------------------------------------------

        // IMPORTANT:
        // Do NOT disable collider.
        //
        // Enemy must still be able to collect
        // this cube later.


        Rigidbody rb =
            target.GetComponent<Rigidbody>();


        if (rb != null)
        {
            rb.isKinematic = true;

            rb.useGravity = false;
        }


        // This cube now belongs to the BOT.
        target.layer =
            LayerMask.NameToLayer("BotBody");


        if (_targetDetector != null)
        {
            _targetDetector.ClearTarget();
        }
    }
}