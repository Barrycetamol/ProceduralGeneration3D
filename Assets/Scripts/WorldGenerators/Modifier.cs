using UnityEngine;

/// <summary>
/// Base class for modifiers. See terrain modifier for an example
/// </summary>
public abstract class Modifier : MonoBehaviour{

    [field: SerializeField] public AnimationCurve Curve;
    public NoiseGenerator NoiseGenerator;

    public abstract float ApplyModifier(float startingValue, float noiseSample);
    public abstract float GetNoiseFromCurve(float noiseSample);
}