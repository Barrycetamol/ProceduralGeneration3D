using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldGenerator : MonoBehaviour
{
    [field: Header("Base generation settings")]
    [field: SerializeField] public Vector2Int GridSize { get; set; }
    [field: SerializeField] public Vector2Int GridResolution { get; set; }
    [field: SerializeField] public int MeshDetailLevel {get; set;}
    [field: SerializeField] public float MeshHeightMultiplyer;
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
    [field: SerializeField] public NoiseTextureRender CloudNoiseTexture { get; set; }
    [field: SerializeField] public NoiseTextureRender WindNoiseTexture { get; set; }
    [field: SerializeField] public Vector2 CombinedMapNormalizationOffsets { get; set; }

    [field: Header("Player ")]
    [field: SerializeField] public GameObject playerPrefab {get; set;}
    [field: SerializeField] public Camera CameraToTrackPlayerWith {get; set;}
    private GameObject player;

    [field: Header("World objects")]
    [field: SerializeField] public GameObject ObjectContainer {get; set;}
    [field: SerializeField] public GameObject LargeTree {get; set;}
    [field: SerializeField] public GameObject SmallTree {get; set;}
    private List<Vector3> Points {get; set;}


    

    private List<Terrain> Terrains { get; set; } = new List<Terrain>();
    private List<Terrain> WaterTerrains { get; set; } = new List<Terrain>();
    private float CalculatedSeaLevel = 0;
    private float[,] FalloffMap;

    public void SetNoiseValues(NoiseSettings height, NoiseSettings erosion, NoiseSettings PV){
        GameObject heightGen;
        GameObject erosionGen;
        GameObject PVGen;
        if(height.noiseType == NoiseType.PERLIN) heightGen = GameObject.FindGameObjectWithTag("PerlinHeight");
        else heightGen = GameObject.FindGameObjectWithTag("SimplexHeight");

        if(erosion.noiseType == NoiseType.PERLIN) erosionGen = GameObject.FindGameObjectWithTag("PerlinErosion");
        else erosionGen = GameObject.FindGameObjectWithTag("SimplexErosion");

        if(PV.noiseType == NoiseType.PERLIN) PVGen = GameObject.FindGameObjectWithTag("PerlinPV");
        else PVGen = GameObject.FindGameObjectWithTag("SimplexPV");

        HeightModifier = heightGen.GetComponent<TerrainModifier>();
        heightGen.GetComponent<NoiseGenerator>().SetSettings(height);

        ErosionModifier = erosionGen.GetComponent<TerrainModifier>();
        erosionGen.GetComponent<NoiseGenerator>().SetSettings(erosion);

        PeaksAndValleysModifier = PVGen.GetComponent<TerrainModifier>();
        PVGen.GetComponent<NoiseGenerator>().SetSettings(PV);
    }

    public void StartGenerating()
    {
        FalloffMap = FalloffGenerator.GenerateFalloffMap(GridResolution.x);
        // Clamp for min size.
        GridResolution = new Vector2Int(Math.Clamp(GridResolution.x, 1, 4096), Math.Clamp(GridResolution.y, 1, 4096));
        GridSize = new Vector2Int(Math.Max(1, GridSize.x), Math.Max(1, GridSize.y));  // Ensure 1x1 square

        // Clear any terrains
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
        // Clear world objects
        for(int i = 0; i < ObjectContainer.transform.childCount; i++){
            Destroy(ObjectContainer.transform.GetChild(i).gameObject);
        }
        

        for (int i = 0; i < GridSize.x; i++)
        {
            for (int j = 0; j < GridSize.y; j++)
            {
                Terrains.Add(new Terrain($"Land_{i}x{j}", GridResolution, new Vector2Int(i, j), false, false, LandColourBands, MeshDetailLevel));
            }
        }
        GenerateWorld();
    }

    /// <summary>
    /// Places player in the world
    /// </summary>
    public void PlacePlayer()
    {
        RemovePlayer();
        player = Instantiate(playerPrefab);
        player.transform.position = new Vector3(20, 0, 20);
        player.GetComponent<BoatController>().StartingHeight = CalculatedSeaLevel;
        var a = CameraToTrackPlayerWith.GetComponent<CameraController>();
        a.boat = player.transform;
    }

    /// <summary>
    /// Removes player if it exists
    /// </summary>
    public void RemovePlayer(){
        if(player != null) Destroy(player);
    }


    /// <summary>
    /// The main world generation function
    /// </summary>
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
                                            HeightModifier.NoiseGenerator.NormalizeSamples(noiseMapInfo.heightSamples.noiseMap, 
                                                                                           minimum, maximum),
                                            minimum, maximum
            );
            noiseMapInfo.erosionSamples = new SampleInfo(
                                            ErosionModifier.NoiseGenerator.NormalizeSamples(noiseMapInfo.erosionSamples.noiseMap, 
                                                                                            minimum, maximum),
                                            minimum, maximum
            );
            noiseMapInfo.PVsamples = new SampleInfo(
                                        PeaksAndValleysModifier.NoiseGenerator.NormalizeSamples(noiseMapInfo.PVsamples.noiseMap, 
                                                                                                minimum, maximum),
                                        minimum, maximum
            );
            float[] combinedValues = CombineMaps(GridResolution, noiseMapInfo);
            noiseMapInfo.CombinedValues = new SampleInfo(NormalizeSamples(combinedValues), combinedValues.Min(), combinedValues.Max());

            landTerrainMaps[terrain.m_Terrain.name] = noiseMapInfo;

            // Create a water terrain and store current combined noisemap
            Terrain waterTerrain = new Terrain($"Water_{terrain.GridPosition.x}x{terrain.GridPosition.y}", GridResolution, new Vector2Int(terrain.GridPosition.x, terrain.GridPosition.y), false, true, WaterColourBands, MeshDetailLevel);
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
        Shader.SetGlobalFloat("_StartTime", Time.time);  // to sync the wave shaders start time.
        foreach (Terrain waterTerrain in WaterTerrains)
        {
            GenerateWater(waterTerrain, waterTerrainMaps[waterTerrain.m_Terrain.name]);
        }

    }

    /// <summary>
    /// generates water terrains
    /// </summary>
    /// <param name="terrain"></param>
    /// <param name="vertices"></param>
    private void GenerateWater(Terrain terrain, float[] vertices)
    {
        terrain.Clear();
        terrain.SetNoiseMap(vertices);
        terrain.SetVertices(GenerateWaterVertices(terrain.GridSize));
        terrain.Refresh();
    }

    /// <summary>
    /// Generates the water vertices at the stored sealevel
    /// </summary>
    /// <param name="gridSize">grid size resolution</param>
    /// <returns>water vertices</returns>
    private Vector3[,] GenerateWaterVertices(Vector2Int gridSize)
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

    /// <summary>
    /// Generates terrain given the noise samples
    /// </summary>
    /// <param name="terrain">current terrain to generate vertices for</param>
    /// <param name="samples">noise info</param>
    private void GenerateTerrain(Terrain terrain, NoiseMapInfo samples)
    {
        terrain.Clear();
        terrain.SetMinMaxHeights(0, 1);
        terrain.SetNoiseMap(samples.CombinedValues.noiseMap);
        terrain.SetVertices(GenerateLandVertices(terrain.GridPosition, terrain.GridSize, samples));
        terrain.Refresh();

        AddObjects(samples);
    }

    /// <summary>
    /// Add objects to map using Poisson disc sampling
    /// </summary>
    /// <param name="samples">noise map samples</param>
    private void AddObjects(NoiseMapInfo samples)
    {
        Points = PoissonDiscSampling.GeneratePoints(50, GridResolution, 20, new Vector2(0.3f, 0.7f), samples.CombinedValues.noiseMap, MeshHeightMultiplyer);
        if(ObjectContainer) {
            foreach (Vector3 point in Points)
            {
                Vector3 position = new Vector3(point.x, point.y, point.z);
                var rand = UnityEngine.Random.Range(0,2);
                GameObject spawnedObject;
                if(rand == 0) spawnedObject = Instantiate(LargeTree, position, Quaternion.identity);
                else spawnedObject = Instantiate(SmallTree, position, Quaternion.identity);

                spawnedObject.transform.parent = ObjectContainer.transform;
            }
        }

    }

    private void OnDrawGizmos(){
        if(Points != null){
            if(Points.Count > 0){
                PoissonDiscSampling.DrawGizmos(Points, 0.5f);
        }
        }
       

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

    /// <summary>
    /// Writes to a debug texture (NoiseTextureRenderers)
    /// </summary>
    /// <param name="noiseMapInfo">noise map sample info</param>
    private void WriteDebugTextures(NoiseMapInfo noiseMapInfo)
    {
        HeightNoiseTexture.WritePixelsToRenderTarget(noiseMapInfo.heightSamples.noiseMap, GridResolution);
        ErosionNoiseTexture.WritePixelsToRenderTarget(noiseMapInfo.erosionSamples.noiseMap, GridResolution);
        PeaksAndValleysNoiseTexture.WritePixelsToRenderTarget(noiseMapInfo.PVsamples.noiseMap, GridResolution);
        CombinedNoiseTexture.WritePixelsToRenderTarget(noiseMapInfo.CombinedValues.noiseMap, GridResolution);
    }

    /// <summary>
    /// Checks if the generated noise maps are different from one another, helper function to test generations are different per grid piece
    /// </summary>
    /// <param name="noiseMaps">Dictionary of terrain name and its noisemap</param>
    /// <param name="terrains">list of terrains to check against</param>
    /// <returns></returns>
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

    /// <summary>
    /// Combines maps together using the PerformHeightCalculation
    /// </summary>
    /// <param name="gridSize">grid piece resolution</param>
    /// <param name="samples">noise map samples to combine together</param>
    /// <returns>the combined noise map</returns>
    private float[] CombineMaps(Vector2Int gridSize, NoiseMapInfo samples)
    {
        float[] combinedMap = new float[gridSize.x * gridSize.y];

        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                var heightCalc = PerformHeightCalculation(samples.heightSamples.noiseMap[i * gridSize.y + j],
                                                          samples.erosionSamples.noiseMap[i * gridSize.y + j],
                                                          samples.PVsamples.noiseMap[i * gridSize.y + j]);
                combinedMap[i * gridSize.y + j] = heightCalc - FalloffMap[i, j];
            }
        }
        return combinedMap;
    }


    /// <summary>
    /// Generates vertices based on the gridSize, samples and meshdetaillevel
    /// </summary>
    /// <param name="gridPosition">(UNUSED NEEDS REFACTOR)</param>
    /// <param name="gridSize">grid piece resolution </param>
    /// <param name="samples">noise map samples</param>
    /// <returns>vertices as a multidemensional array</returns>
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

    /// <summary>
    /// Noisesample combination calculation. Adds together and clamps between 0 and 1;
    /// </summary>
    /// <param name="height">any noise sample</param>
    /// <param name="erosion">any noise sample</param>
    /// <param name="pv">any noise sample</param>
    /// <returns>combined sample</returns>
    private float PerformHeightCalculation(float height, float erosion, float pv)
    {
        return Mathf.Clamp01(HeightModifier.GetNoiseFromCurve(height) +
               Mathf.Clamp01(ErosionModifier.GetNoiseFromCurve(erosion)) +
               Mathf.Clamp01(PeaksAndValleysModifier.GetNoiseFromCurve(pv)));
    }

   
}