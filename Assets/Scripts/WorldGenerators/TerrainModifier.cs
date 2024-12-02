using UnityEngine;


public class TerrainModifier : Modifier
{
    public override float ApplyModifier(float startingValue, float noiseSample)
    {
        float curveSample = GetNoiseFromCurve(noiseSample);
        return startingValue == 0 ? curveSample : startingValue * curveSample;
    }

    public override float GetNoiseFromCurve(float noiseSample)
    {
        return Curve.Evaluate(noiseSample);
    }

    void Start(){
        NoiseGenerator = GetComponent<NoiseGenerator>();
    }
}