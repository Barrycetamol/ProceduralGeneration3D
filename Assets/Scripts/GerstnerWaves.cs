using UnityEngine;

public class GerstnerWaves : MonoBehaviour
{
    public int waveCount = 3;
    public Vector4[] directionsAndAmplitudes; // Each Vector4 represents (directionX, directionZ, amplitude, unused)
    public Vector4[] frequenciesAndSpeeds; // Each Vector4 represents (frequency, speed, unused, unused)

    public Vector2 windDirection = new Vector2(1.0f, 0.5f); // Direction of the wind
    public float windStrength = 0.2f; // Strength of the wind influence

    public float waveHeight = 1.0f;
    public float waveLength = 2.0f;
    public float waveSpeed = 1.0f;

    private MeshFilter meshFilter;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;

    void Start()
    {
        // Initialize default values if arrays are not set
        if (directionsAndAmplitudes == null || directionsAndAmplitudes.Length == 0)
        {
            directionsAndAmplitudes = new Vector4[waveCount];
            for (int i = 0; i < waveCount; i++)
            {
                directionsAndAmplitudes[i] = new Vector4(Mathf.Cos(i * Mathf.PI * 2 / waveCount), Mathf.Sin(i * Mathf.PI * 2 / waveCount), 0.5f, 0);
            }
        }

        if (frequenciesAndSpeeds == null || frequenciesAndSpeeds.Length == 0)
        {
            frequenciesAndSpeeds = new Vector4[waveCount];
            for (int i = 0; i < waveCount; i++)
            {
                frequenciesAndSpeeds[i] = new Vector4(1.0f, 1.0f, 0, 0);
            }
        }

        // Preset examples
        // Calm Waves Preset
        directionsAndAmplitudes[0] = new Vector4(1.0f, 0.5f, 0.2f, 0);
        directionsAndAmplitudes[1] = new Vector4(-0.5f, 1.0f, 0.15f, 0);
        directionsAndAmplitudes[2] = new Vector4(0.7f, -0.7f, 0.1f, 0);

        frequenciesAndSpeeds[0] = new Vector4(0.8f, 0.5f, 0, 0);
        frequenciesAndSpeeds[1] = new Vector4(0.9f, 0.6f, 0, 0);
        frequenciesAndSpeeds[2] = new Vector4(1.0f, 0.4f, 0, 0);

        // Rough Waves Preset
        // Uncomment for rough waves
        // directionsAndAmplitudes[0] = new Vector4(1.0f, 0.2f, 1.0f, 0);
        // directionsAndAmplitudes[1] = new Vector4(-0.6f, 0.9f, 0.8f, 0);
        // directionsAndAmplitudes[2] = new Vector4(0.3f, -0.8f, 0.9f, 0);

        // frequenciesAndSpeeds[0] = new Vector4(1.5f, 1.2f, 0, 0);
        // frequenciesAndSpeeds[1] = new Vector4(1.8f, 1.0f, 0, 0);
        // frequenciesAndSpeeds[2] = new Vector4(1.6f, 1.1f, 0, 0);

        meshFilter = GetComponent<MeshFilter>();
        originalVertices = meshFilter.mesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];
    }

    void Update()
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            Vector3 worldPos = transform.TransformPoint(vertex);
            float waveHeightOffset = CalculateWaveHeight(worldPos.x, worldPos.z);
            vertex.y += waveHeightOffset;
            modifiedVertices[i] = vertex;
        }
        Mesh mesh = meshFilter.mesh;
        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();
    }

    float CalculateWaveHeight(float x, float z)
    {
        float y = 0.0f;
        Vector2 adjustedWindDirection = windDirection.normalized * windStrength;

        for (int i = 0; i < waveCount; i++)
        {
            Vector4 dirAmp = directionsAndAmplitudes[i];
            Vector4 freqSpeed = frequenciesAndSpeeds[i];

            Vector2 baseDirection = new Vector2(dirAmp.x, dirAmp.y).normalized;
            Vector2 influencedDirection = (baseDirection + adjustedWindDirection).normalized;

            float amplitude = dirAmp.z;
            float frequency = freqSpeed.x;
            float speed = freqSpeed.y;

            float k = 2 * Mathf.PI / waveLength;
            float w = frequency * k;

            float phase = w * Time.time * speed;
            y += Mathf.Sin(Vector2.Dot(influencedDirection, new Vector2(x, z)) * k + phase) * amplitude;
        }
        return y;
    }
}
