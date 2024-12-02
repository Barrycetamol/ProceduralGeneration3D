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

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">Name of our gameobject</param>
    /// <param name="width">Width of our mesh</param>
    /// <param name="height">Height of our mesh</param>
    /// <param name="useDeltaTime">Flag that designates whether this terrain should use deltaTime when calculating its mesh</param>
    public Terrain(String name, Vector2Int gridSize, Vector2Int gridPosition, bool useDeltaTime){
        GridPosition = gridPosition;
        GridSize = gridSize;
        UseDeltaTime = useDeltaTime;

        m_Terrain = new GameObject(name);
        m_Terrain.transform.position = new Vector3(GridPosition.x * (GridSize.x - 1), 0, GridPosition.y * (GridSize.y - 1));

        Mesh = new Mesh();
        MeshRenderer = m_Terrain.AddComponent<MeshRenderer>();
        MeshFilter= m_Terrain.AddComponent<MeshFilter>();

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

        MeshFilter.mesh = Mesh;
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
        CreateTriangles();
    }

    public void SetColors(List<Color> colors){
        ColorList = colors;
    }


    private void CreateTriangles(){

        for(int i = 0; i < Vertices.GetLength(1) - 1; i++){
            for(int j = 0; j < Vertices.GetLength(0) - 1; j++){

                Vector3 bottomLeft = Vertices[i, j];
                Vector3 topLeft = Vertices[i, j + 1];
                Vector3 bottomRight = Vertices[i + 1, j];
                Vector3 topRight = Vertices[i + 1, j + 1];

                // Add vertices to the vertex list
                int vertexIndex = VertexList.Count;
                VertexList.Add(bottomLeft);
                VertexList.Add(topLeft);
                VertexList.Add(bottomRight);
                VertexList.Add(topRight);

                // Add triangles for the two triangles of the quad
                TriangleList.Add(vertexIndex);
                TriangleList.Add(vertexIndex + 1);
                TriangleList.Add(vertexIndex + 2);
                TriangleList.Add(vertexIndex + 1);
                TriangleList.Add(vertexIndex + 3);
                TriangleList.Add(vertexIndex + 2);
            }
        }
    }
}