using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ParkourController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Jump Feel Settings")]
    public float coyoteTime = 0.2f;    // How long you can still jump after falling off a ledge
    public float jumpBufferTime = 0.2f; // How early you can press jump before landing

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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        // Ensure character sticks to the ground
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- Calculate Moving Platform Velocity ---
        Vector3 platformVelocity = Vector3.zero;

        // Only perform platform math if the Character Controller confirms we are grounded,
        // AND the downward velocity has stopped (meaning we actually landed).
        if (isGrounded && velocity.y <= -2f)
        {
            // We use a short raycast (0.3f) because we know we are grounded.
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out RaycastHit hit, 0.3f))
            {
                // Only calculate moving platform math if the object actually has a Rigidbody
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

        // --- Standard Input Movement ---
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

        // Combine Input Movement + Platform Movement 
        Vector3 finalMovement = (move * moveSpeed * Time.deltaTime) + platformVelocity;

        controller.Move(finalMovement);

        if (animator != null)
        {
            animator.SetFloat("Speed", move.magnitude);
        }
    }

    private void HandleJump()
    {
        // --- Update Timers ---
        // Coyote Time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump Buffer
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // --- Execute Jump Logic ---
        // If the player pressed jump recently AND they were on the ground recently
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

            // Prevent double jumping by resetting both timers instantly
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;

            // Clear the current platform so we don't inherit rotation mid-air
            currentPlatform = null;

            if (animator != null) animator.SetTrigger("Jump");
        }

        // Stop upward velocity if the player releases the jump button early (Variable Jump Height)
        if (Input.GetButtonUp("Jump") && velocity.y > 0f)
        {
            velocity.y *= 0.5f; // Cuts current upward speed in half
            coyoteTimeCounter = 0f; // Ensure they can't double jump
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}