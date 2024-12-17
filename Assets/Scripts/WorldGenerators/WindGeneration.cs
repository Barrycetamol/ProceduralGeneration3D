using System;
using Unity.VisualScripting;
using UnityEngine;

public class WindGeneration : MonoBehaviour
{
    [field: Header("Noise Generators")]
    [field: SerializeField] public NoiseGenerator WindDirectionNoiseGenerator{get; set;}
    [field: SerializeField] public NoiseGenerator WindStrengthNoiseGenerator{get; set;}

    [field: Header("Wind properties")]
    [field: SerializeField] public int WindSpeedMultiplier {get; set;} = 1;

    [field: Header("Output directions")]
    
    [field: SerializeField] public Vector2 WindSpeed {get; set;}
    [field: SerializeField] public Vector2 WindDirection {get; set;}
    [field: SerializeField] public Vector2 WindTimeOffsets {get; set;}
    
    public Wind GetWind(Vector2Int position)
    {
        var windspeed = WindStrengthNoiseGenerator.GetNoiseSample(position, false);
        var windDirection = WindDirectionNoiseGenerator.GetNoiseSample(position, false);

        WindSpeed = new Vector2(windspeed * WindSpeedMultiplier, windspeed * WindSpeedMultiplier);
        WindDirection = new Vector2(windDirection, windDirection);

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
