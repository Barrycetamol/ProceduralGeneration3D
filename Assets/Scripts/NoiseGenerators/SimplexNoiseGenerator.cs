

using System.Linq;
using UnityEngine;

public class SimplexNoiseGenerator : NoiseGenerator
{
    public override float[] GetNoiseSamples(Vector2Int offsets, Vector2Int gridMeshSize, bool useDeltaTime)
    {
        float[] pixels = new float[gridMeshSize.x *  gridMeshSize.y];

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
                    
                    noiseSample += (OpenSimplex2.Noise2_ImproveX(Seed, xCoord , yCoord ) * 2f - 1f) * amplitude;

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
        return pixels;
    }

    public override float[] NormalizeSamples(float[] noiseMap, float minimum, float maximum)
    {
         for(int i = 0; i < noiseMap.Length; i++){
            float normalizedSample = (noiseMap[i] + 1) / (NormalizationOffsets.x * NormalizationOffsets.y);
            noiseMap[i] = normalizedSample;
        }

        return noiseMap;
    }
}