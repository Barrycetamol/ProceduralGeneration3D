using UnityEngine;

/// <summary>
/// Class that handles terrain modification and reading the associated animation curve to get its value
/// </summary>
public class TerrainModifier : Modifier
{
    /// <summary>
    /// Applies an additional modifier based on the starting value given
    /// </summary>
    /// <param name="startingValue">the initial value to apply the noise curve to</param>
    /// <param name="noiseSample">the noise to read from the animation curve</param>
    /// <returns>starting value + noise modifier</returns>
    public override float ApplyModifier(float startingValue, float noiseSample)
    {
        float curveSample = GetNoiseFromCurve(noiseSample);
        return startingValue == 0 ? curveSample : startingValue * curveSample;
    }

    /// <summary>
    /// Wrapper for animation curves evaluate
    /// </summary>
    /// <param name="noiseSample">a noise sample</param>
    /// <returns>output from curve.evaluate</returns>
    public override float GetNoiseFromCurve(float noiseSample)
    {
        return Curve.Evaluate(noiseSample);
    }
    
    void Start(){
        NoiseGenerator = GetComponent<NoiseGenerator>();
    }
}