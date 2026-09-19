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
    public Transform ledgeRaycastPoint; // Assign an empty GameObject placed near the player's chest/head
    public float climbDuration = 1.2f; // Time it takes for the climb animation to finish

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isClimbing = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Freeze manual movement while the climb animation plays
        if (isClimbing) return;

        HandleMovement();
        HandleJump();

        // Only look for a ledge if the player is falling or mid-air
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
            velocity.y = -2f; // Forces the player to stick to the ground smoothly
        }

        float x = Input.GetAxis("Horizontal"); // A/D keys
        float z = Input.GetAxis("Vertical");   // W/S keys

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Send speed to Animator for walking/running animations
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

            if (animator != null) animator.SetTrigger("Jump");
        }

        // Apply gravity over time
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void DetectLedge()
    {
        Vector3 origin = ledgeRaycastPoint != null ? ledgeRaycastPoint.position : transform.position + Vector3.up * 1.5f;

        // 1. Raycast forward to find the wall
        if (Physics.Raycast(origin, transform.forward, out RaycastHit wallHit, ledgeGrabDistance))
        {
            // 2. Raycast downward just past the wall to find the top edge of the ledge
            Vector3 downRayOrigin = origin + (transform.forward * ledgeGrabDistance) + (Vector3.up * 1f);
            if (Physics.Raycast(downRayOrigin, Vector3.down, out RaycastHit edgeHit, 1.5f))
            {
                StartCoroutine(PerformLedgeClimb(edgeHit.point));
            }
        }
    }

    private IEnumerator PerformLedgeClimb(Vector3 targetLedgePosition)
    {
        isClimbing = true;
        velocity = Vector3.zero; // Stop falling momentum

        // Trigger the climbing animation
        if (animator != null)
        {
            animator.SetTrigger("LedgeClimb");
        }

        // Wait for the animation to play out (adjust climbDuration in the Inspector to match your animation)
        yield return new WaitForSeconds(climbDuration);

        // Snap the character to the top of the ledge once the animation finishes
        controller.enabled = false; // Disable controller briefly to allow manual position overriding
        transform.position = targetLedgePosition + (Vector3.up * 0.1f); // Slight offset to prevent clipping
        controller.enabled = true;

        isClimbing = false;
    }
}