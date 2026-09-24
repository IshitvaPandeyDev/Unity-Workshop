using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ParkourController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Ledge Climbing Settings")]
    public float ledgeGrabDistance = 1f;
    public Transform ledgeRaycastPoint;
    public float climbDuration = 1.2f;
    [Tooltip("Which layers can the player climb? (Don't include the floor/cylinders)")]
    public LayerMask climbableLayers = ~0;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isClimbing = false;
    private Transform mainCameraTransform;

    // Platform Tracking Variables
    private Collider currentPlatform;
    private Vector3 lastPlatformPosition;
    private Quaternion lastPlatformRotation;

    // NEW: Air time tracking to prevent instantly grabbing ledges you just fell off
    private float airTime = 0f;

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
        if (isClimbing) return;

        HandleMovement();
        HandleJump();

        // Track how long the player has been in the air
        if (isGrounded)
        {
            airTime = 0f;
        }
        else
        {
            airTime += Time.deltaTime;
        }

        // Only allow ledge grabbing if falling AND we've been in the air for at least 0.25 seconds
        if (!isGrounded && velocity.y <= 0 && airTime > 0.25f)
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

        Vector3 platformVelocity = Vector3.zero;

        if (isGrounded && Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 0.5f))
        {
            if (!hit.transform.IsChildOf(this.transform))
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

                    Matrix4x4 platformMatrix = Matrix4x4.TRS(currentPlatform.transform.position, rotationDelta, Vector3.one);
                    Vector3 localPositionOnPlatform = transform.position - currentPlatform.transform.position;
                    Vector3 newPositionDueToRotation = platformMatrix.MultiplyPoint3x4(localPositionOnPlatform);

                    Vector3 rotationMovement = newPositionDueToRotation - localPositionOnPlatform;

                    platformVelocity = positionDelta + rotationMovement;

                    lastPlatformPosition = currentPlatform.transform.position;
                    lastPlatformRotation = currentPlatform.transform.rotation;
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

        Vector3 finalMovement = (move * moveSpeed * Time.deltaTime) + platformVelocity;
        controller.Move(finalMovement);

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
            currentPlatform = null;

            if (animator != null) animator.SetTrigger("Jump");
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void DetectLedge()
    {
        Vector3 origin = ledgeRaycastPoint != null ? ledgeRaycastPoint.position : transform.position + Vector3.up * 1.5f;

        // NEW: The raycast now respects the 'climbableLayers' mask
        if (Physics.Raycast(origin, transform.forward, out RaycastHit wallHit, ledgeGrabDistance, climbableLayers))
        {
            Vector3 downRayOrigin = origin + (transform.forward * ledgeGrabDistance) + (Vector3.up * 1f);
            if (Physics.Raycast(downRayOrigin, Vector3.down, out RaycastHit edgeHit, 1.5f, climbableLayers))
            {
                StartCoroutine(PerformLedgeClimb(edgeHit.point));
            }
        }
    }

    private IEnumerator PerformLedgeClimb(Vector3 targetLedgePosition)
    {
        isClimbing = true;
        velocity = Vector3.zero;

        if (animator != null)
        {
            animator.SetTrigger("LedgeClimb");
        }

        yield return new WaitForSeconds(climbDuration);

        controller.enabled = false;
        transform.position = targetLedgePosition + (Vector3.up * 0.1f);
        controller.enabled = true;

        isClimbing = false;
    }
}