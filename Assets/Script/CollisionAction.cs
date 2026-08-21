using UnityEngine;

public class CollisionAction : MonoBehaviour
{
    private SnakeController _snakeController;

    private void Awake()
    {
        _snakeController = GetComponent<SnakeController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Cube"))
        {
            return;
        }

        NumberCube numberCube =
            collision.gameObject.GetComponent<NumberCube>();

        if (numberCube == null)
        {
            Debug.LogWarning(
                "The object has Cube tag but no NumberCube component."
            );

            return;
        }

        int value = numberCube.CubeValue;

        Debug.Log($"Player hit cube: {value}");

        bool collected =
            _snakeController.CollectValue(
                value,
                collision.gameObject
            );

        if (!collected)
        {
            return;
        }

        // -------------------------------------------------
        // This cube is now part of the snake.
        // It is no longer a world obstacle.
        // -------------------------------------------------

        collision.gameObject.tag = "Untagged";


        // -------------------------------------------------
        // Disable collision.
        // -------------------------------------------------

        Collider cubeCollider =
            collision.gameObject.GetComponent<Collider>();

        if (cubeCollider != null)
        {
            cubeCollider.enabled = false;
        }


        // -------------------------------------------------
        // Disable physics.
        // -------------------------------------------------

        Rigidbody rb =
            collision.gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}