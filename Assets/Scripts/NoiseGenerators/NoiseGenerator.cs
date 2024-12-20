using System;
using UnityEngine;

public abstract class NoiseGenerator : MonoBehaviour
{
    [field: Header("Noise Generator")]
    [field: SerializeField] public int Seed { get; set; }
    [field: SerializeField] public int XOffset {get; set;}
    [field: SerializeField] public int YOffset {get; set;}
    [field: SerializeField] public float TimeMultiplier {get; set;} = 1.0f;

    [field: SerializeField] public uint Octaves {get; set;}
    [field: SerializeField] public float Persistance { get; set; }
    [field: SerializeField] public float Lacunarity { get; set; }
    [field: SerializeField] public float Scale { get; set; }
    
    [field: SerializeField] public Vector2 NormalizationOffsets { get; set; }
    
    [field: Header("Debug information")]
    public float minimumNoiseSample = float.MaxValue;
    public float maximumNoiseSample = float.MinValue;
    protected float timePassed = 0;

    public abstract float[] GetNoiseSamples(Vector2Int offsets, Vector2Int sampleSize, bool useDeltaTime);
    public abstract float[] NormalizeSamples(float[] pixels, float minimum, float maximum);
    public float GetNoiseSample(Vector2Int offsets, bool useDeltaTime){
        return GetNoiseSamples(offsets, new Vector2Int(1, 1), useDeltaTime)[0];
    }

    void Update(){
        TimeMultiplier = Mathf.Clamp(TimeMultiplier, 0.01f, 100.0f);
        timePassed += UnityEngine.Time.deltaTime * TimeMultiplier;
    }

    public void SetSettings(NoiseSettings settings)
    {
        Seed = Math.Clamp(settings.seed, -10000, 10000);
        Octaves = (uint) Math.Clamp( settings.octaves, 1, 200);
        Lacunarity = Mathf.Clamp( settings.lacunarity, 0.01f, 10.0f);
        Scale = Mathf.Clamp( settings.scale, 1.0f, 1000);
        Persistance = Mathf.Clamp( settings.persistance, 0.1f, 5.0f);

        
    }
}