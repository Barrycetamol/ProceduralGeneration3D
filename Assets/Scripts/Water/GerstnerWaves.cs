using UnityEngine;

public class GerstnerWaves : MonoBehaviour
{
    public int WaveLength = 5;
    public int waveCount = 3;

    public Vector4 directionsX;
    public Vector4 directionsY;
    public Vector4 amplitudes;
    public Vector4 frequencies;
    public Vector4 speeds;
    public Vector4 windDirection;
    public Vector4 windStrength;
    public WindGeneration WindGeneration {get; set;}
    public float WindUpdateFrequency = 5.0f;
    private float startingHeight = 1.0f;

    private Material rendererMaterial;

    private ColorTextureRenderer ColorBand { get; set; }
    private Wind CurrentWind = new();

    private Vector4 targetDirectionX;
    private Vector4 targetDirectionY;
    private Vector4 targetAmplitudes;
    private Vector4 targetFrequencies;
    private Vector4 targetSpeeds;
    private Vector4 targetWindDirection;
    private Vector4 targetWindStrength;
    private int windCounter = 0;


    void Start()
    {
        WindGeneration = GameObject.FindGameObjectWithTag("Wind").GetComponent<WindGeneration>();

        Renderer renderer = GetComponent<Renderer>();
        rendererMaterial = new Material(Shader.Find("Custom/GerstnerWaves"));
        renderer.material = rendererMaterial;

        // Update wave parameters
        rendererMaterial.SetFloat("_WaveCount", waveCount);
        rendererMaterial.SetFloat("_WaveLength", WaveLength);
        rendererMaterial.SetFloat("_StartingHeight", startingHeight);

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

        InvokeRepeating("UpdateWaveCalculation", 0f, WindUpdateFrequency);
    }
    
    void UpdateWaveCalculation(){
        CurrentWind = WindGeneration.GetWind(new Vector2Int(0, windCounter));
        targetWindDirection = CurrentWind.windDirection;
        targetWindStrength = CurrentWind.windStrength;

        CalculateWaves(Mathf.Max(CurrentWind.windDirection.x * CurrentWind.windStrength.x, CurrentWind.windDirection.y * CurrentWind.windStrength.y));
        windCounter += 1;
    }

    void FixedUpdate()
    {
        float lerpFactor = 0.01f * Time.deltaTime;

        for(int i = 0; i < waveCount; i++){
            directionsX[i] = Mathf.Lerp(directionsX[i], targetDirectionX[i], lerpFactor);
            directionsY[i] = Mathf.Lerp(directionsY[i], targetDirectionY[i], lerpFactor);
            amplitudes[i] = Mathf.Lerp(amplitudes[i], targetAmplitudes[i], lerpFactor);
            frequencies[i] = Mathf.Lerp(frequencies[i], targetFrequencies[i], lerpFactor);
            speeds[i] = Mathf.Lerp(speeds[i], targetSpeeds[i], lerpFactor);
            windDirection[i] = Mathf.Lerp(windDirection[i], targetWindDirection[i], lerpFactor);
            windStrength[i] = Mathf.Lerp(windStrength[i], targetWindStrength[i], lerpFactor);
        }

        rendererMaterial.SetVector("_WindDirection", windDirection);
        rendererMaterial.SetFloat("_WindStrength", windStrength.x);
        rendererMaterial.SetVector("_WaveDirectionX", directionsX);
        rendererMaterial.SetVector("_WaveDirectionY", directionsY);
        rendererMaterial.SetVector("_WaveFrequencies", frequencies);
        rendererMaterial.SetVector("_WaveAmplitudes", amplitudes);
        rendererMaterial.SetVector("_WaveSpeeds", speeds);
    }

    private void CalculateWaves(float wind)
    {
        // Clamp wind direction to ensure it stays in range
        wind = Mathf.Clamp(wind, -10f, 10f);

        // Transition factor: 0 = calm, 1 = stormy
        float intensity = Mathf.InverseLerp(0f, 10f, Mathf.Abs(wind));

        // Base values for calm and stormy states
        float[] calmSpeeds = { 1f, 0.8f, 1.2f };
        float[] stormySpeeds = { 4f, 3.5f, 5f };

        float[] calmAmplitudes = { 0.2f, 0.15f, 0.25f };
        float[] stormyAmplitudes = { 2.5f, 2.0f, 3f };

        float[] calmFrequencies = { 0.5f, 0.4f, 0.6f };
        float[] stormyFrequencies = { 1.5f, 1.2f, 1.8f };

        // Generate properties for each wave
        for (int i = 0; i < 3; i++)
        {
            // Wave speed, amplitude, and frequency interpolation
            float waveSpeed = Mathf.Lerp(calmSpeeds[i], stormySpeeds[i], intensity);
            float waveAmplitude = Mathf.Lerp(calmAmplitudes[i], stormyAmplitudes[i], intensity);
            float waveFrequency = Mathf.Lerp(calmFrequencies[i], stormyFrequencies[i], intensity);

            // Assign wave direction based on wind (add slight variation for each wave)
            Vector2 waveDirection = new Vector2(wind / 10f + Random.Range(-0.2f, 0.2f), 1f).normalized;

            // Store properties in the wave arrays
            targetDirectionX[i] = waveDirection.x;
            targetDirectionY[i] = waveDirection.y;
            targetFrequencies[i] = waveFrequency;
            targetSpeeds[i] = waveSpeed;
            targetAmplitudes[i] = waveAmplitude;
            
        }
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