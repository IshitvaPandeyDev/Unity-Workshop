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

        
        rb.isKinematic = true;

        startPosition = transform.position;
    }

    void FixedUpdate()
    {
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

        
        rb.MovePosition(newPosition);
    }
}
