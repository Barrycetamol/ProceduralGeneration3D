using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    public static List<Vector3> GeneratePoints(
        float radius,
        Vector2 regionSize,
        int numSamplesBeforeRejection,
        Vector2 heightThresholds,
        float[] noiseMap,
        float meshHeightMultiplier
    )
    {
        List<Vector3> points = new List<Vector3>();
        float cellSize = radius / Mathf.Sqrt(2);

        // Grid setup
        int[,] grid = new int[Mathf.CeilToInt(regionSize.x / cellSize), Mathf.CeilToInt(regionSize.y / cellSize)];
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(regionSize / 2); // Start at the center of the region

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];
            bool pointAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCenter + direction * Random.Range(radius, 2 * radius);

                if (IsValid(candidate, regionSize, cellSize, radius, grid, points, noiseMap, heightThresholds, meshHeightMultiplier))
                {
                    // Align candidate with noise map and terrain height
                    float noiseValue = GetNoiseValue(candidate, regionSize, noiseMap);
                    float height = noiseValue * meshHeightMultiplier;

                    points.Add(new Vector3(candidate.x, height, candidate.y));
                    spawnPoints.Add(candidate);
                    grid[Mathf.FloorToInt(candidate.x / cellSize), Mathf.FloorToInt(candidate.y / cellSize)] = points.Count;
                    pointAccepted = true;
                    break;
                }
            }

            if (!pointAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return points;
    }

    private static bool IsValid(
        Vector2 candidate,
        Vector2 regionSize,
        float cellSize,
        float radius,
        int[,] grid,
        List<Vector3> points,
        float[] noiseMap,
        Vector2 heightThresholds,
        float meshHeightMultiplier
    )
    {
        // Check if the candidate is within bounds
        if (candidate.x < 0 || candidate.x >= regionSize.x || candidate.y < 0 || candidate.y >= regionSize.y)
            return false;

        // Calculate grid coordinates
        int cellX = Mathf.FloorToInt(candidate.x / cellSize);
        int cellY = Mathf.FloorToInt(candidate.y / cellSize);

        // Check neighboring cells for minimum distance
        int searchStartX = Mathf.Max(0, cellX - 2);
        int searchEndX = Mathf.Min(grid.GetLength(0) - 1, cellX + 2);
        int searchStartY = Mathf.Max(0, cellY - 2);
        int searchEndY = Mathf.Min(grid.GetLength(1) - 1, cellY + 2);

        for (int x = searchStartX; x <= searchEndX; x++)
        {
            for (int y = searchStartY; y <= searchEndY; y++)
            {
                int pointIndex = grid[x, y] - 1;
                if (pointIndex != -1)
                {
                    float sqrDst = (points[pointIndex] - new Vector3(candidate.x, 0, candidate.y)).sqrMagnitude;
                    if (sqrDst < radius * radius)
                        return false;
                }
            }
        }

        // Check noise map value
        float noiseValue = GetNoiseValue(candidate, regionSize, noiseMap);
        return noiseValue >= heightThresholds.x && noiseValue <= heightThresholds.y;
    }

    private static float GetNoiseValue(Vector2 position, Vector2 regionSize, float[] noiseMap)
    {
        int width = Mathf.FloorToInt(regionSize.x);
        int height = Mathf.FloorToInt(regionSize.y);

        int noiseX = Mathf.FloorToInt(position.x);
        int noiseY = Mathf.FloorToInt(position.y);

        // Ensure valid indices
        if (noiseX < 0 || noiseX >= width || noiseY < 0 || noiseY >= height)
            return 0;

        int noiseIndex = noiseY * width + noiseX;
        return noiseMap[noiseIndex];
    }

    public static void DrawGizmos(List<Vector3> points, float gizmoSize = 0.5f)
    {
        Gizmos.color = Color.green;

        foreach (var point in points)
        {
            Gizmos.DrawSphere(point, gizmoSize);
        }
    }
}