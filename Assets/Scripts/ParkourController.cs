using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ParkourController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Jump Feel Settings")]
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    [Header("Respawn & Checkpoints")]
    [Tooltip("How far the player must fall below their active platform to trigger a respawn")]
    public float fallRespawnDistance = 20f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform mainCameraTransform;

    // --- Jump Tracking Variables ---
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // --- Platform Tracking Variables ---
    private Collider currentPlatform;
    private Vector3 lastPlatformPosition;
    private Quaternion lastPlatformRotation;

    // --- Respawn Tracking ---
    private float activePlatformHeight;
    private Vector3 lastCheckpointPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        // Initialize both heights and checkpoints to the starting position
        activePlatformHeight = transform.position.y;
        lastCheckpointPosition = transform.position;

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        HandleRespawn();
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;

            // Constantly track the height of the current stable ground
            activePlatformHeight = transform.position.y;
        }

        Vector3 platformVelocity = Vector3.zero;

        if (isGrounded && velocity.y <= -2f)
        {
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out RaycastHit hit, 0.3f))
            {
                if (hit.rigidbody != null && !hit.transform.IsChildOf(this.transform))
                {
                    if (hit.collider != currentPlatform)
                    {
                        currentPlatform = hit.collider;

                        lastPlatformPosition = currentPlatform.transform.position;
                        lastPlatformRotation = currentPlatform.transform.rotation;
                    }

                    if (currentPlatform != null)
                    {
                        Vector3 positionDelta = currentPlatform.transform.position - lastPlatformPosition;
                        Quaternion rotationDelta = currentPlatform.transform.rotation * Quaternion.Inverse(lastPlatformRotation);

                        Vector3 localPositionOnPlatform = transform.position - currentPlatform.transform.position;
                        Vector3 rotatedLocalPosition = rotationDelta * localPositionOnPlatform;

                        Vector3 rotationMovement = rotatedLocalPosition - localPositionOnPlatform;

                        platformVelocity = positionDelta + rotationMovement;

                        lastPlatformPosition = currentPlatform.transform.position;
                        lastPlatformRotation = currentPlatform.transform.rotation;
                    }
                }
                else
                {
                    currentPlatform = null;
                }
            }
        }
        else
        {
            currentPlatform = null;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move;

        if (mainCameraTransform != null)
        {
            Vector3 cameraForward = mainCameraTransform.forward;
            Vector3 cameraRight = mainCameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            move = (cameraForward * z + cameraRight * x).normalized;

            if (move != Vector3.zero)
            {
                transform.forward = move;
            }
        }
        else
        {
            move = (transform.right * x + transform.forward * z).normalized;
        }

        // --- SPRINT LOGIC & ANIMATION ---
        float currentSpeed = moveSpeed;
        bool isSprinting = false;

        // Check if the player is holding the Left Shift key AND the W key
        if (Input.GetKey(KeyCode.LeftShift) && z > 0.1f)
        {
            currentSpeed = sprintSpeed;
            isSprinting = true;
        }

        // Apply the calculated speed
        Vector3 finalMovement = (move * currentSpeed * Time.deltaTime) + platformVelocity;
        controller.Move(finalMovement);

        // Update the Animator
        if (animator != null)
        {
            if (move.magnitude > 0)
            {
                // Send 2f for sprint, 1f for walk/run
                float animationSpeedValue = isSprinting ? 2f : 1f;
                animator.SetFloat("Speed", animationSpeedValue);
            }
            else
            {
                // Send 0f for idle
                animator.SetFloat("Speed", 0f);
            }
        }
    }

    private void HandleJump()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            currentPlatform = null;

            if (animator != null) animator.SetTrigger("Jump");
        }

        if (Input.GetButtonUp("Jump") && velocity.y > 0f)
        {
            velocity.y *= 0.5f;
            coyoteTimeCounter = 0f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleRespawn()
    {
        if (transform.position.y < activePlatformHeight - fallRespawnDistance)
        {
            controller.enabled = false;

            // Teleport to the top of the solid checkpoint block (added 1.5f height to ensure feet clear the block)
            transform.position = lastCheckpointPosition + (Vector3.up * 1.5f);

            velocity = Vector3.zero;
            controller.enabled = true;
        }
    }

    // Detects when a Character Controller bumps into solid objects.
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if the solid object we are touching is on the "Checkpoint" layer
        if (hit.gameObject.layer == LayerMask.NameToLayer("Checkpoint"))
        {
            // Update the respawn location to the center of this solid block
            lastCheckpointPosition = hit.gameObject.transform.position;
        }
    }
}