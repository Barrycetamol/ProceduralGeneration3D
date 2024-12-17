using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    public WaveCollision waveCollision; // Reference to the wave system
    public float buoyancyOffset = 0.5f; // Height offset to keep the boat "floating"
    public float moveSpeed = 10f;       // Forward/backward speed
    public float turnSpeed = 50f;       // Turning speed
    public float drag = 1f;             // Water drag
    public float angularDrag = 2f;      // Angular drag
    public float sampleDistance = 2.0f; // Distance for wave normal sampling
    public float StartingHeight = 0.0f;
    public float tiltLimit = 30f;
    public float stabilizationForce = 10f;

    public GameObject focalPoint;

    private Rigidbody rb;

    void Start()
    {
        if (waveCollision == null)
        {
            waveCollision = GetComponent<WaveCollision>();
        }

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;          // Disable default gravity for floating
        rb.drag = drag;                 // Apply water drag
        rb.angularDrag = angularDrag;   // Apply angular drag
    }

    void FixedUpdate()
    {
        float time = Time.time;

        // Handle Floating
        ApplyBuoyancy(time);

        // Handle Movement
        HandleMovement();

        // Stabilize Rotation to Avoid Over-Tilting
        //StabilizeRotation();
    }

    void ApplyBuoyancy(float time)
    {
        if (waveCollision == null) return;

        // Get the boat's current position
        Vector3 position = rb.position;

        // Get the wave height at the boat's positio

        // Sample wave heights evenly for slope calculations
        float centerHeight = waveCollision.GetWaveHeight(position, time);
        float heightFront = waveCollision.GetWaveHeight(position + transform.forward * sampleDistance, time);
        float heightBack = waveCollision.GetWaveHeight(position - transform.forward * sampleDistance, time);
        float heightRight = waveCollision.GetWaveHeight(position + transform.right * sampleDistance, time);
        float heightLeft = waveCollision.GetWaveHeight(position - transform.right * sampleDistance, time);

        float targetY = StartingHeight + centerHeight + buoyancyOffset;
        position.y = Mathf.Lerp(position.y, targetY, Time.fixedDeltaTime * 5.0f); // Smooth transition
        rb.MovePosition(position);

        // Correctly calculate slopes
        Vector3 forwardSlope = new Vector3(0, heightFront - heightBack, sampleDistance).normalized;
        Vector3 rightSlope = new Vector3(sampleDistance, heightRight - heightLeft, 0).normalized;

        // Calculate the wave normal
        Vector3 waveNormal = Vector3.Cross(rightSlope, forwardSlope).normalized;
        if (waveNormal.y < 0) waveNormal = -waveNormal; // Ensure upward normal

        // Smoothly apply the rotation
        Quaternion targetRotation = Quaternion.LookRotation(forwardSlope, waveNormal);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime));
    }

    void HandleMovement()
    {
        // Get input for forward/backward movement
        float moveInput = Input.GetAxis("Vertical"); // W/S or Up/Down
        float turnInput = Input.GetAxis("Horizontal"); // A/D or Left/Right

        // Apply forward/backward force relative to the boat's local forward direction
        Vector3 moveForce = transform.forward * moveInput * moveSpeed;
        rb.AddForce(moveForce, ForceMode.Force);

        // Apply torque for turning (yaw rotation)
        float turnTorque = turnInput * turnSpeed;
        rb.AddTorque(0, turnTorque, 0, ForceMode.Force);
    }

    void StabilizeRotation()
    {
        // Get current rotation as Euler angles
        Vector3 euler = rb.rotation.eulerAngles;

        // Convert to signed angles (-180 to 180)
        if (euler.x > 180) euler.x -= 360;
        if (euler.z > 180) euler.z -= 360;

        // Limit pitch (X) and roll (Z) to tiltLimit
        euler.x = Mathf.Clamp(euler.x, -tiltLimit, tiltLimit);
        euler.z = Mathf.Clamp(euler.z, -tiltLimit, tiltLimit);

        // Smoothly stabilize extreme tilts
        Quaternion stabilizedRotation = Quaternion.Euler(euler.x, rb.rotation.eulerAngles.y, euler.z);
        rb.MoveRotation(Quaternion.Lerp(rb.rotation, stabilizedRotation, Time.fixedDeltaTime * stabilizationForce));
    }
}