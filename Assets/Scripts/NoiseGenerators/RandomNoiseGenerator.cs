using UnityEngine;

public class RandomNoiseGenerator : NoiseGenerator
{
    public override float[] GetNoiseSamples(Vector2Int offsets, Vector2Int sampleSize, bool useDeltaTime)
    {
        float[] pixels = new float[sampleSize.x *  sampleSize.y];
        for (int i = 0; i < sampleSize.x; i++)
        {
            for (int j = 0; j < sampleSize.y; j++)
            {
                pixels[i * sampleSize.y + j] = UnityEngine.Random.Range(0, 1.0f);
            }
        }
        return pixels;

    }

    public override float[] NormalizeSamples(float[] noiseMap, float minimum, float maximum){return new float[0];}
}