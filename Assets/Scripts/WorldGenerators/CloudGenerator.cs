using System.Linq;
using UnityEngine;

/// <summary>
/// Cloud generation class. Writes noise data to texture based on colour band given
/// </summary>
public class CloudGenerator : MonoBehaviour
{
    /// <summary>
    /// any noise generator
    /// </summary>
    [field: SerializeField] public NoiseGenerator noiseGenerator {get; set;}

    /// <summary>
    /// THe colour bands to apply to the clouds
    /// </summary>
    [field: SerializeField] public ColorTextureRenderer ColorBands {get; set;}

    /// <summary>
    /// wind direction obtained from WindGenerator
    /// </summary>
    [field: SerializeField] public Vector2 WindDirection{get; set;}

    /// <summary>
    /// wind strength obtained from WindGenerator
    /// </summary>
    [field: SerializeField] public Vector2 WindSpeed {get; set;}

    /// <summary>
    /// A separate debug texture renderer
    /// </summary>
    [field: SerializeField] public NoiseTextureRender TextureRender{get; set;}

    /// <summary>
    /// Size of samples 
    /// </summary>
    [field: SerializeField] public Vector2Int NoiseSampleSize { get; set;}

    /// <summary>
    /// The skybox to render to
    /// </summary>
    private Texture2D skyTexture;

    void Start(){
        skyTexture = new Texture2D(NoiseSampleSize.x, NoiseSampleSize.y, TextureFormat.RGBA32, false);
    }

    void Update(){
        GenerateClouds();
    }

    /// <summary>
    /// Generates clouds based on the wind direction, applies the colours and writes to the skytexture
    /// </summary>
    private void GenerateClouds(){
        Vector2Int offsets = new Vector2Int((int)(WindDirection.x * WindSpeed.x), (int) (WindDirection.y * WindDirection.y));
        noiseGenerator.XOffset = offsets.x;
        noiseGenerator.YOffset = offsets.y;
        var samples = noiseGenerator.GetNoiseSamples(offsets, NoiseSampleSize, true);
        samples = Normalize(samples, samples.Min(), samples.Max());
        var colors = ColorBands.GenerateColors(samples, false);

        TextureRender.WritePixelsToRenderTarget(samples, NoiseSampleSize);

        WriteToSkybox(colors);
    }

    public float[] Normalize(float[] samples, float min, float max)
    {
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = Mathf.InverseLerp(min, max, samples[i]);
        }

        return samples;
    }

    /// <summary>
    /// Write to the skytexture
    /// </summary>
    /// <param name="colors">array of colour pixels to write to the skyTexture</param>
    private void WriteToSkybox(Color[] colors)
    {
        var skybox = RenderSettings.skybox;
        if(skybox){
            skyTexture.SetPixels(colors);
            skyTexture.filterMode = FilterMode.Bilinear;
            skyTexture.wrapMode = TextureWrapMode.Mirror;
            skyTexture.Apply();
            skybox.SetTexture("_MainTex", skyTexture);

            // Write to Filesystem
            // byte[] bytes = skyTexture.EncodeToPNG();
            // System.IO.File.WriteAllBytes(Application.dataPath + "/GeneratedSky.png", bytes);
        }
    }

    /// <summary>
    /// Set noise settings from main menu
    /// </summary>
    /// <param name="noiseSettings">noise settings from main menu</param>
    public void SetSettings(NoiseSettings noiseSettings)
    {
        GameObject cloudGen;
        if(noiseSettings.noiseType == NoiseType.PERLIN) cloudGen = GameObject.FindGameObjectWithTag("PerlinClouds");
        else cloudGen = GameObject.FindGameObjectWithTag("SimplexClouds");

        cloudGen.GetComponent<NoiseGenerator>().SetSettings(noiseSettings);
    }
}
