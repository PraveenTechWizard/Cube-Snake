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

        _snakeController.bodyPrefab = collision.gameObject;
        
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

        // Ask SnakeController whether it can collect it.
        bool collected =
            _snakeController.CollectValue(value);

        // Only destroy the obstacle if it was actually collected.
        if (collected)
        {
            Destroy(collision.gameObject);
        }
    }
}