using Photon.Pun;
using UnityEngine;

public class PlayerMovementControl : MonoBehaviourPun
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationDeadZone = 1f;

    private Camera _camera;


    private void Awake()
    {
        // Only find/use the camera for the local player.
        if (photonView.IsMine)
        {
            _camera = Camera.main;
        }
    }


    private void Update()
    {
        // IMPORTANT:
        // Only the owner controls this player.
        if (!photonView.IsMine)
            return;


        RotateTowardsMouse();

        MoveForward();
    }


    private void MoveForward()
    {
        transform.position +=
            transform.forward *
            speed *
            Time.deltaTime;
    }


    private void RotateTowardsMouse()
    {
        if (_camera == null)
        {
            _camera = Camera.main;

            if (_camera == null)
                return;
        }


        Ray ray =
            _camera.ScreenPointToRay(
                Input.mousePosition
            );


        Plane plane =
            new Plane(
                Vector3.up,
                transform.position
            );


        if (!plane.Raycast(
                ray,
                out float distance))
        {
            return;
        }


        Vector3 mouseWorldPosition =
            ray.GetPoint(distance);


        Vector3 direction =
            mouseWorldPosition -
            transform.position;


        direction.y = 0f;


        if (direction.sqrMagnitude <=
            rotationDeadZone *
            rotationDeadZone)
        {
            return;
        }


        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }
}