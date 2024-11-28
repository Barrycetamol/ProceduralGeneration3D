using System;
using System.Collections.Generic;
using UnityEngine;


public class TerrainGenerator : MonoBehaviour
{
    [field: SerializeField] public Vector2Int GridSize{get; set;}
    [field: SerializeField] public Vector2Int LandSize { get; set; }
    [field: SerializeField] public float MaximumHeight;
    [field: SerializeField] public float MinimumHeight;
    [field: SerializeField] public float MeshHeightMultiplyer;
    [field: SerializeField] public AnimationCurve MeshHeightCurve;
    [field: SerializeField] public AnimationCurve PeaksAndValleys;
    [field: SerializeField] public AnimationCurve Erosion;
    [field: SerializeField] public AnimationCurve Contentinentalness;

    [field: SerializeField] public bool AutoUpdate;

    private NoiseGenerator NoiseGenerator { get; set; }
    private NoiseGenerator ErosionNoise {get; set;}
    private NoiseGenerator PeaksAndValleysNoise{get; set;}
    private NoiseGenerator ContinentalnessNoise{get; set;}
    private ColorTextureRenderer ColorTextureRenderer {get; set;}

    private Terrain Land { get; set; }
    private List<Terrain> Terrains{get; set;} = new List<Terrain>();

    

    void Start()
    {
        NoiseGenerator = GetComponent<NoiseGenerator>();
        ColorTextureRenderer = GetComponent<ColorTextureRenderer>();

        // Clamp for min size.
        LandSize = new Vector2Int(Math.Clamp(LandSize.x, 1, 128), Math.Clamp(LandSize.y, 1, 128));
        GridSize = new Vector2Int(Math.Max(1, GridSize.x), Math.Max(1, GridSize.y));  // Ensure 1x1 square


    
        if (NoiseGenerator != null){

            if(AutoUpdate){
                InvokeRepeating("StartGenerating", 1.0f, 4.0f);
            }
            else StartGenerating();
        }
    }

    public void StartGenerating(){
        // ErosionNoise.GenerateNoise(false);
        // PeaksAndValleysNoise.GenerateNoise(false);
    // ContinentalnessNoise.GenerateNoise(false);
        foreach(Terrain terrain in Terrains){
            Debug.Log($"Clearing terrain {terrain.m_Terrain.name}");
            Destroy(terrain.m_Terrain);
        }

        Terrains.Clear();
        
        for(int i = 0; i < GridSize.x; i++){
            for(int j = 0; j < GridSize.y; j++){
                Terrains.Add(new Terrain($"Land_{i}x{j}", LandSize, new Vector2Int( i, j), false));
            }
        }
        GenerateWorld();
    }


    private void GenerateWorld()
    {
        ColorTextureRenderer.SortColorBands();
        foreach(Terrain terrain in Terrains){
            Debug.Log($"Staring terrain generation for: {terrain.m_Terrain.name}");
            GenerateTerrain(terrain);
        }
 
    }

    private void GenerateTerrain(Terrain terrain)
    {
        terrain.Clear();
        terrain.SetVertices(GenerateVertices(terrain.GridPosition, terrain.GridSize, MeshHeightCurve, MeshHeightMultiplyer));
        terrain.SetColors(GetColors(terrain.Vertices, terrain.GridPosition, terrain.GridSize, false));
        terrain.Refresh();
    }

    private Vector3[,] GenerateVertices(Vector2Int gridPosition, Vector2Int gridSize, AnimationCurve height, float heightMultiplier)
    {
        Vector3[,] vertices = new Vector3[gridSize.x, gridSize.y];
        float[] noiseSamples = NoiseGenerator.GetNoiseSamples(gridPosition, gridSize, false);
        Debug.Log($"noiseSamples count: {noiseSamples.Length}. VerticesX {vertices.GetLength(0)}, {vertices.GetLength(1)}"); 
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                
                //vertices[i, j] = new Vector3(i, Mathf.Lerp(minHeight, maxHeight, noiseSample), j);
                float heightCalc = height.Evaluate(noiseSamples[i * gridSize.y + j]) * heightMultiplier;
                float vertexResult = heightCalc;
                vertices[i, j] = new Vector3(i, vertexResult, j);
            }
        }

        return vertices;
    }

    private List<Color> GetColors(Vector3[,] Vertices, Vector2Int gridPosition, Vector2Int gridSize, bool flat){
        List<Color> colors = new List<Color>();
        float[] noiseSamples = NoiseGenerator.GetNoiseSamples(gridPosition, gridSize, false);

         for(int i = 0; i < Vertices.GetLength(1) - 1; i++){
            for(int j = 0; j < Vertices.GetLength(0) - 1; j++){
                float noiseSampleBottomLeft  = noiseSamples[i * gridSize.y + j];
                float noiseSampleTopLeft     = noiseSamples[i * gridSize.y + (j + 1)];
                float noiseSampleBottomRight = noiseSamples[(i + 1) * gridSize.y + j];
                float noiseSampleTopRight    = noiseSamples[(i + 1) * gridSize.y + (j + 1)];
                
                Color bottomLeftColor  = ColorTextureRenderer.GenerateColor(noiseSampleBottomLeft, flat);
                Color topLeftColor     = ColorTextureRenderer.GenerateColor(noiseSampleTopLeft, flat);
                Color bottomRightColor = ColorTextureRenderer.GenerateColor(noiseSampleBottomRight, flat);
                Color topRightColor    = ColorTextureRenderer.GenerateColor(noiseSampleTopRight, flat);
  
                colors.Add(bottomLeftColor);
                colors.Add(topLeftColor);
                colors.Add(bottomRightColor);
                colors.Add(topRightColor);
                

            }
        }
        return colors;
    }
}