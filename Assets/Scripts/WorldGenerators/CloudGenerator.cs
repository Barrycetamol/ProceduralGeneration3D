using System.Linq;
using UnityEngine;

public class CloudGenerator : MonoBehaviour
{
    [field: SerializeField] public NoiseGenerator noiseGenerator {get; set;}
    [field: SerializeField] public ColorTextureRenderer ColorBands {get; set;}
    [field: SerializeField] public Vector2 WindDirection{get; set;}
    [field: SerializeField] public Vector2 WindSpeed {get; set;}
    [field: SerializeField] public NoiseTextureRender TextureRender{get; set;}
    [field: SerializeField] public Vector2Int NoiseSampleSize { get; set;}
    private Texture2D skyTexture;

    void Start(){
        skyTexture = new Texture2D(NoiseSampleSize.x, NoiseSampleSize.y, TextureFormat.RGBA32, false);
    }

    void Update(){
        GenerateClouds();
    }

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

    private void WriteToSkybox(Color[] colors)
    {
        var skybox = RenderSettings.skybox;
        if(skybox){
            skyTexture.SetPixels(colors);
            skyTexture.filterMode = FilterMode.Bilinear; // Optional: Improves quality
            skyTexture.wrapMode = TextureWrapMode.Repeat; // Prevents artifacts at edges
            skyTexture.Apply();
            skybox.SetTexture("_MainTex", skyTexture);

            // Write to Filesystem
            // byte[] bytes = skyTexture.EncodeToPNG();
            // System.IO.File.WriteAllBytes(Application.dataPath + "/GeneratedSky.png", bytes);
        }
    }
}
