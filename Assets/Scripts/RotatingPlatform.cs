using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RotatingPlatform : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("The axis to rotate around (e.g., Vector3.forward for Z-axis)")]
    public Vector3 rotationAxis = Vector3.forward;

    [Tooltip("Degrees per second")]
    public float rotationSpeed = 30f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; 
    }

    void FixedUpdate()
    {
        
        Quaternion deltaRotation = Quaternion.Euler(rotationAxis * (rotationSpeed * Time.fixedDeltaTime));
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

}
