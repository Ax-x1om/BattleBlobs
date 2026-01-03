using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator Instance { get; set; }

    // Gets object and script
    public GameObject MapGenerator;

    // To be removed
    public GameObject A_Star;
    MapGenerator mapGenerator;

    // To be removed
    A_Star_Grid aStarGrid;

    // Puts all the variables for the MapGenerator in one place
    readonly int TerrainSize = 500;

    public GameObject tree;
    float radius = 20f;
    float maxTerrainHeight = 170f;
    public float terrainScale = 2f;
    float[,] heightmap;

    int seed;
    Vector2 offset;

    // Variables for creating muddy areas
    float mudThreshold = 0.75f;
    float lowMudHeight = 0.18f;
    float maxMudHeight = 0.35f;
    float[,] premudmap;
    public bool[,] mudmap;

    float treeDropOffHeight = 0.2f;
    float maxTreeHeight = 0.4f;

    Vector2 regionsize = Vector2.one;
    Vector3 TreeUpVector = Vector3.up * 1.5f;
    Vector2 bottomLeftCorner;
    public Vector2 topLeftcorner;
    int rejectionCount = 12;

    List<Vector2> PoissonPoints;
    //List<Vector3> obstaclePoints = new List<Vector3>();

    public GameObject Unit;
    public GameObject Enemy;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapGenerator = MapGenerator.GetComponent<MapGenerator>();

        bottomLeftCorner = Vector2.one * -TerrainSize / 2 * terrainScale;

        topLeftcorner = bottomLeftCorner + Vector2.up * terrainScale * TerrainSize;
        // Creates a Random Seed
        seed = Mathf.RoundToInt(UnityEngine.Random.Range(0f, 10000f));
        // Creates a random offset
        offset.Set(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-1000f, 1000f));

        // Sets variables for A*
        aStarGrid = A_Star.GetComponent<A_Star_Grid>();

        aStarGrid.maxTerrainHeight = maxTerrainHeight;
        aStarGrid.terrainSize = terrainScale;
        aStarGrid.gridWorldSize = Vector2.one * terrainScale * TerrainSize;
        aStarGrid.nodeRadius = 0.5f * terrainScale;
        aStarGrid.topLeft = topLeftcorner;

        // Sets Variables for the map generator
        mapGenerator.seed = seed;
        mapGenerator.offset = offset;
        mapGenerator.mapWidth = TerrainSize;
        mapGenerator.mapHeight = TerrainSize;
        mapGenerator.horizontalScale = terrainScale;
        mapGenerator.verticalScale = maxTerrainHeight;

        mudmap = new bool[TerrainSize, TerrainSize];

        regionsize *= TerrainSize * terrainScale;

        heightmap = mapGenerator.getHeightMap();

        // Creates the list of floats for the mudmap
        offset.Set(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-1000f, 1000f));
        premudmap = Noise.GenerateNoiseMap(500, 500, seed + 1, 18f, 1, 1, 1, 0, offset);

        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        // Creates the list of bools for the mudmap
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                if (heightmap[x, z] < lowMudHeight && premudmap[x, z] > mudThreshold)
                {
                    mudmap[x, z] = true;
                }
                else if (heightmap[x, z] < maxMudHeight && premudmap[x, z] > mudThreshold)
                {
                    if (premudmap[x, z] > LineFromTwoPoints(lowMudHeight, maxMudHeight, heightmap[x, z], mudThreshold))
                    {
                        mudmap[x, z] = true;
                    }
                    else
                    {
                        mudmap[x, z] = false;
                    }
                }
                else
                {
                    mudmap[x, z] = false;
                }
            }
        }
        Debug.Log("Mudmap Created");
        Debug.Log("Terrain Generator: " + mudmap);

        // Sets the heightmap and mudmaps into the grid so the weights and heights can be calculated properly
        aStarGrid.heightMap = heightmap;
        aStarGrid.mudmap = mudmap;

        mapGenerator.GenerateMap(heightmap, mudmap);

        PoissonPoints = PoissonDiscSampling.GeneratePoints(radius, regionsize, bottomLeftCorner, rejectionCount);

        foreach (Vector2 treepoint in PoissonPoints)
        {
            Vector3 rayCastStart = V2toV3(treepoint);
            Ray HeightCheck = new Ray(rayCastStart, Vector3.down);
            if (Physics.Raycast(HeightCheck, out var hitInfo))
            {
                float scaledHeight = hitInfo.point.y / maxTerrainHeight;
                float probability = LineFromTwoPoints(maxTreeHeight, treeDropOffHeight, scaledHeight);

                if (probability >= 1)
                {
                    GameObject Tree = Instantiate(tree, hitInfo.point + TreeUpVector, Quaternion.Euler(0f, Random.value * 360f, 0f));
                }
                else if (probability > 0)
                {
                    if (probability > Random.value)
                    {
                        GameObject Tree = Instantiate(tree, hitInfo.point, Quaternion.identity);
                    }
                }
            }
        }

        // Spawns units for debugging, remove later
        Vector3 Start = new Vector3(0f, 200f, 0f);
     
        for (int i = 0; i < 10; i++)
        {
            Ray Check = new Ray(Start, Vector3.down);

            if (Physics.Raycast(Check, out var Info))
            {
                GameObject UnitTest = Instantiate(Unit, Info.point + Vector3.up * 5f, Quaternion.identity);
            }

            Start += 5 * RandomVector();
        }

        Vector3 EnemyStart = new Vector3(0f, 200f, 100f);

        for (int i = 0; i < 10; i++)
        {
            Ray Check = new Ray(EnemyStart, Vector3.down);

            if (Physics.Raycast(Check, out var Info))
            {
                GameObject UnitTest = Instantiate(Enemy, Info.point + Vector3.up * 2f, Quaternion.identity);
            }

            EnemyStart += 5 * RandomVector();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Creates a line from two points and calculates where a value falls on said line
    float LineFromTwoPoints(float x_0, float x_1, float x, float y_0 = 0, float y_1 = 1, bool clamp = false)
    {
        // Classic gradient formula
        float gradient = (y_1 - y_0) / (x_1 - x_0);
        // A reearangement of y = mx+c to get c
        float y_intercept = y_0 - gradient * x_0;
        // y = mx + c
        float value = gradient * x + y_intercept;
        if (clamp)
        {
            value = Mathf.Clamp01(value);
        }

        return value;
    }


    Vector3 V2toV3(Vector2 V2, float Ycoord = 200f)
    {
        Vector3 V3 = Vector3.zero;
        V3.Set(V2.x, Ycoord, V2.y);
        return V3;
    }

    // Remove later
    Vector3 RandomVector()
    {
        float angle = Random.Range(-Mathf.PI, Mathf.PI);
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }
}
