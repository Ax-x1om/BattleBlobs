using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class UnitSpawningManager : MonoBehaviour
{
    public static UnitSpawningManager Instance { get; set; }

    private Camera cam;

    public GameObject soldierSpawner;
    public GameObject bruteSpawner;
    public GameObject enemysoldierSpawner;
    public GameObject enemybruteSpawner;
    public GameObject enemyManager;

    public bool Active = true;

    List<GameObject> playerSpawnersList = new List<GameObject>();
    List<GameObject> enemySpawnersList = new List<GameObject>();

    Vector2 enemyRegionSize = new Vector2(1000f, 500f);
    Vector2 enemyBottomLeftCorner = new Vector2(-500f, 0f);
    List<Vector2> enemySpawnLocations;
    List<int> bruteIndexes;

    public GameObject unitSelectionManager;
    public GameObject selectionBox;

    public int num_soldiers = 20;
    public int num_brutes = 5;

    Quaternion playerRotation;
    Quaternion enemyRotation;

    LayerMask ground;
    LayerMask spawner;
    LayerMask bspawner;

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

    List<int> UniqueRandomNumbers(int min, int max, int n_numbers)
    {
        List<int> numbersList = new List<int>();
        numbersList.Add(Random.Range(min, max));
        while (numbersList.Count < n_numbers)
        {
            int candidate = Random.Range(min, max);
            if (!(numbersList.Contains(candidate)))
            {
                numbersList.Add(candidate);
            }
        }
        return numbersList;
    }

    void Start()
    {
        cam = Camera.main;
        ground = LayerMask.GetMask("Ground");
        spawner = LayerMask.GetMask("PlayerSpawner");
        bspawner = LayerMask.GetMask("BruteSpawner");

        playerRotation = Quaternion.Euler(0f, 0f, 0f);
        enemyRotation = Quaternion.Euler(0f, 180f, 0f);

        // Creates a series of random spawn locations
        enemySpawnLocations = PoissonDiscSampling.GeneratePoints(5f, enemyRegionSize, enemyBottomLeftCorner, 30, num_soldiers + num_brutes);
        bruteIndexes = UniqueRandomNumbers(0, num_soldiers + num_brutes, num_brutes);
        int i = 0;

        // Creates enemy spawners
        foreach (Vector2 spawnpoint in enemySpawnLocations)
        {
            Vector3 rayCastStart = V2toV3(spawnpoint);
            Ray Check = new Ray(rayCastStart, Vector3.down);
            if (Physics.Raycast(
                ray: Check,
                hitInfo: out var hit,
                maxDistance: Mathf.Infinity,
                layerMask: ground)
                )
            {
                if (bruteIndexes.Contains(i))
                {
                    GameObject newEnemySpawner = Instantiate(enemybruteSpawner, hit.point + 3 * Vector3.up, enemyRotation);
                    enemySpawnersList.Add(newEnemySpawner);
                }
                else
                {
                    GameObject newEnemySpawner = Instantiate(enemysoldierSpawner, hit.point + 3 * Vector3.up, enemyRotation);
                    enemySpawnersList.Add(newEnemySpawner);
                }
            }
            i++;
        }
    }

    Vector3 V2toV3(Vector2 V2, float Ycoord = 200f)
    {
        Vector3 V3 = Vector3.zero;
        V3.Set(V2.x, Ycoord, V2.y);
        return V3;
    }

    bool LayerIsInLayerMask(LayerMask layermask, int layer)
    {
        // Layermasks are bitmasks and layers are ints
        // so we shift the layer according to what number it is and compate it to the layermask
        // if the bitwise AND results in something non-zero, it means it's in the layermask
        int bitShiftedLayer = 1 << layer;
        int result = (layermask & bitShiftedLayer);
        if (result != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    // Update is called once per frame
    void Update()
    { 
        // Spawns soldiers when the player wants to spawn them
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray movePosition = cam.ScreenPointToRay(Input.mousePosition);
            // If we hit a clickable object
            if (Physics.Raycast(movePosition, out hit, Mathf.Infinity, ground + spawner))
            {
                int testLayer = hit.collider.gameObject.layer;

                // checks layer
                if (LayerIsInLayerMask(ground, testLayer))
                {
                    if (num_soldiers > 0)
                    {
                        // If the layer is the ground, spawn a unit
                        GameObject newSoldier = Instantiate(soldierSpawner, hit.point + 3 * Vector3.up, playerRotation);
                        playerSpawnersList.Add(newSoldier);
                        // Removes one from the number of soldiers that can get spawned
                        num_soldiers -= 1;
                    }
                }
                else if (LayerIsInLayerMask(spawner, testLayer))
                {
                    // if the layer is a playerspawner, delete it 
                    Destroy(hit.collider.gameObject);
                    playerSpawnersList.Remove(hit.collider.gameObject);
                    // Adds one to the number of soldiers that can get spawned
                    num_soldiers += 1;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray movePosition = cam.ScreenPointToRay(Input.mousePosition);
            // If we hit a clickable object
            if (Physics.Raycast(movePosition, out hit, Mathf.Infinity, ground + spawner))
            {
                // Pretty much the same logic as above
                int testLayer = hit.collider.gameObject.layer;

                if (LayerIsInLayerMask(ground, testLayer))
                {
                    if (num_brutes > 0)
                    {
                        GameObject newSoldier = Instantiate(bruteSpawner, hit.point + 3 * Vector3.up, playerRotation);
                        playerSpawnersList.Add(newSoldier);
                        num_brutes -= 1;
                    }
                }
                else if (LayerIsInLayerMask(bspawner, testLayer))
                {
                    Destroy(hit.collider.gameObject);
                    playerSpawnersList.Remove(hit.collider.gameObject);
                    num_brutes += 1;
                }
            }
        }
        else if (Input.GetKeyDown("space"))
        {
            unitSelectionManager.SetActive(true);
            selectionBox.SetActive(true);
            enemyManager.SetActive(true);
            enemyManager.GetComponent<EnemyManager>().setMaxEnemies(enemySpawnersList.Count);
            foreach (GameObject spawner in enemySpawnersList)
            {
                spawner.GetComponent<Spawner>().SpawnUnit();
            }
            enemySpawnersList.Clear();
            foreach (GameObject spawner in playerSpawnersList)
            {
                spawner.GetComponent<Spawner>().SpawnUnit();
            }
            playerSpawnersList.Clear();

            Active = false;
        }
    }
}
