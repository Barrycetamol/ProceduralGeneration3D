using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores settings from the generation menu
/// </summary>
public class SettingGatherer : MonoBehaviour
{
    [field: SerializeField] public NoiseSettings noiseSettings;
    public GameObject seed;
    public GameObject scale;
    public GameObject octaves;
    public GameObject persistance;
    public GameObject lacunarity;
    public GameObject noiseType;


    void Start()
    {
        noiseSettings = new NoiseSettings(0, 0, 0, 0, 0, NoiseType.PERLIN);
    }

    // Setters
    public void UpdateSeed(String value)
    {
        int.TryParse(value, out noiseSettings.seed);
    }

    public void UpdateScale(String value)
    {
        float.TryParse(value, out noiseSettings.scale);
    }

    public void UpdateOctaves(String value)
    {
        int.TryParse(value, out noiseSettings.octaves);
    }
    public void UpdatePersistance(String value)
    {
        float.TryParse(value, out noiseSettings.persistance);
    }
    public void UpdateLacunarity(String value)
    {
        float.TryParse(value, out noiseSettings.lacunarity);
    }
    public void UpdateNoiseType(int value)
    {
        switch (value)
        {
            case 0:
                noiseSettings.noiseType = NoiseType.PERLIN;
                break;
            case 1:
                noiseSettings.noiseType = NoiseType.SIMPLEX;
                break;

        }
    }

    public NoiseSettings GetSettings()
    {
        return noiseSettings;
    }


}

[Serializable]
public struct NoiseSettings
{
    public int seed;
    public int octaves;
    public float scale;
    public float persistance;
    public float lacunarity;
    public NoiseType noiseType;
    public NoiseSettings(int seed, int octaves, float scale, float persistance, float lacunarity, NoiseType noiseType)
    {
        this.seed = seed;
        this.octaves = octaves;
        this.scale = scale;
        this.persistance = persistance;
        this.lacunarity = lacunarity;
        this.noiseType = noiseType;
    }
}

public enum NoiseType
{
    PERLIN,
    SIMPLEX
}
