using UnityEngine;

public class RandomNoiseGenerator : NoiseGenerator
{
    public override void GenerateNoiseTexture(bool useDeltaTime)
    {
        pixels = new float[NoiseSampleSize.x * NoiseSampleSize.y];
        for (int i = 0; i < NoiseSampleSize.x; i++)
        {
            for (int j = 0; j < NoiseSampleSize.y; j++)
            {
                pixels[i * NoiseSampleSize.y + j] = UnityEngine.Random.Range(0, 1.0f);
            }
        }
    }
}