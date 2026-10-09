
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovementControl : MonoBehaviourPun
{
    [Header("Movement Settings")]
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationDeadZone = 1f;

    [Header("Speed Boost")]
    [SerializeField] private float boostSpeed = 10f;
    [SerializeField] private float maxBoostEnergy = 2f;
    [SerializeField] private float rechargeRate = 0.5f;

    [Header("Boost UI")]
    [SerializeField] private Image boostTimerFill;

    private float boostEnergy;
    private bool isBoosting;

    private Camera playerCamera;

    private void Awake()
    {
        // Only the local player controls movement and boost.
        if (photonView.IsMine)
        {
            playerCamera = Camera.main;
        }

        // Start with a full boost energy bar.
        boostEnergy = Mathf.Max(0f, maxBoostEnergy);
    }

    private void Update()
    {
        // Ignore remote player instances.
        if (!photonView.IsMine)
            return;

        RotateTowardsMouse();
        UpdateSpeedBoost();
        MoveForward();
        UpdateBoostUI();
    }

    private void MoveForward()
    {
        float currentSpeed = isBoosting ? boostSpeed : normalSpeed;

        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    private void RotateTowardsMouse()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;

            if (playerCamera == null)
                return;
        }

        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        Plane movementPlane = new Plane(Vector3.up, transform.position);

        if (!movementPlane.Raycast(ray, out float distance))
            return;

        Vector3 mouseWorldPosition = ray.GetPoint(distance);
        Vector3 direction = mouseWorldPosition - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= rotationDeadZone * rotationDeadZone)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void UpdateSpeedBoost()
    {
        if (Mouse.current == null)
        {
            isBoosting = false;
            RechargeBoost();
            return;
        }

        bool isHoldingMouse = Mouse.current.leftButton.isPressed;

        if (isHoldingMouse && boostEnergy > 0f)
        {
            // Consume energy while boosting.
            isBoosting = true;

            boostEnergy -= Time.deltaTime;
            boostEnergy = Mathf.Max(0f, boostEnergy);

            // Stop immediately when energy is exhausted.
            if (boostEnergy <= 0f)
                isBoosting = false;
        }
        else
        {
            // Normal movement while released or out of energy.
            isBoosting = false;
            RechargeBoost();
        }
    }

    private void RechargeBoost()
    {
        boostEnergy += rechargeRate * Time.deltaTime;
        boostEnergy = Mathf.Clamp(boostEnergy, 0f, Mathf.Max(0f, maxBoostEnergy));
    }

    private void UpdateBoostUI()
    {
        if (boostTimerFill == null)
            return;

        float fillAmount = maxBoostEnergy > 0f ? boostEnergy / maxBoostEnergy : 0f;

        boostTimerFill.fillAmount = Mathf.Clamp01(fillAmount);
    }

    public bool IsBoosting()
    {
        return isBoosting;
    }

    public float GetBoostEnergyNormalized()
    {
        return maxBoostEnergy > 0f ? boostEnergy / maxBoostEnergy : 0f;
    }
}