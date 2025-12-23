using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Formation : MonoBehaviour
{
    public List<GameObject> FilesInFormation = new List<GameObject>();
    // This is the spacing between the ranks and files respectively
    // Make these not public and get acessor methods
    readonly float xSpacing = 5f;
    readonly float zSpacing = 5f;
    readonly float TurnDst = 5f;
    readonly float speed = 2f;
    readonly float TurnSpeed = 0.2f;
    Quaternion TargetRotation;
    int pointIndex = 0;
    // This is the initial direction the formation will have
    public Vector3 initialDirection;

    // Path of points that it will pathfind to
    private Vector3[] Waypoints;
    Directions path;
    bool moving = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnitSelectionManager.Instance.Formations.Add(gameObject);
    }

    private void OnDestroy()
    {
        foreach (GameObject file in FilesInFormation)
        {
            // Clears and destroys the list of files
            Destroy(file);
        }
        FilesInFormation.Clear();
        UnitSelectionManager.Instance.Formations.Remove(gameObject);
    }

    public float ZSpacing()
    {
        return zSpacing;
    }

    public float XSpacing()
    {
        return xSpacing;
    }
    public void AddFile(GameObject file)
    {
        FilesInFormation.Add(file);
    }
    public void setPath(Vector3[] points)
    {
        Waypoints = points;
        path = new Directions(Waypoints, transform.position, TurnDst);
        moving = true;
        pointIndex = 0;
        foreach (GameObject file in FilesInFormation)
        {
            file.GetComponent<File>().SetPath(path);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            // Code for movement goes here
            Vector2 Pos2D = new Vector2(transform.position.x, transform.position.z);
            Vector3 targetPos = Waypoints[pointIndex];
            if (path.turnBoundaries[pointIndex].HasCrossedLine(Pos2D))
            {
                if (pointIndex < Waypoints.Length - 1)
                {
                    pointIndex++;
                }
                else
                {
                    moving = false;
                    foreach (GameObject file in FilesInFormation)
                    {
                        file.GetComponent<File>().moving = false;
                        file.GetComponent<File>().StopMarching();
                    }
                }
            }
            else
            {
                TargetRotation = Quaternion.LookRotation(targetPos - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, Time.deltaTime * TurnSpeed);
                transform.position += transform.forward * Time.deltaTime * speed;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 2f);
    }
}
