using System;
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
    [field: SerializeField] public int Lacunarity {get; set;}
    public PerlinNoiseGenerator(int seed, int widthResolution, int heightResolution, int xOffset, int yOffset, int lacunarity, int octaves)
    {
        Seed = seed;
        NoiseSampleSize = new Vector2Int(widthResolution, heightResolution);
        XOffset = xOffset;
        YOffset = yOffset;
        Scale = Math.Max(Scale, 0.0001f); // Scale must be positive
        Lacunarity = Math.Min(1, lacunarity);
        Octaves = Math.Min(0, octaves);

    }

    public override void GenerateNoise(bool useDeltaTime)
    {
        pixels = new float[NoiseSampleSize.x * NoiseSampleSize.y];
        float noiseSample;
        float xCoord;
        float yCoord;
        for (int i = 0; i < NoiseSampleSize.x; i++)
        {
            for (int j = 0; j < NoiseSampleSize.y; j++)
            {
                if (useDeltaTime)
                {
                    xCoord = (XOffset + (float)i + timeOffset + Seed) / NoiseSampleSize.x * Scale;
                    yCoord = (YOffset + (float)j + timeOffset + Seed) / NoiseSampleSize.y * Scale;
                }
                else
                {
                    xCoord = (XOffset + (float)i + Seed) / NoiseSampleSize.x * Scale;
                    yCoord = (YOffset + (float)j + Seed) / NoiseSampleSize.y * Scale;
                }
                noiseSample = Mathf.PerlinNoise(xCoord, yCoord);
                pixels[i * NoiseSampleSize.y + j] = noiseSample;
            }
        }
        Debug.Log($"Finished generating perlin noise");
    }
}