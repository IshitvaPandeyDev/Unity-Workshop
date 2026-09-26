using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingTile : MonoBehaviour
{
    public enum MovementAxis { X, Z }

    [Header("Movement Settings")]
    public MovementAxis axis = MovementAxis.X;
    public float movementDistance = 5f;
    public float speed = 2f;

    private Rigidbody rb;
    private Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Critical: Set the Rigidbody to kinematic so it isn't affected by gravity,
        // but still interacts with the player's Character Controller.
        rb.isKinematic = true;

        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Calculate the offset using a sine wave for smooth back-and-forth motion
        float offset = Mathf.Sin(Time.time * speed) * movementDistance;
        Vector3 newPosition = startPosition;

        if (axis == MovementAxis.X)
        {
            newPosition.x += offset;
        }
        else if (axis == MovementAxis.Z)
        {
            newPosition.z += offset;
        }

        // MovePosition is required for Rigidbody physics to calculate velocity correctly 
        // for the player standing on it.
        rb.MovePosition(newPosition);
    }
}