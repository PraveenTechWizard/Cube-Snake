using Photon.Pun;
using UnityEngine;

public class BotMovement : MonoBehaviourPun
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
            _moveDirection = Vector3.forward;

        _moveDirection.Normalize();
    }

    private void Update()
    {
        // MASTER/OWNER ONLY
        if (!PhotonNetwork.IsMasterClient)
            return;

        Rotate();
        MoveForward();
    }

    private void Rotate()
    {
        if (_moveDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void MoveForward()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    public void SetMoveDirection(Vector3 direction)
    {
        if (!photonView.IsMine)
            return;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        _moveDirection = direction.normalized;
    }
}