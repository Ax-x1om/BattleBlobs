using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.GridLayoutGroup;

public class A_Star_Grid : MonoBehaviour
{
    public Transform player;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public Vector2 topLeft;
    public float terrainSize;
    public float maxTerrainHeight;
    int mudPenalty = 20;
    public float nodeRadius;
    Node[,] grid;

    public float[,] heightMap;
    public bool[,] mudmap;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        Debug.Log(nodeRadius);
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        Debug.Log(gridSizeX);
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
        Vector3 worldTopLeft = new Vector3(topLeft.x, 0f, topLeft.y);

        // Change
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                float height = heightMap[x, y] * maxTerrainHeight;
                Vector3 worldPoint = new Vector3(terrainSize * (topLeft.x + x), heightMap[x, y] * maxTerrainHeight, terrainSize * (topLeft.y - y));

                int movePenalty = 0;
                if (mudmap[x, y])
                {
                    movePenalty = mudPenalty;
                }

                grid[x, y] = new Node(true, worldPoint, x, y, height, movePenalty);
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
        float X = worldPosition.x;
        float Z = worldPosition.z;
        int x = Mathf.FloorToInt((X - topLeft.x) / terrainSize);
        int z = Mathf.FloorToInt((topLeft.y - Z) / terrainSize);
        return grid[x, z];
    }
}
    
