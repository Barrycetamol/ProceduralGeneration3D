using UnityEngine;
public class Floating : MonoBehaviour
{
    public WaveCollision WaveCollision; 
    public float StartingHeight {get; set;} = 0;
    public float buoyancyOffset = 0.5f;
    public float timeOffset = 0.0f;

    void Start(){
        WaveCollision = GetComponent<WaveCollision>();
    }

    void Update()
    {
        float time = Time.time + timeOffset;

        UpdatePosition(time);
        UpdateRotation(time);
    }

    void UpdatePosition(float time)
    {
        Vector3 position = transform.position;
        float waveHeight = WaveCollision.GetWaveHeight(position, time);

        position.y = StartingHeight + waveHeight + buoyancyOffset;
        transform.position = position;
    }

void UpdateRotation(float time)
{
    // Sample points around the ship to calculate the wave slope
    float sampleDistance = 1.0f;
    Vector3 position = transform.position;

    // get samples from around the flaoting object
    float heightFront = WaveCollision.GetWaveHeight(position + transform.forward * sampleDistance, time);
    float heightBack = WaveCollision.GetWaveHeight(position - transform.forward * sampleDistance, time);
    float heightLeft = WaveCollision.GetWaveHeight(position - transform.right * sampleDistance, time);
    float heightRight = WaveCollision.GetWaveHeight(position + transform.right * sampleDistance, time);

    // Calculate slope vectors
    Vector3 forwardSlope = new Vector3(0, heightFront - heightBack, sampleDistance).normalized;
    Vector3 rightSlope = new Vector3(sampleDistance, heightRight - heightLeft, 0).normalized;

    Vector3 waveNormal = Vector3.Cross(rightSlope, forwardSlope).normalized;

    Quaternion targetRotation = Quaternion.LookRotation(forwardSlope, waveNormal);
    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 2.0f);

    // Lock Z so the boat doesnt flip. Not sure why this is the case but
    Vector3 lockedEuler = transform.rotation.eulerAngles;
    lockedEuler.z = 0; 
    transform.rotation = Quaternion.Euler(lockedEuler);
}
}