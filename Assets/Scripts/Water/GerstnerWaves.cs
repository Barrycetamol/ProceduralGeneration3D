using System;
using UnityEngine;

public class GerstnerWaves : MonoBehaviour
{
    public int waveCount = 3;
    public Vector4[] directionsAndAmplitudes; //  (directionX, directionZ, amplitude, unused)
    public Vector4[] frequenciesAndSpeeds;   // (frequency, speed, unused, unused)
    public WindGeneration WindGeneration {get; set;}
    private float startingHeight = 1.0f;

    private Material rendererMaterial;

    private ColorTextureRenderer ColorBand { get; set; }
    private MeshCollider MeshCollider {get; set;}
    private MeshFilter MeshFilter {get; set;}
    private Wind CurrentWind = new();
    private Wind prevWind = new();

    void Start()
    {
        WindGeneration = GameObject.FindGameObjectWithTag("Wind").GetComponent<WindGeneration>();
        Renderer renderer = GetComponent<Renderer>();
        rendererMaterial = new Material(Shader.Find("Custom/GerstnerWaves"));
        renderer.material = rendererMaterial;

        // Update wave parameters
        rendererMaterial.SetFloat("_WaveCount", 3);
        rendererMaterial.SetFloat("_StartingHeight", startingHeight);

        // Update wind parameters


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
        prevWind = CurrentWind;
        CurrentWind = WindGeneration.GetWind(new Vector2Int(0,0));

        // if our wind is different, update shader
        if(prevWind.windDirection != CurrentWind.windDirection && prevWind.windStrength != CurrentWind.windStrength){
            rendererMaterial.SetVector("_WindDirection", new Vector4(CurrentWind.windDirection.x, CurrentWind.windDirection.y, 0, 0));
            rendererMaterial.SetFloat("_WindStrength", CurrentWind.windStrength.x);
            rendererMaterial.SetVector("_WaveDirections", new Vector4(1.0f, 0.5f, 0.2f, 0));
            rendererMaterial.SetVector("_WaveFrequencies", new Vector4(0.8f, 0.9f, 1.0f, 0));
            rendererMaterial.SetVector("_WaveAmplitudes", new Vector4(0.2f, 0.15f, 0.1f, 0));
            rendererMaterial.SetVector("_WaveSpeeds", new Vector4(0.5f, 0.6f, 0.4f, 0));
        }


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