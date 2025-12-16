using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.GridLayoutGroup;

public class Grid : MonoBehaviour
{
    public Transform player;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldTopLeft = transform.position - Vector3.right * gridWorldSize.x / 2 + Vector3.forward * gridWorldSize.y / 2;

        // Change
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldTopLeft + Vector3.right * (x * nodeDiameter + nodeRadius) - Vector3.forward * (y * nodeDiameter + nodeRadius);
                Vector3 RayStart = worldPoint;
                RayStart.y = 200;

                Ray HeightCheck = new Ray(RayStart, Vector3.down);
                float height = 0;
                if (Physics.Raycast(HeightCheck, out var hitInfo))
                {
                    height = hitInfo.point.y;
                }
                bool walkable = true;
                worldPoint.y = height;
                grid[x, y] = new Node(walkable, worldPoint, x, y, height);
            }
        }
    }

    public List<Node> GetNeigbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                int CheckX = node.GridX + x;
                int CheckY = node.GridY + y;

                if (CheckX >= 0 && CheckX < gridSizeX && CheckY >= 0 && CheckY < gridSizeY)
                {
                    neighbours.Add(grid[CheckX, CheckY]);
                }
            }
        }
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = 1 - Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }
}
    
