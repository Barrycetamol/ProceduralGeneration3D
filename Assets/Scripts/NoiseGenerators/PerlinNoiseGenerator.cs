using System;
using System.Linq;
using UnityEngine;


/// <summary>
/// perlin noise generator based off of: https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html
/// </summary>
/// 
[Serializable]
public class PerlinNoiseGenerator : NoiseGenerator
{
    [field: SerializeField] public float Scale { get; set; }
    [field: SerializeField] public float XOffset { get; set; }
    [field: SerializeField] public float YOffset { get; set; }
    [field: SerializeField] public int Octaves {get; set;}
    [field: SerializeField] public float Lacunarity {get; set;}
    [field: SerializeField] public float Persistance {get; set;}
    public PerlinNoiseGenerator(int seed, float scale, int widthResolution, int heightResolution, int xOffset, int yOffset, float lacunarity, int octaves, float persistance)
    {
        NoiseSampleSize = new Vector2Int(widthResolution, heightResolution);
        Seed = seed;
        XOffset = xOffset;
        YOffset = yOffset;
        Scale = Math.Max(scale, 0.0001f); // Scale must be positive
        Lacunarity = Math.Min(1, lacunarity);
        Octaves = Math.Min(0, octaves);
        Persistance = Math.Clamp(persistance, 0.0f, 1.0f);

    }

    void Start(){
        Scale = Math.Max(Scale, 0.0001f); // Scale must be positive
        Lacunarity = Math.Min(1, Lacunarity);
        Octaves = Math.Min(1, Octaves);
        Persistance = Math.Clamp(Persistance, 0.01f, 1.0f);
    }

    public override void GenerateNoise(bool useDeltaTime)
    {
        pixels = new float[NoiseSampleSize.x * NoiseSampleSize.y];

        float timeOffset = 1;
        float xCoord;
        float yCoord;

        if(useDeltaTime) timeOffset = timePassed;
        for (int i = 0; i < NoiseSampleSize.x; i++)
        {
            for (int j = 0; j < NoiseSampleSize.y; j++)
            {
                float amplitude = 1;
                float freq = 1;
                float noiseSample = 0;
                for(int k = 0; k < Octaves; k++){
                    xCoord = (XOffset + timeOffset + Seed) + (float)i / NoiseSampleSize.x * Scale * freq;
                    yCoord = (YOffset + timeOffset + Seed) + (float)j / NoiseSampleSize.y * Scale * freq;
                
                    noiseSample += (Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1) * amplitude;

                    amplitude *= Persistance;
                    freq *= Lacunarity;
                }
                pixels[i * NoiseSampleSize.y + j] = noiseSample;
            }
        }

        // Consider optimising this section, since we have to loop it 3 times to get the value
        float min = pixels.Min();
        float max = pixels.Max();

        // normalize the output to be between 0 and 1
        for(int i = 0; i < pixels.Length; i++){
            pixels[i] = Mathf.InverseLerp(min, max, pixels[i]);
        }
        Debug.Log($"Finished generating perlin noise");
    }
}