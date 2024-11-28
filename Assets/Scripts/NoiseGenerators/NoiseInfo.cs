

public struct NoiseMapInfo{
    public float[] noiseMap;
    public float minSample;
    public float maxSample;

    public NoiseMapInfo(float[] noiseSamples, float minimumNoiseSample, float maximumNoiseSample) : this()
    {
        this.noiseMap = noiseSamples;
        this.minSample = minimumNoiseSample;
        this.maxSample = maximumNoiseSample;
    }
};