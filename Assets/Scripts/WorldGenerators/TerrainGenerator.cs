using System;
using System.Collections.Generic;
using UnityEngine;


public class TerrainGenerator : MonoBehaviour
{
    [field: SerializeField] public Vector2Int Size { get; set; }
    [field: SerializeField] public float MaximumHeight;
    [field: SerializeField] public float MinimumHeight;
    [field: SerializeField] public float MeshHeightMultiplyer;
    [field: SerializeField] public AnimationCurve MeshHeightCurve;

    [field: SerializeField] public bool AutoUpdate;

    private NoiseGenerator NoiseGenerator { get; set; }
    private ColorTextureRenderer ColorTextureRenderer {get; set;}

    private Terrain Land { get; set; }

    

    void Start()
    {
        NoiseGenerator = GetComponent<NoiseGenerator>();
        ColorTextureRenderer = GetComponent<ColorTextureRenderer>();

        // Clamp for min size.
        Size = new Vector2Int(Math.Clamp(Size.x, 1, 128), Math.Clamp(Size.y, 1, 128));

        Land = new Terrain("Land", Size.x, Size.y, false);
        if (NoiseGenerator != null){

            if(AutoUpdate){
                InvokeRepeating("StartGenerating", 1.0f, 4.0f);
            }
            else StartGenerating();
        }
    }

    public void StartGenerating(){
        NoiseGenerator.GenerateNoise(false);
        GenerateWorld();
    }


    private void GenerateWorld()
    {
        ColorTextureRenderer.SortColorBands();
        GenerateTerrain(Land);
    }

    private void GenerateTerrain(Terrain terrain)
    {
        terrain.Clear();
        terrain.SetVertices(GenerateVertices(MinimumHeight, MaximumHeight, MeshHeightCurve, MeshHeightMultiplyer));
        terrain.SetColors(GetColors(terrain.Vertices));
        terrain.Refresh();
    }

    private Vector3[,] GenerateVertices(float minHeight, float maxHeight, AnimationCurve height, float heightMultiplier)
    {
        Vector3[,] vertices = new Vector3[Size.x + 1, Size.y + 1];
        for (int i = 0; i <= Size.x; i++)
        {
            for (int j = 0; j <= Size.y; j++)
            {
                float noiseSample = NoiseGenerator.GetNoiseSample(i, j);
                //vertices[i, j] = new Vector3(i, Mathf.Lerp(minHeight, maxHeight, noiseSample), j);   
                vertices[i, j] = new Vector3(i, height.Evaluate(noiseSample) * heightMultiplier, j);
            }
        }

        return vertices;
    }

    private List<Color> GetColors(Vector3[,] Vertices){
        List<Color> colors = new List<Color>();

         for(int i = 0; i < Vertices.GetLength(1) - 1; i++){
            for(int j = 0; j < Vertices.GetLength(0) - 1; j++){
                float noiseSampleBottomLeft  = NoiseGenerator.GetNoiseSample(i, j);
                float noiseSampleTopLeft     = NoiseGenerator.GetNoiseSample(i, j + 1);
                float noiseSampleBottomRight = NoiseGenerator.GetNoiseSample(i + 1, j);
                float noiseSampleTopRight    = NoiseGenerator.GetNoiseSample(i + 1, j + 1);

                colors.Add(ColorTextureRenderer.GenerateColor(noiseSampleBottomLeft));
                colors.Add(ColorTextureRenderer.GenerateColor(noiseSampleTopLeft));
                colors.Add(ColorTextureRenderer.GenerateColor(noiseSampleBottomRight));
                colors.Add(ColorTextureRenderer.GenerateColor(noiseSampleTopRight));
            }
        }
        return colors;
    }
}