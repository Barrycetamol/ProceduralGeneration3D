using UnityEngine;

public class WaveCollision : MonoBehaviour
{
    public float waveLength = 20.0f;
    public Vector4[] waveDirectionsAndAmplitudes; // (directionX, directionZ, amplitude, unused)
    public Vector2[] waveFrequenciesAndSpeeds;   // (frequency, speed
    public Vector2 windDirection = new Vector2(1.0f, 0.5f);

    public WindGeneration WindGeneration{get;set;}

    void Start(){
        WindGeneration = GameObject.FindGameObjectWithTag("Wind").GetComponent<WindGeneration>();
    }

    public float GetWaveHeight(Vector3 position, float time)
    {

        Vector2Int pos = new Vector2Int((int)position.x, (int)position.z);
        Wind wind = WindGeneration.GetWind(pos);
        Vector2 windDir = wind.windDirection.normalized * wind.windStrength;
        
        float height = 0.0f;
        for (int i = 0; i < waveDirectionsAndAmplitudes.Length; i++)
        {
            Vector2 waveDir = new Vector2(waveDirectionsAndAmplitudes[i].x, waveDirectionsAndAmplitudes[i].y).normalized;
            waveDir += windDir;
            waveDir = waveDir.normalized;

            float amplitude = waveDirectionsAndAmplitudes[i].z;
            float frequency = waveFrequenciesAndSpeeds[i].x;
            float speed = waveFrequenciesAndSpeeds[i].y;

            float k = 2.0f * Mathf.PI / waveLength;
            float w = frequency * k;

            float phase = w * time * speed;
            height += Mathf.Sin(Vector2.Dot(waveDir, new Vector2(position.x, position.z)) * k + phase) * amplitude;
        }

        return height;
    }

    public Vector3 GetWaveNormal(Vector3 position, float time)
    {
        float delta = 0.1f; // Small offset for numerical differentiation
        float heightX1 = GetWaveHeight(position + new Vector3(delta, 0, 0), time);
        float heightX2 = GetWaveHeight(position - new Vector3(delta, 0, 0), time);
        float heightZ1 = GetWaveHeight(position + new Vector3(0, 0, delta), time);
        float heightZ2 = GetWaveHeight(position - new Vector3(0, 0, delta), time);

        Vector3 normal = new Vector3(heightX1 - heightX2, 2 * delta, heightZ1 - heightZ2).normalized;
        return normal;
    }
}