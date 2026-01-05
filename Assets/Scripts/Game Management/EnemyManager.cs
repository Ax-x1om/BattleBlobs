using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; set; }

    float attackTime = 60f;
    float attackTimer;

    int attackNumber;

    public List<GameObject> allEnemiesList = new List<GameObject>();

    List<Vector3> Waypoints = new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        else
        {
            PathRequestManager.RequestPath(new PathRequest(COMofUnits(), UnitSelectionManager.Instance.COMofAllUnits(), OnPathFound));
            attackTimer = attackTime + Random.Range(-attackTime/5f,attackTime/5f);
        }
    }

    public void OnPathFound(Vector3[] newpath, bool pathSucessful)
    {
        Waypoints.Clear();
        // Creates waypoints when the A* finds a path
        Debug.Log("Was path sucessful?: " + pathSucessful);
        if (pathSucessful)
        {
            foreach (Vector3 point in newpath)
            {
                Waypoints.Add(point);
            }
        }
        Debug.Log("SelectionManager Waypoints Length: " + Waypoints.Count);

        
        foreach (GameObject unit in allEnemiesList)
        {
            if (Random.value > 0.6f)
            {
                if (unit.GetComponent<BaseEnemyScript>().getState() != "Moving")
                {
                    unit.GetComponent<BaseEnemyScript>().setPath(Waypoints.ToArray());
                    unit.GetComponent<BaseEnemyScript>().setState("Moving");
                }
            }
        }
        
    }
    Vector3 COMofUnits()
    {
        // Gets the average location of all selected units
        float n = 0;
        Vector3 COM = Vector3.zero;
        foreach (GameObject unit in allEnemiesList)
        {
            // Adds up all their positions
            COM += unit.transform.position;
            n += 1;
        }
        // Divides by N to get the mean
        COM = COM / n;
        // Makes it flat
        COM.y = 0;
        return COM;
    }
}
