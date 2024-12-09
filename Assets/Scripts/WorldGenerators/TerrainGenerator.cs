using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class TerrainGenerator : MonoBehaviour
{
    [field: Header("Base generation settings")]
    [field: SerializeField] public Vector2Int GridSize { get; set; }
    [field: SerializeField] public Vector2Int GridResolution { get; set; }
    [field: SerializeField] public int MeshDetailLevel {get; set;}
    [field: SerializeField] public float MaximumHeight;
    [field: SerializeField] public float MinimumHeight;
    [field: SerializeField] public float MeshHeightMultiplyer;
    [field: SerializeField] public bool AutoUpdate;
    [field: SerializeField] public float SeaLevel { get; private set; }

    [field: Header("Noise maps")]
    [field: SerializeField] public TerrainModifier HeightModifier;
    [field: SerializeField] public TerrainModifier ErosionModifier;
    [field: SerializeField] public TerrainModifier PeaksAndValleysModifier;
    [field: SerializeField] public ColorTextureRenderer WaterColourBands {get; set;}
    [field: SerializeField] public ColorTextureRenderer LandColourBands { get; set; }
    [field: SerializeField] public ColorTextureRenderer CloudColourBands { get; set; }


    [field: Header("Visualisation of noisemaps")]
    [field: SerializeField] public NoiseTextureRender HeightNoiseTexture { get; set; }
    [field: SerializeField] public NoiseTextureRender ErosionNoiseTexture { get; set; }
    [field: SerializeField] public NoiseTextureRender PeaksAndValleysNoiseTexture { get; set; }
    [field: SerializeField] public NoiseTextureRender CombinedNoiseTexture { get; set; }
    [field: SerializeField] public Vector2 CombinedMapNormalizationOffsets { get; set; }

    private List<Terrain> Terrains { get; set; } = new List<Terrain>();
    private List<Terrain> WaterTerrains { get; set; } = new List<Terrain>();
    private float CalculatedSeaLevel = 0;


    void Start()
    {
        if (AutoUpdate)
        {
            InvokeRepeating("StartGenerating", 1.0f, 4.0f);
        }
        else StartGenerating();
    }

    void Update(){
        if(WaterTerrains.Count > 0){

        }
    }

    public void StartGenerating()
    {
        // Clamp for min size.
        GridResolution = new Vector2Int(Math.Clamp(GridResolution.x, 1, 4096), Math.Clamp(GridResolution.y, 1, 4096));
        GridSize = new Vector2Int(Math.Max(1, GridSize.x), Math.Max(1, GridSize.y));  // Ensure 1x1 square

        foreach (Terrain terrain in Terrains)
        {
            Debug.Log($"Clearing terrain {terrain.m_Terrain.name}");
            Destroy(terrain.m_Terrain);
        }

        foreach (Terrain terrain in WaterTerrains)
        {
            Debug.Log($"Clearing terrain {terrain.m_Terrain.name}");
            Destroy(terrain.m_Terrain);
        }

        Terrains.Clear();
        WaterTerrains.Clear();

        for (int i = 0; i < GridSize.x; i++)
        {
            for (int j = 0; j < GridSize.y; j++)
            {
                Terrains.Add(new Terrain($"Land_{i}x{j}", GridResolution, new Vector2Int(i, j), false, false));
            }
        }
        GenerateWorld();
    }


    private void GenerateWorld()
    {
        LandColourBands.SortColorBands();
        WaterColourBands.SortColorBands();
        CalculatedSeaLevel = SeaLevel * MeshHeightMultiplyer / 2;

        // Create noise maps for each terrain segment
        Dictionary<string, NoiseMapInfo> landTerrainMaps = new Dictionary<string, NoiseMapInfo>();
        Dictionary<string, float[]> waterTerrainMaps = new Dictionary<string, float[]>();
        float minimum = float.MinValue;
        float maximum = float.MaxValue;
        foreach (Terrain terrain in Terrains)
        {
            var heightsample = new SampleInfo(HeightModifier.NoiseGenerator.GetNoiseSamples(terrain.GridPosition, terrain.GridSize, false),
                                                HeightModifier.NoiseGenerator.minimumNoiseSample,
                                                HeightModifier.NoiseGenerator.maximumNoiseSample
            );
            var erosionSample = new SampleInfo(ErosionModifier.NoiseGenerator.GetNoiseSamples(terrain.GridPosition, terrain.GridSize, false),
                                                ErosionModifier.NoiseGenerator.minimumNoiseSample,
                                                ErosionModifier.NoiseGenerator.maximumNoiseSample
            );
            var pvSample = new SampleInfo(PeaksAndValleysModifier.NoiseGenerator.GetNoiseSamples(terrain.GridPosition, terrain.GridSize, false),
                                                PeaksAndValleysModifier.NoiseGenerator.minimumNoiseSample,
                                                PeaksAndValleysModifier.NoiseGenerator.maximumNoiseSample
            );
            landTerrainMaps[terrain.m_Terrain.name] = new NoiseMapInfo(heightsample, erosionSample, pvSample);

            // get min/max values from our generated samples
            if (HeightModifier.NoiseGenerator.minimumNoiseSample < minimum) minimum = HeightModifier.NoiseGenerator.minimumNoiseSample;
            if (HeightModifier.NoiseGenerator.maximumNoiseSample > maximum) maximum = HeightModifier.NoiseGenerator.maximumNoiseSample;
        }

        // sanity check to make sure generation hasn't failed.
        CheckNoiseMapsAreDifferent(landTerrainMaps, Terrains);

        // Normalize heights based on our min/maxes of each grid
        foreach (Terrain terrain in Terrains)
        {
            var noiseMapInfo = landTerrainMaps[terrain.m_Terrain.name];
            noiseMapInfo.heightSamples = new SampleInfo(
                    HeightModifier.NoiseGenerator.NormalizeSamples(
                        landTerrainMaps[terrain.m_Terrain.name].heightSamples.noiseMap, minimum, maximum),
                        minimum, maximum
            );
            noiseMapInfo.erosionSamples = new SampleInfo(
                    ErosionModifier.NoiseGenerator.NormalizeSamples(
                        landTerrainMaps[terrain.m_Terrain.name].erosionSamples.noiseMap, minimum, maximum),
                        minimum, maximum
            );
            noiseMapInfo.PVsamples = new SampleInfo(
                    PeaksAndValleysModifier.NoiseGenerator.NormalizeSamples(
                        landTerrainMaps[terrain.m_Terrain.name].PVsamples.noiseMap, minimum, maximum),
                        minimum, maximum
            );
            float[] combinedValues = CombineMaps(GridResolution, noiseMapInfo);
            noiseMapInfo.CombinedValues = new SampleInfo(NormalizeSamples(combinedValues), minimum, maximum);

            landTerrainMaps[terrain.m_Terrain.name] = noiseMapInfo;

            // Create a water terrain and store current combined noisemap
            Terrain waterTerrain = new Terrain($"Water_{terrain.GridPosition.x}x{terrain.GridPosition.y}", GridResolution, new Vector2Int(terrain.GridPosition.x, terrain.GridPosition.y), false, true);
            waterTerrainMaps[waterTerrain.m_Terrain.name] = noiseMapInfo.CombinedValues.noiseMap;
            WaterTerrains.Add(waterTerrain);
        }

        // sanity check to make sure normalization hasn't failed.
        CheckNoiseMapsAreDifferent(landTerrainMaps, Terrains);

        // generate the terrain
        foreach (Terrain terrain in Terrains)
        {
            Debug.Log($"Staring terrain generation for: {terrain.m_Terrain.name}");
            GenerateTerrain(terrain, landTerrainMaps[terrain.m_Terrain.name]);
        }

        WriteDebugTextures(landTerrainMaps["Land_0x0"]);

        // From terrain, add water
        //generate water

        foreach (Terrain waterTerrain in WaterTerrains)
        {
            GenerateWater(waterTerrain, waterTerrainMaps[waterTerrain.m_Terrain.name]);
        }

    }

    private void GenerateWater(Terrain terrain, float[] vertices)
    {
        terrain.Clear();
        terrain.SetVertices(GenerateWaterVertices(terrain.GridPosition, terrain.GridSize, vertices));
        terrain.SetColors(GetColors(terrain.Vertices, terrain.GridSize, vertices, WaterColourBands, false));
        terrain.Refresh();
    }

    private Vector3[,] GenerateWaterVertices(Vector2Int gridPosition, Vector2Int gridSize, float[] noiseMap)
    {
        Vector3[,] vertices = new Vector3[gridSize.x * MeshDetailLevel, gridSize.y * MeshDetailLevel];

        for (int i = 0; i < gridSize.x * MeshDetailLevel; i++)
        {
            for (int j = 0; j < gridSize.y * MeshDetailLevel; j++)
            {
                vertices[i, j] = new Vector3(i / MeshDetailLevel, CalculatedSeaLevel, j / MeshDetailLevel);
            }
        }

        return vertices;
    }

    private void GenerateTerrain(Terrain terrain, NoiseMapInfo samples)
    {
        terrain.Clear();
        terrain.SetVertices(GenerateLandVertices(terrain.GridPosition, terrain.GridSize, samples));
        terrain.SetColors(GetColors(terrain.Vertices, terrain.GridSize, samples.CombinedValues.noiseMap, LandColourBands, false));
        terrain.Refresh();
    }

    private float[] StripLandVertices(float[] landVertices)
    {
        List<float> processedNoise = new List<float>(landVertices.Length);
        foreach (float landVertex in landVertices)
        {
            if (landVertex < CalculatedSeaLevel) processedNoise.Add(landVertex);
        }

        return processedNoise.ToArray();
    }

    private float[] NormalizeSamples(float[] combinedValues)
    {
        float[] newValues = new float[combinedValues.Length];
        for (int i = 0; i < combinedValues.GetLength(0); i++)
        {
            newValues[i] = (combinedValues[i]) / (CombinedMapNormalizationOffsets.x * CombinedMapNormalizationOffsets.y);
        }

        return newValues;
    }

    private void WriteDebugTextures(NoiseMapInfo noiseMapInfo)
    {
        HeightNoiseTexture.WritePixelsToRenderTarget(noiseMapInfo.heightSamples.noiseMap, GridResolution);
        ErosionNoiseTexture.WritePixelsToRenderTarget(noiseMapInfo.erosionSamples.noiseMap, GridResolution);
        PeaksAndValleysNoiseTexture.WritePixelsToRenderTarget(noiseMapInfo.PVsamples.noiseMap, GridResolution);
        CombinedNoiseTexture.WritePixelsToRenderTarget(noiseMapInfo.CombinedValues.noiseMap, GridResolution);
    }

    private bool CheckNoiseMapsAreDifferent(Dictionary<string, NoiseMapInfo> noiseMaps, List<Terrain> terrains)
    {
        bool isDifferent = true;
        float[] prev = new float[0];
        string prev_name = "";
        foreach (Terrain terrain in terrains)
        {
            var a = noiseMaps[terrain.m_Terrain.name].heightSamples.noiseMap;
            if (prev.Length >= 0)
            {
                if (prev.Length == a.Length && prev.SequenceEqual(a))
                {
                    Debug.LogError($"Terrains were equal after normalization: {prev_name} {terrain.m_Terrain.name}");
                    isDifferent = false;
                }
                else
                {
                    prev = a;
                    prev_name = terrain.m_Terrain.name;
                }
            }
            else
            {
                // Set first terrain
                prev = noiseMaps[terrain.m_Terrain.name].heightSamples.noiseMap;
                prev_name = terrain.m_Terrain.name;
            }
        }
        return isDifferent;
    }

    private float[] CombineMaps(Vector2Int gridSize, NoiseMapInfo samples)
    {
        float[] combinedMap = new float[gridSize.x * gridSize.y];

        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                combinedMap[i * gridSize.y + j] = PerformHeightCalculation(samples.heightSamples.noiseMap[i * gridSize.y + j],
                                                                            samples.erosionSamples.noiseMap[i * gridSize.y + j],
                                                                            samples.PVsamples.noiseMap[i * gridSize.y + j]);
            }
        }
        return combinedMap;
    }



    private Vector3[,] GenerateLandVertices(Vector2Int gridPosition, Vector2Int gridSize, NoiseMapInfo samples)
    {
        Vector3[,] vertices = new Vector3[gridSize.x * MeshDetailLevel, gridSize.y * MeshDetailLevel];

        for (int i = 0; i < gridSize.x * MeshDetailLevel; i++)
        {
            for (int j = 0; j < gridSize.y * MeshDetailLevel; j++)
            {
                int i_offset = i / MeshDetailLevel;
                int j_offset = j / MeshDetailLevel;
                vertices[i, j] = new Vector3((float)i / MeshDetailLevel, samples.CombinedValues.noiseMap[i_offset * gridSize.y + j_offset] * MeshHeightMultiplyer, (float)j / MeshDetailLevel);
            }
        }

        return vertices;
    }

    private float PerformHeightCalculation(float height, float erosion, float pv)
    {
        return Mathf.Clamp01(HeightModifier.GetNoiseFromCurve(height) +
               Mathf.Clamp01(ErosionModifier.GetNoiseFromCurve(erosion)) +
               Mathf.Clamp01(PeaksAndValleysModifier.GetNoiseFromCurve(pv)));
    }

    private List<Color> GetColors(Vector3[,] Vertices, Vector2Int gridSize, float[] samples, ColorTextureRenderer colorBands, bool flat)
    {
        List<Color> colors = new List<Color>();
        Debug.Log($"Vertices lengths: {Vertices.GetLength(1)}, {Vertices.GetLength(0)}  {Vertices.GetLength(0) * Vertices.GetLength(1)},  noiseSample length: {samples.Length}");

        for (int i = 0; i < Vertices.GetLength(1); i++)
        {
            for (int j = 0; j < Vertices.GetLength(0); j++)
            {
                // fractional differences between current vertex and grid resolution
                float fractionalDistanceX = (i % MeshDetailLevel) / (float)MeshDetailLevel;
                float fractionalDistanceY = (j % MeshDetailLevel) / (float)MeshDetailLevel;

                // current noisemap element
                int noiseMapX = i / MeshDetailLevel;
                int noiseMapY = j / MeshDetailLevel;
                // the next noiseMap element
                int noiseMapX_Next = Mathf.Min(noiseMapX + 1, gridSize.x - 1);
                int noiseMapY_Next = Mathf.Min(noiseMapY + 1, gridSize.y - 1);

                // Get noise samples from noisemap
                float noiseSampleBottomLeft = samples[noiseMapX * gridSize.y + noiseMapY];
                float noiseSampleTopLeft = samples[noiseMapX_Next * gridSize.y + noiseMapY];
                float noiseSampleBottomRight = samples[noiseMapX * gridSize.y + noiseMapY_Next];
                float noiseSampleTopRight = samples[noiseMapX_Next * gridSize.y + noiseMapY_Next];

                // interpolate between the points
                float noiseSampleBottom = Mathf.Lerp(noiseSampleBottomLeft, noiseSampleBottomRight, fractionalDistanceX);
                float noiseSampleTop = Mathf.Lerp(noiseSampleTopLeft, noiseSampleTopRight, fractionalDistanceX);
                float noiseSample = Mathf.Lerp(noiseSampleBottom, noiseSampleTop, fractionalDistanceY);

                colors.Add(colorBands.GenerateColor(Mathf.InverseLerp(MinimumHeight, MaximumHeight, noiseSample), flat));

                // Color bottomLeftColor = colorBands.GenerateColor(Mathf.InverseLerp(MinimumHeight, MaximumHeight, noiseSampleBottomLeft), flat);
                // Color topLeftColor = colorBands.GenerateColor(Mathf.InverseLerp(MinimumHeight, MaximumHeight,  noiseSampleTopLeft), flat);
                // Color bottomRightColor = colorBands.GenerateColor(Mathf.InverseLerp(MinimumHeight, MaximumHeight,  noiseSampleBottomRight), flat);
                // Color topRightColor = colorBands.GenerateColor(Mathf.InverseLerp(MinimumHeight, MaximumHeight,  noiseSampleTopRight), flat);


                // colors.Add(bottomLeftColor);
                // colors.Add(topLeftColor);
                // colors.Add(bottomRightColor);
                // colors.Add(topRightColor);


            }
        }
        Debug.Log($"Color size: {colors.Count}, {Vertices.Length}");
        return colors;
    }
}