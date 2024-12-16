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
    public float sampleDistance = 2.0f; // Distance for wave normal samplin
    public float StartingHeight = 0.0f;
    public float tiltLimit = 30f;
    public float stabilizationForce = 10f;
    private Rigidbody rb;

    void Start()
    {
        waveCollision = GetComponent<WaveCollision>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;          // Disable default gravity for floating
        rb.drag = drag;                 // Apply water drag
        rb.angularDrag = angularDrag;   // Apply angular drag
    }
    
    void Update(){
        float time = Time.time;

        // Handle Floating
        ApplyBuoyancy(time);

        // Handle Movement
        HandleMovement();

        StabilizeRotation();
    }
    void FixedUpdate()
    {

    }

    void ApplyBuoyancy(float time)
    {
        // Get the boat's current position
        Vector3 position = rb.position;

        // Get the wave height at the boat's position
        float waveHeight = waveCollision.GetWaveHeight(position, time);
        position.y = StartingHeight + waveHeight + buoyancyOffset;

        // Adjust the boat's position to float on the water
        rb.MovePosition(new Vector3(position.x, position.y, position.z));

        // Sample wave heights at key points
        float heightFront = waveCollision.GetWaveHeight(position + transform.forward * sampleDistance, time);
        float heightBack = waveCollision.GetWaveHeight(position - transform.forward * sampleDistance, time);
        float heightLeft = waveCollision.GetWaveHeight(position - transform.right * sampleDistance, time);
        float heightRight = waveCollision.GetWaveHeight(position + transform.right * sampleDistance, time);

        // Calculate slope vectors
        Vector3 forwardSlope = new Vector3(0, heightFront - heightBack, sampleDistance).normalized; // Forward/backward slope
        Vector3 rightSlope = new Vector3(sampleDistance, heightRight - heightLeft, 0).normalized;   // Left/right slope

        // Calculate wave normal
        Vector3 waveNormal = Vector3.Cross(rightSlope, forwardSlope).normalized;

        // Calculate the new rotation
        Quaternion targetRotation = Quaternion.LookRotation(forwardSlope, waveNormal);

        // Smoothly apply the rotation
        rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 1.0f));
    }

    void HandleMovement()
    {
        // Get input for forward/backward movement
        float moveInput = Input.GetAxis("Vertical"); // W/S or Up/Down
        float turnInput = Input.GetAxis("Horizontal"); // A/D or Left/Right

        // Apply force for forward/backward movement
        Vector3 moveForce = transform.forward * moveInput * moveSpeed;
        rb.AddForce(moveForce, ForceMode.Force);

        // Apply torque for turning
        float turnTorque = turnInput * turnSpeed;
        rb.AddTorque(0, turnTorque, 0, ForceMode.Force);
    }

    void StabilizeRotation()
    {
        // Get current rotation
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