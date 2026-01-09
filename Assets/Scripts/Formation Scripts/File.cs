using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class File : MonoBehaviour
{
    public int n_ranks;
    public bool moving = false;
    public List<GameObject> UnitsInFile = new List<GameObject>();
    Vector3[] UnitPositions;
    Vector3[] Waypoints;

    readonly float speed = 2f;
    readonly float TurnSpeed = 0.2f;
    Quaternion TargetRotation;
    int pointIndex = 0;

    // Path of points that it will pathfind to
    Directions path;

    public float HorizontalSpacing;
    // This variable gets set by UnitSelectionManager

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnitPositions = new Vector3[n_ranks];
    }

    private void OnDestroy()
    {
        foreach (GameObject unit in UnitsInFile)
        {
            if (unit)
            {
                // Makes units shuffle to break up the formation
                
                unit.GetComponent<BaseUnitScript>().setState("Shuffling");
            }
        }
        UnitsInFile.Clear();
    }

    public void AddUnit(GameObject unit)
    {
        UnitsInFile.Add(unit);
    }

    public void StopMarching()
    {
        // Makes each unit stop marching when the file stops moving
        foreach (GameObject unit in UnitsInFile)
        {
            if (unit)
            {
                unit.GetComponent<BaseUnitScript>().setState("At Ease");
            }
        }
    }

    // Change this so it trails behind a target instead of following a path
    public void SetPath(Directions newpath)
    {
        path = newpath;
        moving = true;
        foreach (GameObject unit in UnitsInFile)
        {
            if (unit)
            {
                unit.GetComponent<BaseUnitScript>().setState("Marching");
            }
        }
        Waypoints = path.lookPoints;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 3; i++)
        {
            UnitPositions[i] = transform.position + HorizontalSpacing * transform.right * (n_ranks / 2 - i);
        }

        // Make this follow the path of the formation if moving is true
        if (moving)
        {
            // Code for movement goes here

            // Change to trailing behind a path
            Vector2 Pos2D = new Vector2(transform.position.x, transform.position.z);
            Vector3 targetPos = Waypoints[pointIndex];
            if (path.turnBoundaries[pointIndex].HasCrossedLine(Pos2D))
            {
                if (pointIndex < Waypoints.Length - 1)
                {
                    pointIndex++;
                }
            }
            else
            {
                TargetRotation = Quaternion.LookRotation(targetPos - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, Time.deltaTime * TurnSpeed);
                transform.position += transform.forward * Time.deltaTime * speed;
                for (int i = 0; i < UnitsInFile.Count; i++)
                {
                    if (UnitsInFile[i])
                    {
                        UnitsInFile[i].GetComponent<BaseUnitScript>().setTarget(UnitPositions[i]);
                    }
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 1f);
        Gizmos.color = Color.yellow;
        foreach (Vector3 pos in UnitPositions)
        {
            Gizmos.DrawSphere(pos, 0.6f);
        }
    }
}
