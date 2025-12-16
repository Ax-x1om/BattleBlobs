using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, Vector2 bottomLeftCorner, int numSamplesBeforeRejection = 30)
    {
        // So any 2 points in the same grid will always be too close and you only need to check within a 5x5 block of cells
        float cellSize = radius / Mathf.Sqrt(2);
        Vector2 topRightCorner = bottomLeftCorner + sampleRegionSize;

        // Integer of points with their indexes in the point list
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        // List of points we are going to output
        List<Vector2> points = new List<Vector2>();
        // Potential spawnpoints
        List<Vector2> spawnpoints = new List<Vector2>();

        // Inital spawnpoint
        spawnpoints.Add((topRightCorner + bottomLeftCorner) / 2);
        // Runs loop while there are still possible points to spawn
        while (spawnpoints.Count > 0)
        {
            // Picks a random point to attempt to spawn from
            int spawnIndex = Random.Range(0, spawnpoints.Count);
            Vector2 spawnCentre = spawnpoints[spawnIndex];

            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                // Creates a random angle from 0 to 2pi radians
                float angle = Random.value * Mathf.PI * 2;
                // Random unit vector
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                // Creates a random point between 1 and 2 radii away from the initial point
                Vector2 candidate = spawnCentre + dir * Random.Range(radius, 2 * radius);

                if (IsValid(candidate, bottomLeftCorner, topRightCorner, cellSize, radius, points, grid))
                {
                    // If the candidate is valid, it adds the point to the potential spawnpoints and spawn indexes
                    points.Add(candidate);
                    spawnpoints.Add(candidate);
                    // Adds the index of the point to the grid so no more points get added
                    grid[(int)((candidate.x - bottomLeftCorner.x) / cellSize), (int)((candidate.y - bottomLeftCorner.y) / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                // Removes point from spawnpoints since it's not possible to spawn another point there
                spawnpoints.RemoveAt(spawnIndex);
            }
        }
        return points;
    }
    static bool IsValid(Vector2 candidate, Vector2 bottomLeftCorner, Vector2 topRightCorner, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        // Checks if the point is within the bounds of the area
        if (candidate.x >= bottomLeftCorner.x && candidate.x < topRightCorner.x && candidate.y >= bottomLeftCorner.y && candidate.y < topRightCorner.y)
        {
            // Gets cell in grid from the points coordinate
            int CellX = (int)((candidate.x - bottomLeftCorner.x) / cellSize);
            int CellY = (int)((candidate.y - bottomLeftCorner.y) / cellSize);
            // Searches either within a 5x5 square of the point
            // Doesn't search ouside the grid
            int searchStartX = Mathf.Max(0, CellX - 2);
            int searchEndX = Mathf.Min(CellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, CellY - 2);
            int searchEndY = Mathf.Min(CellY + 2, grid.GetLength(1) - 1);

            // Checks the 5x5 for points in the grid
            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    // checks if there is a point in the square
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        // Checks the distance between the points
                        // Computin sqrMagnitude is faster than computing the magnitude so we do that
                        float sqrdist = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrdist < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }
}
