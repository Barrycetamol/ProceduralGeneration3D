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
    [field: SerializeField] public int Octaves {get; set;}
    [field: SerializeField] public float Lacunarity {get; set;}
    [field: SerializeField] public float Persistance {get; set;}
    [field: SerializeField] public Vector2 NormalizationOffsets {get; set;}

    public PerlinNoiseGenerator(int seed, float scale, int widthResolution, int heightResolution, int xOffset, int yOffset, float lacunarity, int octaves, float persistance)
    {
        NoiseSampleSize = new Vector2Int(widthResolution, heightResolution);
        Seed = seed;
        XOffset = xOffset;
        YOffset = yOffset;
        Scale = Math.Max(scale, 1.0001f); // Scale must be positive
        Lacunarity = Math.Max(1, lacunarity);
        Octaves = Math.Max(0, octaves);
        Persistance = Math.Clamp(persistance, 0.0f, 1.0f);

    }

    void Start(){
        Scale = Math.Max(Scale, 1.0001f); // Scale must be positive
        Lacunarity = Math.Max(1, Lacunarity);
        Octaves = Math.Max(1, Octaves);
        Persistance = Math.Clamp(Persistance, 0.01f, 1.0f);
    }

    public override float[] GetNoiseSamples(Vector2Int offsets, Vector2Int gridMeshSize, bool useDeltaTime)
    {
        float[] pixels = new float[gridMeshSize.x *  gridMeshSize.y];
        Debug.Log($"Size of pixels in noise generation: {pixels.Length}");

        float chunkXoffset = offsets.x * (gridMeshSize.x - 1) + XOffset;
        float chunkYoffset = offsets.y * (gridMeshSize.y - 1) + YOffset;

        float timeOffset = 0;
        float xCoord;
        float yCoord;

        if(useDeltaTime) timeOffset = timePassed;
        for (int i = 0 ; i < gridMeshSize.x; i++)
        {
            for (int j = 0 ; j < gridMeshSize.y; j++)
            {
                float amplitude = 1;
                float freq = 1;
                float noiseSample = 0;
                float globalXcoord = (float) i + chunkXoffset;
                float globalYcoord = (float) j + chunkYoffset;
                for(int k = 0; k < Octaves; k++){
                    xCoord =  ( globalXcoord + Seed + timeOffset) / Scale * freq;
                    yCoord =  ( globalYcoord + Seed + timeOffset) / Scale * freq;
                    
                    noiseSample += (Mathf.PerlinNoise(xCoord , yCoord ) * 2f - 1f) * amplitude;

                    amplitude *= Persistance;
                    freq *= Lacunarity;
                }
                pixels[i * gridMeshSize.y + j] = noiseSample;
            }
        }

        // Consider optimising this section, since we have to loop it 3 times to get the value
        minimumNoiseSample = pixels.Min();
        maximumNoiseSample = pixels.Max();

        if(minimumNoiseSample == maximumNoiseSample) Debug.LogError("Noise generation didnt work. All pixel values were the same.");
        Debug.Log($"Finished generating perlin noise.");
        return pixels;
       
    }


    public override float[] NormalizeSamples(float[] noiseMap, float min, float max){
         for(int i = 0; i < noiseMap.Length; i++){
            float normalizedSample = (noiseMap[i] + 1) / (NormalizationOffsets.x * NormalizationOffsets.y);
            //noiseMap[i] = Mathf.InverseLerp(min, max, noiseMap[i]);
            noiseMap[i] = normalizedSample;
        }

        return noiseMap;
    }

}