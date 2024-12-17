using UnityEngine;

public class WaveCollision : MonoBehaviour
{
    [field: SerializeField] public WindGeneration WindGeneration{get;set;}
    [field: SerializeField] public GerstnerWaves GerstnerWaves {get; set;}

    void Start(){
        WindGeneration = GameObject.FindGameObjectWithTag("Wind").GetComponent<WindGeneration>();
        GerstnerWaves = GameObject.FindGameObjectWithTag("Water").GetComponentsInChildren<GerstnerWaves>()[0];  // This will need to be changed for tiled maps.
    }

    public float GetWaveHeight(Vector3 position, float time)
    {
        if(GerstnerWaves == null) return 0.0f;

        Vector2Int pos = new Vector2Int((int)position.x, (int)position.z);
        Wind wind = WindGeneration.GetWind(pos);
        Vector2 windDir = wind.windDirection.normalized * wind.windStrength;
        
        float height = 0.0f;
        for (int i = 0; i < GerstnerWaves.waveCount; i++)
        {
            Vector2 waveDir = new Vector2(GerstnerWaves.directionsX[i], GerstnerWaves.directionsY[i]).normalized;
            waveDir += windDir;
            waveDir = waveDir.normalized;

            float amplitude = GerstnerWaves.amplitudes[i];
            float frequency = GerstnerWaves.frequencies[i];
            float speed = GerstnerWaves.speeds[i];

            float k = 2.0f * Mathf.PI / GerstnerWaves.WaveLength;
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