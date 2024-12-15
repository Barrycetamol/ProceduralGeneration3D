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
    public float waveLength = 20.0f;
    public float waveSpeed = 1.0f;
    private float startingHeight = 1.0f;

    private Material rendererMaterial;

    private ColorTextureRenderer ColorBand { get; set; }
    private MeshCollider MeshCollider {get; set;}
    private MeshFilter MeshFilter {get; set;}

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        rendererMaterial = new Material(Shader.Find("Custom/GerstnerWaves"));
        renderer.material = rendererMaterial;

        // Update wave parameters
        rendererMaterial.SetFloat("_WaveCount", 3);
        rendererMaterial.SetVector("_WaveDirections", new Vector4(1.0f, 0.5f, 0.2f, 0));
        rendererMaterial.SetVector("_WaveFrequencies", new Vector4(0.8f, 0.9f, 1.0f, 0));
        rendererMaterial.SetVector("_WaveAmplitudes", new Vector4(0.2f, 0.15f, 0.1f, 0));
        rendererMaterial.SetVector("_WaveSpeeds", new Vector4(0.5f, 0.6f, 0.4f, 0));
        rendererMaterial.SetFloat("_StartingHeight", startingHeight);

        // Update wind parameters
        rendererMaterial.SetVector("_WindDirection", new Vector4(1.0f, 0.5f, 0, 0));
        rendererMaterial.SetFloat("_WindStrength", 0.2f);

        Color[] colorBands = {
            new Color(0.0f, 0.5f, 1.0f, 1.0f), // Deep blue
            new Color(0.1f, 0.8f, 1.0f, 1.0f), // Light blue
            new Color(1.0f, 1.0f, 1.0f, 1.0f), // White
            new Color(0.8f, 0.8f, 0.8f, 1.0f)  // Light gray
        };
        float[] thresholds = { 0.1f, 0.3f, 0.7f, 1.0f };

        rendererMaterial.SetColorArray("_ColorBands", colorBands);
        rendererMaterial.SetFloatArray("_Thresholds", thresholds);
        rendererMaterial.SetFloat("_BandCount", thresholds.Length);
        rendererMaterial.SetFloat("_Mode", 3);
        MeshCollider = GetComponent<MeshCollider>();
        MeshFilter = GetComponent<MeshFilter>();
    }

    void Update()
    {
        MeshCollider.sharedMesh = null;
        MeshCollider.sharedMesh = MeshFilter.sharedMesh;
    }

    public void AddColorBand(ColorTextureRenderer colorBand)
    {
        ColorBand = colorBand;
    }

    public void SetDefaultHeight(Vector3 vertex)
    {
        if(rendererMaterial) rendererMaterial.SetFloat("_StartingHeight", vertex.y);
        else startingHeight = vertex.y;
    }
}