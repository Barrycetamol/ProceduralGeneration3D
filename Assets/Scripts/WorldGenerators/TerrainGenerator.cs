using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
    [field: SerializeField] public Vector2Int GridSize{get; set;}
    [field: SerializeField] public Vector2Int LandSize { get; set; }
    [field: SerializeField] public float MaximumHeight;
    [field: SerializeField] public float MinimumHeight;
    [field: SerializeField] public float MeshHeightMultiplyer;
    [field: SerializeField] public AnimationCurve MeshHeightCurve;
    [field: SerializeField] public AnimationCurve PeaksAndValleysCurve;
    [field: SerializeField] public AnimationCurve ErosionCurve;
    [field: SerializeField] public AnimationCurve ContentinentalnessCurve;

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

        // Create noise maps for each terrain segment
        Dictionary<string, NoiseMapInfo> noiseMaps = new Dictionary<string, NoiseMapInfo>();
        float minimum = float.MinValue;
        float maximum = float.MaxValue;
        foreach(Terrain terrain in Terrains){
            float[] noiseSamples = NoiseGenerator.GetNoiseSamples(terrain.GridPosition, terrain.GridSize, false);
            noiseMaps[terrain.m_Terrain.name] = new NoiseMapInfo(noiseSamples, NoiseGenerator.minimumNoiseSample, NoiseGenerator.maximumNoiseSample);
            
            // get min/max values from our generated samples
            if(noiseMaps[terrain.m_Terrain.name].minSample < minimum) minimum = noiseMaps[terrain.m_Terrain.name].minSample;
            if(noiseMaps[terrain.m_Terrain.name].maxSample > maximum) maximum = noiseMaps[terrain.m_Terrain.name].maxSample;
        }

        float[] prev = new float[0];
        string prev_name = "";
        foreach(Terrain terrain in Terrains){
            var a = noiseMaps[terrain.m_Terrain.name].noiseMap;
            if(prev.Length >= 0){
                if(prev.Length == a.Length && prev.SequenceEqual(a)){
                    Debug.LogError($"Terrains were equal before normalization: {prev_name} {terrain.m_Terrain.name}");
                }
                else{
                    prev = a;
                    prev_name = terrain.m_Terrain.name;
                }
            }
            else {
                prev = noiseMaps[terrain.m_Terrain.name].noiseMap;
                prev_name = terrain.m_Terrain.name;
            }
        }


        // Normalize heights based on our min/maxes of each grid
        foreach(Terrain terrain in Terrains){
            noiseMaps[terrain.m_Terrain.name] = new NoiseMapInfo(
                    NoiseGenerator.NormalizeSamples(
                        noiseMaps[terrain.m_Terrain.name].noiseMap, 
                        minimum, 
                        maximum), 
                    minimum, maximum
            );
        }

        prev = new float[0];
        prev_name = "";
        foreach(Terrain terrain in Terrains){
            var a = noiseMaps[terrain.m_Terrain.name].noiseMap;
            if(prev.Length >= 0){
                if(prev.Length == a.Length && prev.SequenceEqual(a)){
                    Debug.LogError($"Terrains were equal after normalization: {prev_name} {terrain.m_Terrain.name}");
                }
                else{
                    prev = a;
                    prev_name = terrain.m_Terrain.name;
                }
            }
            else {
                prev = noiseMaps[terrain.m_Terrain.name].noiseMap;
                prev_name = terrain.m_Terrain.name;
            }
        }


        foreach(Terrain terrain in Terrains){
            Debug.Log($"Staring terrain generation for: {terrain.m_Terrain.name}");
            GenerateTerrain(terrain, noiseMaps[terrain.m_Terrain.name].noiseMap);
        }
 
    }

    private void GenerateTerrain(Terrain terrain, float[] noiseSamples)
    {
        terrain.Clear();
        terrain.SetVertices(GenerateVertices(terrain.GridPosition, terrain.GridSize, noiseSamples));
        terrain.SetColors(GetColors(terrain.Vertices, terrain.GridPosition, terrain.GridSize, noiseSamples, false));
        terrain.Refresh();
    }

    private Vector3[,] GenerateVertices(Vector2Int gridPosition, Vector2Int gridSize, float[] noiseSamples)
    {
        Vector3[,] vertices = new Vector3[gridSize.x, gridSize.y];

        Debug.Log($"noiseSamples count: {noiseSamples.Length}. VerticesX {vertices.GetLength(0)}, {vertices.GetLength(1)}"); 
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                
                //vertices[i, j] = new Vector3(i, Mathf.Lerp(minHeight, maxHeight, noiseSample), j);
                float heightCalc = MeshHeightCurve.Evaluate(noiseSamples[i * gridSize.y + j]);
                heightCalc -= ErosionCurve.Evaluate(noiseSamples[i * gridSize.y + j]);
                float vertexResult = heightCalc  * MeshHeightMultiplyer;
                vertices[i, j] = new Vector3(i, vertexResult, j);
            }
        }

        return vertices;
    }

    private List<Color> GetColors(Vector3[,] Vertices, Vector2Int gridPosition, Vector2Int gridSize, float[] noiseSamples, bool flat){
        List<Color> colors = new List<Color>();

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