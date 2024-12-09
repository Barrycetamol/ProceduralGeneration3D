using System;
using UnityEngine;

public class GerstnerWaves : MonoBehaviour
{
    public int waveCount = 3;
    public Vector4[] directionsAndAmplitudes; // Each Vector4 represents (directionX, directionZ, amplitude, unused)
    public Vector4[] frequenciesAndSpeeds;   // Each Vector4 represents (frequency, speed, unused, unused)

    public Vector2 windDirection = new Vector2(1.0f, 0.5f); // Direction of the wind
    public float windStrength = 0.2f;                       // Strength of the wind influence

    public float waveHeight = 1.0f;
    public float waveLength = 2.0f;
    public float waveSpeed = 1.0f;

    private MeshFilter meshFilter;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;
    private Color[] vertexColors;

    private ColorTextureRenderer ColorBand { get; set; }

    void Start()
    {
        // Initialize wave directions and amplitudes if not set
        if (directionsAndAmplitudes == null || directionsAndAmplitudes.Length == 0)
        {
            directionsAndAmplitudes = new Vector4[waveCount];
            for (int i = 0; i < waveCount; i++)
            {
                directionsAndAmplitudes[0] = new Vector4(1.0f, 0.5f, 0.2f, 0);
                directionsAndAmplitudes[1] = new Vector4(-0.5f, 1.0f, 0.15f, 0);
                directionsAndAmplitudes[2] = new Vector4(0.7f, -0.7f, 0.1f, 0);
            }
        }

        // Initialize frequencies and speeds if not set
        if (frequenciesAndSpeeds == null || frequenciesAndSpeeds.Length == 0)
        {
            frequenciesAndSpeeds = new Vector4[waveCount];
            for (int i = 0; i < waveCount; i++)
            {
                frequenciesAndSpeeds[0] = new Vector4(0.8f, 0.5f, 0, 0);
                frequenciesAndSpeeds[1] = new Vector4(0.9f, 0.6f, 0, 0);
                frequenciesAndSpeeds[2] = new Vector4(1.0f, 0.4f, 0, 0);
            }
        }

        meshFilter = GetComponent<MeshFilter>();
        originalVertices = meshFilter.mesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];
        vertexColors = new Color[originalVertices.Length];
    }

    void Update()
    {
        // Update wave vertices
        float[] waveHeights = CalculateWaveHeights();
        ApplyWaveVertices(waveHeights);

        Mesh mesh = meshFilter.mesh;
        mesh.vertices = modifiedVertices;

        // Update colors if ColorBand is assigned
        if (ColorBand != null)
        {
            ApplyVertexColors(waveHeights);
            mesh.colors = vertexColors;
        }

        mesh.RecalculateNormals();
    }

    private float[] CalculateWaveHeights()
    {
        float[] heights = new float[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = transform.TransformPoint(originalVertices[i]);
            heights[i] = CalculateWaveHeight(vertex.x, vertex.z);
        }
        return heights;
    }

    private void ApplyWaveVertices(float[] heights)
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            vertex.y += heights[i];
            modifiedVertices[i] = vertex;
        }
    }

    private void ApplyVertexColors(float[] heights)
    {
        for (int i = 0; i < heights.Length; i++)
        {
            float normalizedHeight = Mathf.InverseLerp(0, waveHeight, heights[i]);
            vertexColors[i] = ColorBand.GenerateColor(normalizedHeight, false);
        }
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

    public void AddColorBand(ColorTextureRenderer colorBand)
    {
        ColorBand = colorBand;
    }
}