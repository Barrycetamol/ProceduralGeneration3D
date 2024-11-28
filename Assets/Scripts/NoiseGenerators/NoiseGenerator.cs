

using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class NoiseGenerator : MonoBehaviour
{

    [field: SerializeField] public int Seed { get; set; }
    [field: SerializeField] public Vector2Int NoiseSampleSize { get; set; }
    [field: SerializeField] public int XOffset {get; set;}
    [field: SerializeField] public int YOffset {get; set;}

    public float minimumNoiseSample = float.MaxValue;
    public float maximumNoiseSample = float.MinValue;
    protected float timePassed = 0;

    public abstract float[] GetNoiseSamples(Vector2Int offsets, Vector2Int sampleSize, bool useDeltaTime);
    public abstract float[] NormalizeSamples(float[] pixels, float min, float max);
    public float GetNoiseSample(Vector2Int offsets, Vector2Int sampleSize, bool useDeltaTime){
        return GetNoiseSamples(offsets, sampleSize, useDeltaTime)[0];
    }

    void Update(){
        timePassed += UnityEngine.Time.deltaTime;
    }

}