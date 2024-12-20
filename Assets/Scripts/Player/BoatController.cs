using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    public WaveCollision waveCollision; // Reference to the wave system
    public float buoyancyOffset = 0.5f; // Height offset to keep the boat "floating"
    public float moveSpeed = 50f;       // Forward/backward speed
    public float turnSpeed = 50f;       // Turning speed
    public float drag = 1f;             // Water drag
    public float angularDrag = 2f;      // Angular drag
    public float sampleDistance = 2.0f; // Distance for wave normal sampling
    public float StartingHeight = 0.0f;
    public float tiltLimit = 30f;
    public float stabilizationForce = 10f;
    public float stabilizationSpeed = 2.0f; // Rotation smoothing factor

    public GameObject focalPoint;
    public GameObject[] buoyancyPoints;

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
            if (waveCollision == null || buoyancyPoints.Length != 4) return;

            Vector3[] points = new Vector3[4];
            float[] heights = new float[4];

            // Collect positions and wave heights for all 4 points
            for (int i = 0; i < buoyancyPoints.Length; i++)
            {
                points[i] = buoyancyPoints[i].transform.position;
                heights[i] = waveCollision.GetWaveHeight(points[i], time);
            }

            // Calculate average buoyancy height
            float averageHeight = (heights[0] + heights[1] + heights[2] + heights[3]) / 4.0f;

            // Adjust the boat's vertical position
            Vector3 position = rb.position;
            position.y = Mathf.Lerp(position.y, averageHeight + buoyancyOffset + StartingHeight, Time.fixedDeltaTime * 5.0f);
            rb.MovePosition(position);

            // Calculate wave normal from sample points
            Vector3 forwardSlope = new Vector3(0, heights[0] - heights[1], Vector3.Distance(points[0], points[1])).normalized;
            Vector3 rightSlope = new Vector3(Vector3.Distance(points[2], points[3]), heights[3] - heights[2], 0).normalized;

            Vector3 waveNormal = Vector3.Cross(rightSlope, forwardSlope).normalized;
            if (waveNormal.y < 0) waveNormal = -waveNormal; // Ensure upward normal

            // Calculate target rotation
            Quaternion targetRotation = Quaternion.LookRotation(forwardSlope, waveNormal);

            // Smoothly apply rotation
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * stabilizationSpeed));
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