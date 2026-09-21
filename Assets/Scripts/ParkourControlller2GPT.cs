using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ParkourController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Parkour Settings")]
    public float ledgeGrabDistance = 1f;
    public Transform ledgeRaycastPoint;
    public float climbDuration = 1.2f;

    private CharacterController controller;
    private Animator animator;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isClimbing = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (isClimbing)
            return;

        HandleMovement();
        HandleJump();

        if (!isGrounded && velocity.y <= 0)
        {
            DetectLedge();
        }
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Camera directions
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Ignore camera vertical rotation
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Camera-relative movement
        Vector3 move = forward * z + right * x;

        if (move.magnitude > 0.1f)
        {
            move.Normalize();

            // Move player
            controller.Move(move * moveSpeed * Time.deltaTime);

            // Rotate player toward movement
            Quaternion targetRotation = Quaternion.LookRotation(move);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Send movement speed to Animator
        if (animator != null)
        {
            animator.SetFloat("Speed", move.magnitude);
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void DetectLedge()
    {
        Vector3 origin;

        if (ledgeRaycastPoint != null)
        {
            origin = ledgeRaycastPoint.position;
        }
        else
        {
            origin = transform.position + Vector3.up * 1.5f;
        }

        // Detect wall
        if (Physics.Raycast(
            origin,
            transform.forward,
            out RaycastHit wallHit,
            ledgeGrabDistance))
        {
            Vector3 downRayOrigin =
                origin +
                transform.forward * ledgeGrabDistance +
                Vector3.up * 1f;

            // Detect top of ledge
            if (Physics.Raycast(
                downRayOrigin,
                Vector3.down,
                out RaycastHit edgeHit,
                1.5f))
            {
                StartCoroutine(
                    PerformLedgeClimb(edgeHit.point)
                );
            }
        }
    }

    private IEnumerator PerformLedgeClimb(Vector3 targetLedgePosition)
    {
        isClimbing = true;

        velocity = Vector3.zero;

        // PLAY VAULT ANIMATION
        if (animator != null)
        {
            animator.SetTrigger("Vault");
        }

        // Wait for animation
        yield return new WaitForSeconds(climbDuration);

        // Move player to top
        controller.enabled = false;

        transform.position =
            targetLedgePosition + Vector3.up * 0.1f;

        controller.enabled = true;

        isClimbing = false;
    }
}