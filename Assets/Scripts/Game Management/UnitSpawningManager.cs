using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class UnitSpawningManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Camera cam;

    public GameObject soldierSpawner;
    public GameObject enemysoldierSpawner;
    public GameObject enemyManager;

    List<GameObject> spawnersList = new List<GameObject>();

    Vector2 enemyRegionSize = new Vector2(1000f, 500f);
    Vector2 enemyBottomLeftCorner = new Vector2(-500f, 0f);
    List<Vector2> enemySoldierSpawnLocations;

    public GameObject unitSelectionManager;
    public GameObject selectionBox;

    int num_soldiers = 20;

    Quaternion playerRotation;
    Quaternion enemyRotation;

    LayerMask ground;
    LayerMask spawner;
    void Start()
    {
        cam = Camera.main;
        ground = LayerMask.GetMask("Ground");
        spawner = LayerMask.GetMask("PlayerSpawner");

        playerRotation = Quaternion.Euler(0f, 0f, 0f);
        enemyRotation = Quaternion.Euler(0f, 180f, 0f);

        // Creates a series of random spawn locations
        enemySoldierSpawnLocations = PoissonDiscSampling.GeneratePoints(5f, enemyRegionSize, enemyBottomLeftCorner, 30, num_soldiers);

        // Creates enemy spawners
        foreach (Vector2 spawnpoint in enemySoldierSpawnLocations)
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
                GameObject newEnemySpawner = Instantiate(enemysoldierSpawner, hit.point + 3 * Vector3.up, enemyRotation);
                spawnersList.Add(newEnemySpawner);
            }
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
        // if they're both 1, we return true
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

                if (LayerIsInLayerMask(ground, testLayer))
                {
                    if (num_soldiers > 0)
                    {
                        GameObject newSoldier = Instantiate(soldierSpawner, hit.point + 3 * Vector3.up, playerRotation);
                        spawnersList.Add(newSoldier);
                        num_soldiers -= 1;
                    }
                }
                else if (LayerIsInLayerMask(spawner, testLayer))
                {
                    Destroy(hit.collider.gameObject);
                    spawnersList.Remove(hit.collider.gameObject);
                    num_soldiers += 1;
                }
            }
        }
        if (Input.GetKeyDown("space"))
        {
            unitSelectionManager.SetActive(true);
            selectionBox.SetActive(true);
            enemyManager.SetActive(true);
            foreach (GameObject spawner in spawnersList)
            {
                spawner.GetComponent<Spawner>().SpawnUnit();
            }
            spawnersList.Clear();
        }
    }
}
