using UnityEngine;

public class BotMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    private Vector3 _moveDirection;

    private void Awake()
    {
        _moveDirection = transform.forward;

        _moveDirection.y = 0f;

        if (_moveDirection.sqrMagnitude < 0.01f)
        {
            _moveDirection = Vector3.forward;
        }

        _moveDirection.Normalize();
    }

    private void Update()
    {
        Rotate();
        MoveForward();
    }

    // =========================================================
    // ROTATION
    // =========================================================

    private void Rotate()
    {
        if (_moveDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(_moveDirection);

        // SAME style as PlayerMovementControl
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void MoveForward()
    {
        transform.position +=
            transform.forward *
            moveSpeed *
            Time.deltaTime;
    }

    // =========================================================
    // SET DIRECTION
    // =========================================================

    public void SetMoveDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        _moveDirection = direction.normalized;
    }
}