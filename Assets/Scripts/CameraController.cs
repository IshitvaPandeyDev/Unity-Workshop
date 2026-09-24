using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomspeed = 2f;
    [SerializeField] private float zoomLerpspeed = 10f;
    [SerializeField] private float minDist = 3f;
    [SerializeField] private float maxDist = 15f;

    [Header("Look Settings")]
    [SerializeField] private float lookSpeedX = 0.2f;
    [SerializeField] private float lookSpeedY = 0.2f;
    [SerializeField] private bool invertY = true;

    private InputSystem_Actions Controls;

    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private Vector2 scrolldelta;

    private float targetzoom;
    private float currentzoom;

    private void Start()
    {
        Controls = new InputSystem_Actions();
        Controls.Enable();
        Controls.MouseControl.MouseZoom.performed += HandleMouseScroll;

        // Locks the cursor to the center of the screen and hides it
        Cursor.lockState = CursorLockMode.Locked;

        cam = GetComponent<CinemachineCamera>();
        orbital = GetComponent<CinemachineOrbitalFollow>();

        if (orbital != null)
        {
            targetzoom = currentzoom = orbital.Radius;
        }
    }

    private void Update()
    {
        HandleZoom();
        HandleLookAround();
    }

    private void HandleZoom()
    {
        if (orbital == null) return;

        if (scrolldelta.y != 0)
        {
            targetzoom = Mathf.Clamp(orbital.Radius - scrolldelta.y * zoomspeed, minDist, maxDist);
            scrolldelta = Vector2.zero;
        }

        float bumperdelta = Controls.MouseControl.GamePadZoom.ReadValue<float>();
        if (bumperdelta != 0)
        {
            targetzoom = Mathf.Clamp(orbital.Radius - bumperdelta * zoomspeed, minDist, maxDist);
        }

        currentzoom = Mathf.Lerp(currentzoom, targetzoom, Time.deltaTime * zoomLerpspeed);
        orbital.Radius = currentzoom;
    }

    private void HandleLookAround()
    {
        // Continuously read mouse input without requiring any button press
        if (Mouse.current != null && orbital != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            orbital.HorizontalAxis.Value += mouseDelta.x * lookSpeedX;

            float verticalAdjustment = mouseDelta.y * lookSpeedY;
            orbital.VerticalAxis.Value += invertY ? -verticalAdjustment : verticalAdjustment;
        }
    }

    private void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrolldelta = context.ReadValue<Vector2>();
    }

    private void OnDestroy()
    {
        if (Controls != null)
        {
            Controls.MouseControl.MouseZoom.performed -= HandleMouseScroll;
            Controls.Disable();
        }
    }
}