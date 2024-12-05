using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public struct ColorBand{
    [field: SerializeField] public Color color;
    [field: SerializeField] public float threshold;

    public ColorBand(Color desiredColor, float desiredThreshold){
        color = desiredColor;
        threshold = desiredThreshold;

    }
}

public class ColorTextureRenderer  : MonoBehaviour{

    [field: SerializeField] public List<ColorBand> ColorBands {get; set;} = new List<ColorBand>();
    public Color[] Colors{get; set;}

    void Start(){
        //Sort ColorBands by threshold
        SortColorBands();
    }

    public void SortColorBands(){
        ColorBands = ColorBands.OrderBy(colorBand => colorBand.threshold).ToList();
    }


    public Color[] GenerateColors(float[] noiseSamples, bool flat){
        Colors = new Color[noiseSamples.GetLength(0)];

        for(int i = 0; i < noiseSamples.GetLength(0); i++){
            Colors.Append(GenerateColor(noiseSamples[i], flat));
        }  

        return Colors;
    }

    public Color GenerateColor(float noiseSample, bool flat){
        Color returnColor = new Color(noiseSample, noiseSample, noiseSample);
        if(ColorBands.Count == 0) return returnColor;

        float prev_threshold = 0;
        Color prev_color = ColorBands[0].color;
        for(int i = 0; i < ColorBands.Count; i++){
            if(noiseSample <= ColorBands[i].threshold) {
                if(flat) returnColor = ColorBands[i].color;
                else{
                    float t = Mathf.InverseLerp(prev_threshold, ColorBands[i].threshold, noiseSample);
                    returnColor = Color.Lerp(prev_color, ColorBands[i].color, t);
                } 
                break;
            }
            prev_threshold = ColorBands[i].threshold;
            prev_color = ColorBands[i].color;
        }

        return returnColor;
    }

    public Color GenerateColorGradient(float noiseSample){
        return new Color();
    }

    


}