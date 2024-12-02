using System;
using UnityEngine;


public class GuassianNoiseGenerator : NoiseGenerator
{
    [field: SerializeField] public double Mean {get; set;}
    [field: SerializeField] public double StandardDeviation {get; set;}

    private bool hasSpare = false;
    private double spare = 0;
    public override float[] GetNoiseSamples(Vector2Int offsets, Vector2Int sampleSize, bool useDeltaTime)
    {
        float timeOffset = 1;
        if(useDeltaTime) timeOffset = timePassed;
        float[] pixels = new float[sampleSize.x *  sampleSize.y];
        UnityEngine.Random.InitState(Seed);
        for (int i = 0; i < sampleSize.x; i++)
        {
            for (int j = 0; j < sampleSize.y; j++)
            {
                float sample = (float) GenerationGuassian();  // casting here may cause artifacts
                pixels[i * sampleSize.y + j] = sample * timeOffset;

            }
        }
        Debug.Log($"Finished generting guassian noise");
        return pixels;
    }
 
    // Taken from: https://en.wikipedia.org/wiki/Marsaglia_polar_method   C++ section
    //  Edited to work with unity
    double GenerationGuassian() {
        if (hasSpare) {
            hasSpare = false;
            return spare * StandardDeviation + Mean;
        } else {
            double u, v, s;
            do {
                u = 2.0 * UnityEngine.Random.value - 1.0;
                v = 2.0 * UnityEngine.Random.value - 1.0;
                s = u * u + v * v;
            } while (s >= 1.0 || s == 0.0);
            s = Math.Sqrt(-2.0 * Math.Log(s) / s);
            spare = v * s;
            hasSpare = true;
            return Mean + StandardDeviation * u * s;
        }
    }

    public override float[] NormalizeSamples(float[] noiseMap, float minimum, float maximum){return new float[0];}
}