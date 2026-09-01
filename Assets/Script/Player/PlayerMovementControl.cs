using UnityEngine;

public class PlayerMovementControl : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationDeadZone = 1f;

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

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
        if (_camera == null) return;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(Vector3.up, transform.position);

        if (!plane.Raycast(ray, out float distance)) return;

        Vector3 mouseWorldPosition = ray.GetPoint(distance);

        Vector3 direction = mouseWorldPosition - transform.position;

        direction.y = 0f;

        // made by Bhargav
        if (direction.sqrMagnitude <= rotationDeadZone * rotationDeadZone) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}