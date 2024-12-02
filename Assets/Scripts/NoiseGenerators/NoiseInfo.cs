

using UnityEngine;

public struct NoiseMapInfo{
    public SampleInfo heightSamples;
    public SampleInfo erosionSamples;
    public SampleInfo PVsamples;
    public SampleInfo CombinedValues;

    public NoiseMapInfo(SampleInfo height, SampleInfo erosion, SampleInfo pv){
        this.heightSamples = height;
        this.erosionSamples = erosion;
        this.PVsamples = pv;
        CombinedValues = new SampleInfo();
    }
};

public struct SampleInfo
{
    public float[] noiseMap;
    public float minSample;
    public float maxSample;

    public SampleInfo(float[] noiseSamples, float minimumNoiseSample, float maximumNoiseSample) : this()
    {
        this.noiseMap = noiseSamples;
        this.minSample = minimumNoiseSample;
        this.maxSample = maximumNoiseSample;
    }


    
}