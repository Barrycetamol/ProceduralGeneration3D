using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject prefab; // Tree or collectible prefab
    public Vector2 regionSize = new Vector2(512, 512);
    public float radius = 3f; // Minimum spacing between objects
    public float terrainHeight = 0f; // Set height for objects

    void Start()
    {
        List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius, regionSize);

        foreach (Vector2 point in points)
        {
            Vector3 position = new Vector3(point.x, terrainHeight, point.y);
            Instantiate(prefab, position, Quaternion.identity);
        }
    }
}