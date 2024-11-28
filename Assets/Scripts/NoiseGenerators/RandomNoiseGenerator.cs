using UnityEngine;

public class RandomNoiseGenerator : NoiseGenerator
{
    public override float[] GetNoiseSamples(Vector2Int offsets, Vector2Int sampleSize, bool useDeltaTime)
    {
        NoiseSampleSize = sampleSize;
        float[] pixels = new float[NoiseSampleSize.x * NoiseSampleSize.y];
        for (int i = 0; i < NoiseSampleSize.x; i++)
        {
            for (int j = 0; j < NoiseSampleSize.y; j++)
            {
                pixels[i * NoiseSampleSize.y + j] = UnityEngine.Random.Range(0, 1.0f);
            }
        }
        return pixels;

    }

    public override float[] NormalizeSamples(float[] noiseMap, float min, float max){return new float[0];}
}