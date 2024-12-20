using System;
using UnityEngine;

/// <summary>
/// Class that handles wind generation
/// </summary>
public class WindGeneration : MonoBehaviour
{
    [field: Header("Noise Generators")]
    [field: SerializeField] public NoiseGenerator WindDirectionXNoiseGenerator{get; set;}
    [field: SerializeField] public NoiseGenerator WindStrengthXNoiseGenerator{get; set;}
    [field: SerializeField] public NoiseGenerator WindDirectionYNoiseGenerator{get; set;}
    [field: SerializeField] public NoiseGenerator WindStrengthYNoiseGenerator{get; set;}

    [field: Header("Wind properties")]
    [field: SerializeField] public int WindSpeedMultiplier {get; set;} = 1; // How fast the wind should move

    [field: Header("Output directions")]
    [field: SerializeField] public Vector2 WindSpeed {get; set;}
    [field: SerializeField] public Vector2 WindDirection {get; set;}
    
    /// <summary>
    /// Samples the noise generator to obtain the current wind direction and strength
    /// </summary>
    /// <param name="position">The position in the world to get the wind at</param>
    /// <returns>wind contained in a Wind struct</returns>
    public Wind GetWind(Vector2Int position)
    {
        var windspeedX = WindStrengthXNoiseGenerator.GetNoiseSample(position, false);
        var windspeedY = WindStrengthYNoiseGenerator.GetNoiseSample(position, false);
        var windDirectionX = WindDirectionXNoiseGenerator.GetNoiseSample(position, true);
        var WindDirectionY = WindDirectionYNoiseGenerator.GetNoiseSample(position , true);

        WindSpeed = new Vector2(windspeedX * WindSpeedMultiplier, windspeedY * WindSpeedMultiplier);
        WindDirection = new Vector2(windDirectionX, WindDirectionY);

        Wind wind = new(WindSpeed, WindDirection);

        return wind;
    }

    /// <summary>
    /// Sets randomwind change to occur
    /// </summary>
    void Start(){
        InvokeRepeating("RandomWindChange", 0, 0.5f);
    }

    /// <summary>
    /// Sets a random wind change based on the WindSpeedMultiplier
    /// </summary>
    void RandomWindChange(){
        var rand = UnityEngine.Random.Range(0, 2);
        if(rand == 0) WindSpeedMultiplier -= 1;
        if(rand == 1) WindSpeedMultiplier += 1;

        WindSpeedMultiplier = Math.Clamp(WindSpeedMultiplier, -5, 5);
    }

    /// <summary>
    /// Stores the noise settings to the respective noise generator
    /// </summary>
    /// <param name="windDirX">noise settings from main menu</param>
    /// <param name="windDirY">noise settings from main menu</param>
    /// <param name="windStrX">noise settings from main menu</param>
    /// <param name="windStrY">noise settings from main menu</param>
    public void SetNoiseSettings(NoiseSettings windDirX, NoiseSettings windDirY, NoiseSettings windStrX, NoiseSettings windStrY){
        GameObject directionX;
        if(windDirX.noiseType == NoiseType.PERLIN) directionX = GameObject.FindGameObjectWithTag("PerlinWindDirX");
        else directionX = GameObject.FindGameObjectWithTag("SimplexWindDirX");

        GameObject directionY;
        if(windDirY.noiseType == NoiseType.PERLIN) directionY = GameObject.FindGameObjectWithTag("PerlinWindDirY");
        else directionY = GameObject.FindGameObjectWithTag("SimplexWindDirY");

        GameObject strX;
        if(windStrX.noiseType == NoiseType.PERLIN) strX = GameObject.FindGameObjectWithTag("PerlinWindStrX");
        else strX = GameObject.FindGameObjectWithTag("SimplexWindStrX");

        GameObject strY;
        if(windStrY.noiseType == NoiseType.PERLIN) strY = GameObject.FindGameObjectWithTag("PerlinWindStrY");
        else strY = GameObject.FindGameObjectWithTag("SimplexWindStrY");

        directionX.GetComponent<NoiseGenerator>().SetSettings(windDirX);
        directionY.GetComponent<NoiseGenerator>().SetSettings(windDirY);
        strX.GetComponent<NoiseGenerator>().SetSettings(windStrX);
        strY.GetComponent<NoiseGenerator>().SetSettings(windStrY);
    }
}

/// <summary>
/// Defines wind paramters, Strength and direction
/// </summary>
public struct Wind{
    public Vector2 windStrength;
    public Vector2 windDirection;

    public Wind(Vector2 strength, Vector2 direction){
        this.windStrength = strength;
        this.windDirection = direction;
    }
}
