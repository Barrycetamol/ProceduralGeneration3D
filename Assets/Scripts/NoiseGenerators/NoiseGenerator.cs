using UnityEngine;

public abstract class NoiseGenerator : MonoBehaviour
{
    [field: Header("Noise Generator")]
    [field: SerializeField] public int Seed { get; set; }
    [field: SerializeField] public int XOffset {get; set;}
    [field: SerializeField] public int YOffset {get; set;}
    [field: SerializeField] public float TimeMultiplier {get; set;} = 1.0f;
    
    [field: SerializeField] public Vector2 NormalizationOffsets { get; set; }
    
    [field: Header("Debug information")]
    public float minimumNoiseSample = float.MaxValue;
    public float maximumNoiseSample = float.MinValue;
    protected float timePassed = 0;

    public abstract float[] GetNoiseSamples(Vector2Int offsets, Vector2Int sampleSize, bool useDeltaTime);
    public abstract float[] NormalizeSamples(float[] pixels, float minimum, float maximum);
    public float GetNoiseSample(Vector2Int offsets, bool useDeltaTime){
        return GetNoiseSamples(offsets, new Vector2Int(1, 1), useDeltaTime)[0];
    }

    void Update(){
        TimeMultiplier = Mathf.Clamp(TimeMultiplier, 0.01f, 100.0f);
        timePassed += UnityEngine.Time.deltaTime * TimeMultiplier;
    }

}