

using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class NoiseGenerator : MonoBehaviour
{

    [field: SerializeField] public int Seed { get; set; }
    [field: SerializeField] public Vector2Int NoiseSampleSize { get; set; }
    protected float[] pixels;
    protected float timeOffset = 0;
    public abstract void GenerateNoise(bool useDeltaTime);
    public float[] GetNoiseSamples(){return pixels;}
    public float GetNoiseSample(int x, int y){ return pixels[(x % NoiseSampleSize.x) * NoiseSampleSize.y + (y % NoiseSampleSize.y)];}
    void Update(){
        timeOffset += UnityEngine.Time.deltaTime;
    }

}