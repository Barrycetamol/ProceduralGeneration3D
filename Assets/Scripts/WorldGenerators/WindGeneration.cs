using System;
using Unity.VisualScripting;
using UnityEngine;

public class WindGeneration : MonoBehaviour
{
    [field: Header("Noise Generators")]
    [field: SerializeField] public NoiseGenerator WindDirectionXNoiseGenerator{get; set;}
    [field: SerializeField] public NoiseGenerator WindStrengthXNoiseGenerator{get; set;}
    [field: SerializeField] public NoiseGenerator WindDirectionYNoiseGenerator{get; set;}
    [field: SerializeField] public NoiseGenerator WindStrengthYNoiseGenerator{get; set;}

    [field: Header("Wind properties")]
    [field: SerializeField] public int WindSpeedMultiplier {get; set;} = 1;

    [field: Header("Output directions")]
    
    [field: SerializeField] public Vector2 WindSpeed {get; set;}
    [field: SerializeField] public Vector2 WindDirection {get; set;}
    [field: SerializeField] public Vector2 WindTimeOffsets {get; set;}
    
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

    void Start(){
        InvokeRepeating("RandomWindChange", 0, 0.5f);
    }

    void RandomWindChange(){
        var rand = UnityEngine.Random.Range(0, 2);
        if(rand == 0) WindSpeedMultiplier -= 1;
        if(rand == 1) WindSpeedMultiplier += 1;

        WindSpeedMultiplier = Math.Clamp(WindSpeedMultiplier, -5, 5);
    }

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

public struct Wind{
    public Vector2 windStrength;
    public Vector2 windDirection;

    public Wind(Vector2 strength, Vector2 direction){
        this.windStrength = strength;
        this.windDirection = direction;
    }
}
