using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

public class TerrainGenrator : MonoBehaviour
{
    // Gets object and script
    public GameObject MapGenerator;
    MapGenerator mapGenerator;

    // Puts all the variables for the MapGenerator in one place
    readonly int TerrainSize = 500;

    public GameObject tree;
    float radius = 20f;
    float maxTerrainHeight = 170f;
    float terrainScale = 2f;
    float[,] heightmap;

    int seed;
    Vector2 offset;

    // Variables for creating muddy areas
    float mudThreshold = 0.75f;
    float lowMudHeight = 0.18f;
    float maxMudHeight = 0.35f;
    float[,] premudmap;
    bool[,] mudmap;

    float treeDropOffHeight = 0.2f;
    float maxTreeHeight = 0.4f;

    Vector2 regionsize = Vector2.one;
    Vector3 TreeUpVector = Vector3.up * 1.5f;
    Vector2 bottomLeftCorner = new Vector2(-500f, -500f);
    int rejectionCount = 12;

    List<Vector2> PoissonPoints;
    //List<Vector3> obstaclePoints = new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapGenerator = MapGenerator.GetComponent<MapGenerator>();

        // Creates a Random Seed
        seed = Mathf.RoundToInt(UnityEngine.Random.Range(0f, 10000f));
        // Creates a random offset
        offset.Set(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-1000f, 1000f));

        mapGenerator.seed = seed;
        mapGenerator.offset = offset;
        mapGenerator.mapWidth = TerrainSize;
        mapGenerator.mapHeight = TerrainSize;

        mudmap = new bool[TerrainSize, TerrainSize];

        regionsize *= TerrainSize * terrainScale;

        heightmap = mapGenerator.getHeightMap();

        // Creates the list of floats for the mudmap
        offset.Set(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-1000f, 1000f));
        premudmap = Noise.GenerateNoiseMap(500, 500, seed + 1, 18f, 1, 1, 1, 0, offset);

        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

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
                    // The random assortment of floats is to make a clean lerp between the threshold being 0.5 at 0.3 and 1 at 0.43
                    if (premudmap[x, z] > LineFromTwoPoints(maxMudHeight, lowMudHeight, heightmap[x, z], 1, mudThreshold))
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

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;


        //for (int z = 0; z < height; z++)
        //{
        //    for (int x = 0; x < width; x++)
        //    {
        //        obstaclePoints.Add(new Vector3(terrainSize * (topLeftX + x), heightmap[x, z] * maxTerrainHeight, terrainSize * (topLeftZ - z)));
        //    }
        //}
        //Debug.Log(obstaclePoints.Count);

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
    }

    // Update is called once per frame
    void Update()
    {

    }

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
}
