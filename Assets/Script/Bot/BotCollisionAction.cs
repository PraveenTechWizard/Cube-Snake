using UnityEngine;

public class BotCollisionAction : MonoBehaviour
{
    private SnakeController _snakeController;
    private BotTargetDetector _targetDetector;

    private void Awake()
    {
        _snakeController =
            GetComponent<SnakeController>();

        _targetDetector =
            GetComponent<BotTargetDetector>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Cube"))
            return;

        NumberCube numberCube =
            collision.gameObject.GetComponent<NumberCube>();

        if (numberCube == null)
            return;

        int value =
            numberCube.CubeValue;

        bool collected =
            _snakeController.CollectValue(
                value,
                collision.gameObject
            );

        // =====================================================
        // BOT CANNOT COLLECT THIS CUBE
        // =====================================================

        if (!collected)
        {
            // Force detector to forget this target.
            if (_targetDetector != null)
            {
                _targetDetector.ClearTarget();
            }

            return;
        }

        // =====================================================
        // COLLECTED
        // =====================================================

        Collider cubeCollider =
            collision.gameObject.GetComponent<Collider>();

        if (cubeCollider != null)
        {
            cubeCollider.enabled = false;
        }

        Rigidbody rb =
            collision.gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}