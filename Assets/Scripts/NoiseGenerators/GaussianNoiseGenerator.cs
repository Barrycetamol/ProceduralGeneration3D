using System;
using UnityEngine;


public class GuassianNoiseGenerator : NoiseGenerator
{
    [field: SerializeField] public double Mean {get; set;}
    [field: SerializeField] public double StandardDeviation {get; set;}

    private bool hasSpare = false;
    private double spare = 0;
    public override void GenerateNoise(bool useDeltaTime)
    {
        float timeOffset = 1;
        if(useDeltaTime) timeOffset = timePassed;
        pixels = new float[NoiseSampleSize.x * NoiseSampleSize.y];
        UnityEngine.Random.InitState(Seed);
        for (int i = 0; i < NoiseSampleSize.x; i++)
        {
            for (int j = 0; j < NoiseSampleSize.y; j++)
            {
                float sample = (float) GenerationGuassian();  // casting here may cause artifacts
                pixels[i * NoiseSampleSize.y + j] = sample * timeOffset;

            }
        }
        Debug.Log($"Finished generting guassian noise");
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
}