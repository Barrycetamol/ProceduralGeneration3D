using UnityEngine;


public abstract class Modifier : MonoBehaviour{

    [field: SerializeField] public AnimationCurve Curve;
    public NoiseGenerator NoiseGenerator;

    public abstract float ApplyModifier(float startingValue, float noiseSample);
    public abstract float GetNoiseFromCurve(float noiseSample);
}