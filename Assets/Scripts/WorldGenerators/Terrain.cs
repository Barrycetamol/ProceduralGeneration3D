using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class containing Terrain information
/// </summary>
public class Terrain{

    /// <summary>
    /// Our gameobject that appears in the scene tree
    /// </summary>
    public GameObject m_Terrain{get; set;}
    
    /// <summary>
    /// Flag that designates whether this terrain should use deltaTime
    /// </summary>
    public bool UseDeltaTime{get; set;}
    
    /// <summary>
    /// m_Terrain's MeshRenderer
    /// </summary>
    private MeshRenderer MeshRenderer{get;set;}

    /// <summary>
    /// m_Terrain's MeshFilter
    /// </summary>
    private MeshFilter MeshFilter { get; set; }

    /// <summary>
    /// The mesh to display
    /// </summary>
    private Mesh Mesh{get;set;}

    /// <summary>
    /// 2D array containing our vertices for our mesh. Used for calcualting neighbours
    /// </summary>
    public Vector3[,] Vertices {get; set;}

    /// <summary>
    /// The same as Vertices in List form. 
    /// </summary>
    public List<Vector3> VertexList{get; set;}

    /// <summary>
    /// The triangles of each quad
    /// </summary>
    public List<int> TriangleList { get; set;}

    /// <summary>
    /// The colours of our associated vertices.
    /// </summary>
    public List<Color> ColorList {get; set;}

    public Vector2Int GridPosition {get; set;}
    public Vector2Int GridSize {get; set;}
    private GerstnerWaves GerstnerWaves{get; set;} = new GerstnerWaves();
    private bool IsWater{get; set;}
    private ColorTextureRenderer ColorBand {get; set;}
    private float[] NoiseMapSamples{get; set;}
    private int MeshDetailLevel{get; set;}
    public float MinimumHeight { get; private set; }
    public float MaximumHeight { get; private set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">Name of our gameobject</param>
    /// <param name="width">Width of our mesh</param>
    /// <param name="height">Height of our mesh</param>
    /// <param name="useDeltaTime">Flag that designates whether this terrain should use deltaTime when calculating its mesh</param>
    public Terrain(String name, Vector2Int gridSize, Vector2Int gridPosition, bool useDeltaTime, bool isWater, ColorTextureRenderer colorBand, int meshDetailLevel){
        GridPosition = gridPosition;
        GridSize = gridSize;
        UseDeltaTime = useDeltaTime;
        IsWater = isWater;
        ColorBand = colorBand;
        MeshDetailLevel = meshDetailLevel;

        m_Terrain = new GameObject(name);
        m_Terrain.transform.position = new Vector3(GridPosition.x * (GridSize.x - 1), 0, GridPosition.y * (GridSize.y - 1));

        Mesh = new Mesh();
        MeshRenderer = m_Terrain.AddComponent<MeshRenderer>();
        MeshFilter= m_Terrain.AddComponent<MeshFilter>();
        Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        MeshRenderer.material = new Material(Shader.Find("Custom/VertexColorShader"));
        MeshRenderer.material.SetFloat("_Smoothness", 0.0f);
        MeshRenderer.material.SetFloat("_Metallic", 0.0f);

        Vertices = new Vector3[GridSize.x, GridSize.y];
        TriangleList = new List<int>();
        ColorList = new List<Color>();
        VertexList = new List<Vector3>();

    }

    /// <summary>
    /// Refreshes our Mesh instance. Must be after mesh updates.
    /// </summary>
    public void Refresh(){
        Mesh.vertices = VertexList.ToArray();
        Mesh.colors = ColorList.ToArray();
        Mesh.triangles = TriangleList.ToArray();
        Mesh.RecalculateNormals();
        Mesh.RecalculateBounds();

        MeshFilter.mesh = Mesh;

        if(IsWater){
            m_Terrain.AddComponent<GerstnerWaves>();
            GerstnerWaves waves;
            m_Terrain.TryGetComponent<GerstnerWaves>(out waves);
            if(waves){
                waves.AddColorBand(ColorBand);
            }
        }
    }

    /// <summary>
    /// Clears our mesh data. Not including the mesh.
    /// </summary>
    public void Clear()
    {
        VertexList.Clear();
        ColorList.Clear();
        TriangleList.Clear();
    }

    /// <summary>
    /// Clears our current Mesh and associated data.
    /// </summary>
    public void ClearMesh(){
        Mesh = new Mesh();
        Clear();
    }

    public void SetVertices(Vector3[,] vertices){
        Vertices = vertices;
        for(int i = 0; i < Vertices.GetLength(1); i++){
            for(int j = 0; j < Vertices.GetLength(0); j++){
                VertexList.Add(Vertices[i, j]);
            }
        }

        CreateTriangles();
        ColorList = GetColors(Vertices, GridSize, NoiseMapSamples, ColorBand, false);
    }


    private void CreateTriangles(){
        int height = Vertices.GetLength(0); 
        int width = Vertices.GetLength(1);

        for(int i = 0; i < width - 1; i++){
            for(int j = 0; j < height - 1; j++){
                int vertexIndex = i * height + j;
                int bottomLeft = vertexIndex;
                int bottomRight = vertexIndex + height;
                int topLeft = vertexIndex + 1;
                int topRight = bottomRight + 1;

                // Add triangles for the two triangles of the quad
                TriangleList.Add(bottomLeft);
                TriangleList.Add(topLeft);
                TriangleList.Add(bottomRight);

                TriangleList.Add(topLeft);
                TriangleList.Add(topRight);
                TriangleList.Add(bottomRight);
            }
        }
    }

    private List<Color> GetColors(Vector3[,] Vertices, Vector2Int gridSize, float[] samples, ColorTextureRenderer colorBands, bool flat)
    {
        List<Color> colors = new List<Color>();
        Debug.Log($"Vertices lengths: {Vertices.GetLength(1)}, {Vertices.GetLength(0)}  {Vertices.GetLength(0) * Vertices.GetLength(1)},  noiseSample length: {samples.Length}");
        Debug.Log($"Minimum {MinimumHeight} -- maximum {MaximumHeight}");
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

            }
        }
        Debug.Log($"Color size: {colors.Count}, {Vertices.Length}");
        return colors;
    }

    public void SetNoiseMap(float[] samples)
    {
        NoiseMapSamples = samples;
    }

    internal void SetMinMaxHeights(float minSample, float maxSample)
    {
        MinimumHeight = minSample;
        MaximumHeight = maxSample;
    }
}