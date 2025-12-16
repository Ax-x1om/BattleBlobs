using Unity.Mathematics;
using UnityEditor.AssetImporters;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    // public enum DrawMode { NoiseMap, ColourMap, Mesh };
    // public DrawMode drawMode;

    // Turn all of these into private readonlies
    public int mapWidth = 500;
    public int mapHeight = 500;
    readonly float noiseScale = 250f;

    readonly int octaves = 4;

    readonly float persistance = 0.52f;
    readonly float lacunarity = 2f;

    readonly float erosionCoefficient = 1.8f;

    // Get passed in
    public int seed;
    public Vector2 offset;

    Color mudcolor = new Color(0.45882f, 0.32941f, 0.21569f);

    // Creates each of the terrain types and puts them an array
    // Inlined because it would be monstrosly long otherwise
    TerrainType[] regions = new TerrainType[] {
    new TerrainType(0.18f, new Color(0f, 1f, 0.07843f)),
    new TerrainType(0.35f, new Color(0f, 0.61569f, 0.04313f)),
    new TerrainType(0.44f, new Color(0.29019f,0.58039f,0.26275f)),
    new TerrainType(0.59f, new Color(0.20392f, 0.28235f, 0.20784f)),
    new TerrainType(0.65f, new Color(0.28235f, 0.29412f, 0.27059f)),
    new TerrainType(0.71f, new Color(0.44706f, 0.44706f, 0.44706f)),
    new TerrainType(0.81f, new Color(0.85098f, 0.85098f, 0.85098f)),
    new TerrainType(1.1f, new Color(1f, 1f, 1f)),
    };

    public void GenerateMap(float[,] noiseMap, bool[,] mudmap)
    {
        Color[] colourMap = new Color[mapWidth * mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (mudmap[x, y] == true)
                {
                    colourMap[y * mapWidth + x] = mudcolor;
                }
                else
                {
                    float CurrentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (CurrentHeight <= regions[i].height)
                        {
                            colourMap[y * mapWidth + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }
        }

        MapDisplay display = FindFirstObjectByType<MapDisplay>();

        display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
    }

    public float[,] getHeightMap()
    {
        return Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, erosionCoefficient, offset);
    }
}

[System.Serializable]
public struct TerrainType
{
    public float height;
    public Color colour;

    public TerrainType(float _height, Color _color)
    {
        this.height = _height;
        this.colour = _color;
    }
}
