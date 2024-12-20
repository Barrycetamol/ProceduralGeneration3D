using UnityEngine;

/// <summary>
/// A boat controller for the player
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    public WaveCollision waveCollision; // Our wave samples
    public float buoyancyOffset = 0.5f;  // offset from waves, lower places the boat closer to the height of the waves
    public float moveSpeed = 50f;       
    public float turnSpeed = 50f;       
    public float drag = 1f;            
    public float angularDrag = 2f;      
    public float sampleDistance = 2.0f; 
    public float StartingHeight = 0.0f;
    public float tiltLimit = 30f;
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
        rb.useGravity = false;          // floating is based on wave, doesn't use mesh colliders
        rb.drag = drag;                 
        rb.angularDrag = angularDrag;   
    }

    void FixedUpdate()
    {
        float time = Time.time;
        ApplyBuoyancy(time);
        HandleMovement();
    }


    /// <summary>
    /// Applys buoyancy to the player object
    /// </summary>
    /// <param name="time">Unitys Time.time</param>
    void ApplyBuoyancy(float time)
    {
        if (waveCollision == null || buoyancyPoints.Length != 4) return;

        Vector3[] points = new Vector3[4];
        float[] heights = new float[4];

        for (int i = 0; i < buoyancyPoints.Length; i++)
        {
            points[i] = buoyancyPoints[i].transform.position;
            heights[i] = waveCollision.GetWaveHeight(points[i], time);
        }

        float averageHeight = (heights[0] + heights[1] + heights[2] + heights[3]) / 4.0f;

        // adjust the boats vertical position
        Vector3 position = rb.position;
        position.y = Mathf.Lerp(position.y, averageHeight + buoyancyOffset + StartingHeight, Time.fixedDeltaTime * 5.0f);
        rb.MovePosition(position);

        // calculate boat slope from heights
        Vector3 forwardSlope = new Vector3(0, heights[0] - heights[1], Vector3.Distance(points[0], points[1])).normalized;
        Vector3 rightSlope = new Vector3(Vector3.Distance(points[2], points[3]), heights[3] - heights[2], 0).normalized;

        Vector3 waveNormal = Vector3.Cross(rightSlope, forwardSlope).normalized;
        if (waveNormal.y < 0) waveNormal = -waveNormal; // face up

        Quaternion targetRotation = Quaternion.LookRotation(forwardSlope, waveNormal);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * stabilizationSpeed));
    }

    /// <summary>
    /// A basic controller, uses WSAD or UDLR
    /// </summary>
    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical"); // W/S or Up/Down
        float turnInput = Input.GetAxis("Horizontal"); // A/D or Left/Right

        // Move the boat
        Vector3 moveForce = transform.forward * moveInput * moveSpeed;
        rb.AddForce(moveForce, ForceMode.Force);
        float turnTorque = turnInput * turnSpeed;
        rb.AddTorque(0, turnTorque, 0, ForceMode.Force);
    }
}