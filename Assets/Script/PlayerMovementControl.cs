using System;
using UnityEngine;

public class PlayerMovementControl : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    
    private Camera _camera;
    
    //
    private void Awake()
    {
        _camera = Camera.main;
    }
    //
    
    public void Update()
    {
        RotateTowardsMouse();
        MoveForward();
    }

    private void MoveForward()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void RotateTowardsMouse()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(Vector3.up, transform.position);
        
       // Debug.Log($"Plane {plane}");

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPosition = ray.GetPoint(distance);

            Vector3 direction = mouseWorldPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}
