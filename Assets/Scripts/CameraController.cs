using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomspeed = 2f;
    [SerializeField] private float zoomLerpspeed = 10f;
    [SerializeField] private float minDist = 1f; // Lowered to allow closer zoom during wall clips
    [SerializeField] private float maxDist = 15f;

    [Header("Look Settings")]
    [SerializeField] private float lookSpeedX = 0.2f;
    [SerializeField] private float lookSpeedY = 0.2f;
    [SerializeField] private bool invertY = true;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float collisionOffset = 0.2f; // Keeps camera slightly away from the wall surface

    private InputSystem_Actions Controls;
    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private Transform playerTransform; // Target for raycast

    private Vector2 scrolldelta;
    private float targetzoom;
    private float currentzoom;
    private float actualZoom; // Used for temporary zoom when colliding with walls

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

            // Find the object the camera is following
            if (cam.Follow != null)
            {
                playerTransform = cam.Follow;
            }
        }
    }

    private void Update()
    {
        HandleZoom();
        HandleLookAround();
    }

    private void HandleZoom()
    {
        if (orbital == null || playerTransform == null) return;

        // 1. Process normal scroll wheel and controller inputs
        if (scrolldelta.y != 0)
        {
            targetzoom = Mathf.Clamp(targetzoom - scrolldelta.y * zoomspeed, minDist, maxDist);
            scrolldelta = Vector2.zero;
        }

        float bumperdelta = Controls.MouseControl.GamePadZoom.ReadValue<float>();
        if (bumperdelta != 0)
        {
            targetzoom = Mathf.Clamp(targetzoom - bumperdelta * zoomspeed, minDist, maxDist);
        }

        // 2. Smoothly calculate where the user WANTS the camera to be
        currentzoom = Mathf.Lerp(currentzoom, targetzoom, Time.deltaTime * zoomLerpspeed);

        // 3. Assume actual zoom matches current zoom, unless blocked
        actualZoom = currentzoom;

        // 4. Perform Raycast from player toward intended camera position
        Vector3 directionToCamera = transform.position - playerTransform.position;
        directionToCamera.Normalize();

        if (Physics.Raycast(playerTransform.position, directionToCamera, out RaycastHit hit, currentzoom, wallLayer))
        {
            // If we hit a wall, force the actual zoom to stop right in front of the wall
            float distanceToWall = hit.distance - collisionOffset;
            actualZoom = Mathf.Clamp(distanceToWall, minDist, maxDist);
        }

        // 5. Apply final calculated zoom to Cinemachine
        orbital.Radius = actualZoom;
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