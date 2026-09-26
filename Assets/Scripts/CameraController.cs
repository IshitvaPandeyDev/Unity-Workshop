using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomspeed = 2f;
    [SerializeField] private float zoomLerpspeed = 10f;
    [SerializeField] private float minDist = 1f;
    [SerializeField] private float maxDist = 15f;

    [Header("Sprint Zoom Settings")]
    [SerializeField] private float sprintZoomMultiplier = 1.25f; // Expands radius by 25% (0.8x visual scale)
    [SerializeField] private float sprintTransitionSpeed = 6f;

    [Header("Look Settings")]
    [SerializeField] private float lookSpeedX = 0.2f;
    [SerializeField] private float lookSpeedY = 0.2f;
    [SerializeField] private bool invertY = true;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float collisionOffset = 0.2f;

    private InputSystem_Actions Controls;
    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private Transform playerTransform;

    private Vector2 scrolldelta;
    private float targetzoom;
    private float currentzoom;
    private float actualZoom;

    private void Start()
    {
        Controls = new InputSystem_Actions();
        Controls.Enable();
        Controls.MouseControl.MouseZoom.performed += HandleMouseScroll;

        Cursor.lockState = CursorLockMode.Locked;

        cam = GetComponent<CinemachineCamera>();
        orbital = GetComponent<CinemachineOrbitalFollow>();

        if (orbital != null)
        {
            targetzoom = currentzoom = orbital.Radius;

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

        // 1. Process normal scroll wheel and controller inputs for base zoom
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

        // 2. Check if the player is actively sprinting (matches ParkourController logic)
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0.1f;

        // 3. Calculate temporary zoom target based on sprint state
        float desiredZoom = isSprinting ? targetzoom * sprintZoomMultiplier : targetzoom;

        // 4. Smoothly transition to the desired zoom
        float activeLerpSpeed = isSprinting ? sprintTransitionSpeed : zoomLerpspeed;
        currentzoom = Mathf.Lerp(currentzoom, desiredZoom, Time.deltaTime * activeLerpSpeed);

        actualZoom = currentzoom;

        // 5. Perform Raycast to prevent clipping through walls
        Vector3 directionToCamera = transform.position - playerTransform.position;
        directionToCamera.Normalize();

        if (Physics.Raycast(playerTransform.position, directionToCamera, out RaycastHit hit, currentzoom, wallLayer))
        {
            float distanceToWall = hit.distance - collisionOffset;
            actualZoom = Mathf.Clamp(distanceToWall, minDist, maxDist * sprintZoomMultiplier);
        }

        // 6. Apply final calculated zoom to Cinemachine
        orbital.Radius = actualZoom;
    }

    private void HandleLookAround()
    {
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