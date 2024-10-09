using System;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    private Mesh mesh;

    [field: SerializeField]
    Material material;

    [field: SerializeField]
    public int X{get; set;}

    [field: SerializeField]    
    public int Y{get; set;}

    [field: SerializeField]    
    public float MaxHeight{get; set;}
    [field: SerializeField]    
    public float MinHeight{get; set;}

    [field: SerializeField]
    public float PerlinScale {get; set;}

    [field: SerializeField]
    public float PerlinSeed {get; set;}
    [field: SerializeField]
    public float TimeOffset = 0;
    [field: SerializeField]
    public float WaterSpeed = 1;
    [field: SerializeField]    
    public float WaterMaxHeight{get; set;}
    [field: SerializeField]    
    public float WaterMinHeight{get; set;}

    [field: SerializeField]
    public float SmoothFactor {get; set;}
    [field: SerializeField]
    public int SmoothingIteration{get; set;}

    private Vector3[,] waterVertices;
    private Mesh waterMesh;
    private Mesh landMesh;
    private Vector3[,] landVertices;

    private GameObject water;
    private GameObject land;

    private float deltaTime;

    
    void Start()
    {
        //GenerateLandscapeAsTiles();

        water = new GameObject($"Water");
        land = new GameObject($"Land");

        InvokeRepeating("GenerateLandscapeAsSingleMesh", 1.0f, 1.0f);
        
    }

    private void GenerateLandscapeAsSingleMesh()
    {
        landVertices = GenerateVerticesWithTime(MinHeight, MaxHeight, 0);
        landVertices = SmoothVertexGrid(landVertices);
        landMesh = new Mesh();

        MeshFilter landMeshFilter;
        MeshRenderer landMeshRenderer;
        if(!land.TryGetComponent<MeshFilter>(out landMeshFilter)) landMeshFilter = land.AddComponent<MeshFilter>();
        if(!land.TryGetComponent<MeshRenderer>(out landMeshRenderer)) landMeshRenderer = land.AddComponent<MeshRenderer>();

        var vertexList = new List<Vector3>();
        var triangleList = new List<int>();
        var colorList = new List<Color>();
       
        for(int i = 0; i < X; i++){
            for(int j = 0; j < Y; j++){
                Vector3 bottomLeft = landVertices[i, j];
                Vector3 topLeft = landVertices[i, j + 1];
                Vector3 bottomRight = landVertices[i + 1, j];
                Vector3 topRight = landVertices[i + 1, j + 1];

                // Add vertices to the vertex list
                int vertexIndex = vertexList.Count;
                vertexList.Add(bottomLeft);
                vertexList.Add(topLeft);
                vertexList.Add(bottomRight);
                vertexList.Add(topRight);

                // Add triangles for the two triangles of the quad
                triangleList.Add(vertexIndex);
                triangleList.Add(vertexIndex + 1);
                triangleList.Add(vertexIndex + 2);
                triangleList.Add(vertexIndex + 1);
                triangleList.Add(vertexIndex + 3);
                triangleList.Add(vertexIndex + 2);

                float perlinNoise = Mathf.PerlinNoise((float)((i + PerlinSeed) * PerlinScale ), (float) ((j + PerlinSeed)* PerlinScale));

                Color black = new Color(0.1f, 0.1f, 0.1f);
                Color white = new Color(1.0f, 1.0f, 1.0f);
                Color brown = new Color(0.24f, 0.132f, 0.173f);
                Color green = new Color(0.146f, 0.610f, 0.239f);
                Color baseColor = black;
                Color endColor = white;
                if(perlinNoise <= 0.2f) endColor = brown;
                else if(perlinNoise <= 0.8f) {
                    endColor = green;
                    baseColor = brown;
                }
                else{
                    endColor = white;
                    baseColor = green;
                }

                Color bottomLeftColor = Color.Lerp(baseColor, endColor, bottomLeft.y);
                Color bottomRightColor = Color.Lerp(baseColor, endColor, bottomRight.y);
                Color topRightColor = Color.Lerp(baseColor, endColor, topRight.y);
                Color topLeftColor = Color.Lerp(baseColor, endColor, topLeft.y);

                colorList.Add(bottomLeftColor);
                colorList.Add(topLeftColor);
                colorList.Add(bottomRightColor);
                colorList.Add(topRightColor);
            }
        }

        landMesh = new Mesh{
            vertices = vertexList.ToArray(),
            triangles = triangleList.ToArray(),
            colors = colorList.ToArray()
        };
        landMesh.RecalculateNormals();
        landMeshFilter.mesh = landMesh;
        landMeshRenderer.material = new Material(Shader.Find("Custom/VertexColorShader"));
        landMeshRenderer.material.SetFloat("_Smoothness", 1.0f);
        landMeshRenderer.material.SetFloat("_Metallic", 0.0f);
        //landMeshRenderer.material.color = colorList.ElementAt(0);
        //landMeshRenderer.material.mainTexture = GeneratePerlinTexture(256, 256);   //applying to whole mesh, need to change to work with just the vertexes

    }
    private void GenerateWater()
    {
        waterVertices = GenerateVerticesWithTime(WaterMinHeight, WaterMaxHeight, deltaTime);
        waterVertices = SmoothVertexGrid(waterVertices);
        waterMesh = new Mesh();

        MeshFilter waterMeshFilter;
        MeshRenderer waterMeshRenderer;
        if(!water.TryGetComponent<MeshFilter>(out waterMeshFilter)) waterMeshFilter = water.AddComponent<MeshFilter>();
        if(!water.TryGetComponent<MeshRenderer>(out waterMeshRenderer)) waterMeshRenderer = water.AddComponent<MeshRenderer>();

        var vertexList = new List<Vector3>();
        var triangleList = new List<int>();
        var colorList = new List<Color>();
       
        for(int i = 0; i < X; i++){
            for(int j = 0; j < Y; j++){
                Vector3 bottomLeft = waterVertices[i, j];
                Vector3 topLeft = waterVertices[i, j + 1];
                Vector3 bottomRight = waterVertices[i + 1, j];
                Vector3 topRight = waterVertices[i + 1, j + 1];

                // Add vertices to the vertex list
                int vertexIndex = vertexList.Count;
                vertexList.Add(bottomLeft);
                vertexList.Add(topLeft);
                vertexList.Add(bottomRight);
                vertexList.Add(topRight);

                // Add triangles for the two triangles of the quad
                triangleList.Add(vertexIndex);
                triangleList.Add(vertexIndex + 1);
                triangleList.Add(vertexIndex + 2);
                triangleList.Add(vertexIndex + 1);
                triangleList.Add(vertexIndex + 3);
                triangleList.Add(vertexIndex + 2);

                float perlinNoise = Mathf.PerlinNoise((i + PerlinSeed) * PerlinScale + deltaTime, (j + PerlinSeed) * PerlinScale + deltaTime);
                float colorValue = Mathf.Lerp(0.2f, 1.0f, perlinNoise);
                Color color = new Color(0, 0, colorValue); // blue level
                colorList.Add(color);
                colorList.Add(color);
                colorList.Add(color);
                colorList.Add(color);
            }
        }

        waterMesh = new Mesh{
            vertices = vertexList.ToArray(),
            triangles = triangleList.ToArray(),
            colors = colorList.ToArray()
        };
        waterMesh.RecalculateNormals();
        waterMeshFilter.mesh = waterMesh;
        waterMeshRenderer.material = new Material(Shader.Find("Standard"));
        waterMeshRenderer.material.SetFloat("_Smoothness", 1.0f);
        waterMeshRenderer.material.SetFloat("_Metallic", 0.0f);
        waterMeshRenderer.material.color = colorList.ElementAt(0);

    }

    // Chatgpt example 2d texture genertion using perlinnoise
     Texture2D GeneratePerlinTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        // Generate Perlin noise for each pixel
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)((x) + PerlinSeed) * PerlinScale ;
                float yCoord = (float)((y) + PerlinSeed)* PerlinScale;

                // Get the Perlin noise value
                float perlinValue = Mathf.PerlinNoise(xCoord, yCoord);

                // Create a color based on the Perlin noise value (green)
                Color color;
                if (perlinValue < 0.3f)
                    color = Color.green; // Grass
                else if (perlinValue < 0.6f)
                    color = Color.yellow; // Dirt
                else
                    color = Color.white; // Snow
                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply(); // Apply changes to the texture
        return tex;
    }

    private Vector3[,] SmoothVertexGrid(Vector3[,] vertices)
    {
        Vector3[,] smoothedVertices = new Vector3[X + 1, Y + 1];
        for(int k = 0; k <= SmoothingIteration; k++){

            for (int i = 0; i <= X; i++) // This loop onwards is chatgpt
            {
                for (int j = 0; j <= Y; j++)
                {
                    // Sum up the neighboring vertex heights
                    float sumHeight = 0f;
                    int count = 0;

                    // Iterate through the neighbors, including the current vertex
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        for (int offsetY = -1; offsetY <= 1; offsetY++)
                        {
                            int neighborX = i + offsetX;
                            int neighborY = j + offsetY;

                            // Check if the neighbor is within bounds
                            if (neighborX >= 0 && neighborX < X && neighborY >= 0 && neighborY < Y)
                            {
                                sumHeight += vertices[neighborX, neighborY].y; // Add the neighbor's height
                                count++; // Keep track of the number of neighbors
                            }
                        }
                    }

                    // Calculate the average height, including the smoothing factor
                    float averageHeight = sumHeight / count;
                    float smoothedHeight = Mathf.Lerp(vertices[i, j].y, averageHeight, SmoothFactor);

                    // Set the smoothed vertex
                    smoothedVertices[i, j] = new Vector3(i, smoothedHeight, j);
                }
            }
            vertices = smoothedVertices;
        }
        
       

        return smoothedVertices;
    }

    private void GenerateLandscapeAsTiles(){
        Vector3[,] vertices = GenerateVertices(MinHeight, MaxHeight);

        for(int i = 0; i < X; i++){
            for(int j = 0; j < Y; j++){
                GameObject tile = new GameObject($"Tile ({i},{j})");
                tile.transform.parent = gameObject.transform;   // reparent for 

                MeshFilter meshFilter = tile.AddComponent<MeshFilter>();
                meshFilter.mesh = GetQuad(vertices[i,j], vertices[i + 1, j], vertices[i, j + 1], vertices[i + 1, j + 1]);

                MeshRenderer meshRenderer = tile.AddComponent<MeshRenderer>();
                
                float perlinNoise = Mathf.PerlinNoise((float)((i + PerlinSeed) * PerlinScale), (float) ((j + PerlinSeed) * PerlinScale));
                
                float color = Mathf.Lerp(0.0f, 1.0f, perlinNoise);

                meshRenderer.material = new Material(Shader.Find("Standard"));
                meshRenderer.material.SetFloat("_Smoothness", 0.0f);
                meshRenderer.material.SetFloat("_Metallic", 0.0f);
                meshRenderer.material.color = new Color(0, color, 0);
            }
        }
    }

    /// <summary>
    /// Creates a number of vertices equal to the grid size required.
    /// </summary>
    /// <returns>A 2d array containing the vertices</returns>
    private Vector3[,] GenerateVertices(float minHeight, float maxHeight){
        Vector3[,] vertices = new Vector3[X + 1, Y + 1];
        for(int i = 0; i <= X; i++){
            for(int j = 0; j <= Y; j++){
                float perlinNoise = Mathf.PerlinNoise((float)((i + PerlinSeed) * PerlinScale), (float) ((j + PerlinSeed)* PerlinScale));
                vertices[i, j] = new Vector3(i, Mathf.Lerp(minHeight, maxHeight, perlinNoise), j); 
            }
        }

        return vertices;
    }

    private Vector3[,] GenerateVerticesWithTime(float minHeight, float maxHeight, float time){
        Vector3[,] vertices = new Vector3[X + 1, Y + 1];
        for(int i = 0; i <= X; i++){
            for(int j = 0; j <= Y; j++){
                float perlinNoise = Mathf.PerlinNoise((float)((i + PerlinSeed) * PerlinScale + time), (float) ((j + PerlinSeed)* PerlinScale + time));
                vertices[i, j] = new Vector3(i, Mathf.Lerp(minHeight, maxHeight, perlinNoise), j);
            }
        }

        return vertices;
    }



    /// <summary>
    /// Generates a quad mesh from 4 vertices
    /// </summary>
    /// <param name="bottomLeft">Bottom left vertex</param>
    /// <param name="bottomRight">Bottom right vertex</param>
    /// <param name="topLeft">top left vertex</param>
    /// <param name="topRight">top right vertex</param>
    /// <returns>A 4 pointed mesh.</returns>
    private Mesh GetQuad(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight){
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            bottomLeft,
            bottomRight,
            topLeft,
            topRight,
        };

        int[] triangles = new int[]
        {
            0, 2, 1, // First triangle, bottom left, bottom right, top left
            1, 2, 3  // Second triangle
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }


    public void Update(){
        deltaTime += Time.deltaTime * WaterSpeed;
        GenerateWater();
        
    }


}
