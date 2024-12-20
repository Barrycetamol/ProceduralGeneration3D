using UnityEngine;

/// <summary>
/// A gerstner waves handler class that pushes its calculated values off to the gersner shader
/// </summary>
public class GerstnerWaves : MonoBehaviour
{
    [field: SerializeField] public Vector4 WaveLength {get; set;} = new Vector4(2, 3, 8, 0); 
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
    private int windCounter;


    void Start()
    {
        WindGeneration = GameObject.FindGameObjectWithTag("Wind").GetComponent<WindGeneration>();
        
        // convert to use our colour band system later
        Color[] colorBands = {
            new Color(0.0f, 0.5f, 1.0f, 1.0f), // deep blue
            new Color(0.1f, 0.8f, 1.0f, 1.0f), // light blue
            new Color(1.0f, 1.0f, 1.0f, 1.0f), // white
            new Color(0.8f, 0.8f, 0.8f, 1.0f) // gray
        };
        float[] thresholds = { 0.1f, 0.3f, 0.7f, 1.0f };

        // starting preset from experimenting. This should be moved to something we can edit.
        directionsX[0] = 0.4f;
        directionsX[1] = -0.1f;
        directionsX[2] = 0.1f;

        directionsY[0] = 1.0f;
        directionsY[1] = -0.1f;
        directionsY[2] = 0.1f;

        frequencies[0] = 0.6f;
        frequencies[1] = -0.4f;
        frequencies[2] = 0.6f;

        amplitudes[0] = 0.34f;
        amplitudes[1] = 0.26f;
        amplitudes[2] = 0.43f;



        // Store initial values in shader
        Renderer renderer = GetComponent<Renderer>();
        rendererMaterial = new Material(Shader.Find("Custom/GerstnerWaves"));
        renderer.material = rendererMaterial;
        rendererMaterial.SetFloat("_WaveCount", waveCount);
        rendererMaterial.SetVector("_WaveLength", WaveLength);
        rendererMaterial.SetFloat("_StartingHeight", startingHeight);
        rendererMaterial.SetColorArray("_ColorBands", colorBands);
        rendererMaterial.SetFloatArray("_Thresholds", thresholds);
        rendererMaterial.SetFloat("_BandCount", thresholds.Length);
        rendererMaterial.SetFloat("_Mode", 3);

        InvokeRepeating("UpdateWaveCalculation", 0f, WindUpdateFrequency);
    }

    // update the waves so they change over time, invoked from start
    void UpdateWaveCalculation(){
        CurrentWind = WindGeneration.GetWind(new Vector2Int(0, windCounter));
        targetWindDirection = CurrentWind.windDirection;
        targetWindStrength = CurrentWind.windStrength;

        CalculateWaves(Mathf.Max(CurrentWind.windDirection.x * CurrentWind.windStrength.x, CurrentWind.windDirection.y * CurrentWind.windStrength.y));
        windCounter += 1;
    }

    // updates gerstner shader based on settings
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
        rendererMaterial.SetVector("_WaveLength", WaveLength);
    }

    /// <summary>
    /// Calcualtes waves based on wind
    /// </summary>
    /// <param name="wind">wind sample</param>
    private void CalculateWaves(float wind)
    {
        wind = Mathf.Clamp(wind, -10f, 10f);

        // lerp speed
        float intensity = Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(wind));

        // Shift these to something we can set
        float[] calmSpeeds = { 1f, 0.8f, 1.2f };
        float[] stormySpeeds = { 4f, 3.5f, 5f };

        float[] calmAmplitudes = { 0.2f, 0.15f, 0.25f };
        float[] stormyAmplitudes = { 2.5f, 2.0f, 3f };

        float[] calmFrequencies = { 0.5f, 0.4f, 0.6f };
        float[] stormyFrequencies = { 1.5f, 1.2f, 1.8f };

        // calculate wave properties
        for (int i = 0; i < 3; i++)
        {
            float waveSpeed = Mathf.Lerp(calmSpeeds[i], stormySpeeds[i], intensity);
            float waveAmplitude = Mathf.Lerp(calmAmplitudes[i], stormyAmplitudes[i], intensity);
            float waveFrequency = Mathf.Lerp(calmFrequencies[i], stormyFrequencies[i], intensity);

            // wave direction based on wind
            Vector2 waveDirection = new Vector2(wind, 1f).normalized;

            if(i % 2 == 1) {
                waveDirection.x = -waveDirection.x;
                waveDirection.y = -waveDirection.y;
                waveFrequency = -waveFrequency;
                waveSpeed = -waveSpeed;
            }

            targetDirectionX[i] = waveDirection.x;
            targetDirectionY[i] = waveDirection.y;   // Set y negatove hjere to make sure the waves are always moving different
            targetFrequencies[i] = waveFrequency;
            targetSpeeds[i] = waveSpeed;
            targetAmplitudes[i] = waveAmplitude;
            
        }
    }

    public void AddColorBand(ColorTextureRenderer colorBand)
    {
        ColorBand = colorBand;
    }

    /// <summary>
    /// Sets starting sea level
    /// </summary>
    /// <param name="vertex"></param>
    public void SetDefaultHeight(Vector3 vertex)
    {
        if(rendererMaterial) rendererMaterial.SetFloat("_StartingHeight", vertex.y);
        else startingHeight = vertex.y;
    }
}