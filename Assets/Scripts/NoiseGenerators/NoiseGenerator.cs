

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
    protected float[] pixels;
    protected float timePassed = 0;

    public abstract float[] GetNoiseSamples(Vector2Int offsets, Vector2Int sampleSize, bool useDeltaTime);
    public float GetNoiseSample(Vector2Int offsets, Vector2Int sampleSize, bool useDeltaTime){
        return GetNoiseSamples(offsets, sampleSize, useDeltaTime)[0];
    }
    public float[] GetLastNoiseSamples(){return pixels;}

    void Update(){
        timePassed += UnityEngine.Time.deltaTime;
    }

}