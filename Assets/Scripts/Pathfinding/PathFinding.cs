
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;


public class Pathfinding : MonoBehaviour
{
    readonly float uphillPenalty = 20f;
    readonly float downhillPenalty = -15f;
    readonly float cliffThreshold = 7.5f;

    A_Star_Grid grid;

    void Awake()
    {
        grid = GetComponent<A_Star_Grid>();
    }

    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSucess = false;
        bool nullNode = false;

        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);
        nullNode = (startNode == null | targetNode == null);

        // Creates a heap
        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        // Creates a hash set for fast searching
        HashSet<Node> closedSet = new HashSet<Node>();
        // adds startNode to heap
        openSet.Add(startNode);

        while (openSet.Count > 0 && !(nullNode))
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                sw.Stop();
                print("Path found: " + sw.ElapsedMilliseconds + " ms");
                pathSucess = true;
                break;
            }
            foreach (Node neighbour in grid.GetNeigbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                // Movement cost calculated based on distance, height and weight
                int NewMovementCostToNeighbour = currentNode.gCost + getDistance(currentNode, neighbour) + getHeightPenalty(currentNode, neighbour) + neighbour.movementPenalty;
                // Updates the gCost if the newmovementcost is lower
                if (NewMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = NewMovementCostToNeighbour;
                    neighbour.hCost = getDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                    else
                    {
                        openSet.UpdateItem(neighbour);
                    }
                }
            }
        }
        if (pathSucess)
        {
            // Retraces path
            waypoints = RetracePath(startNode, targetNode);
            pathSucess = waypoints.Length > 0;
        }
        callback(new PathResult(waypoints, pathSucess, request.callback));
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplyfyPath(path);
        // Reverses path because the path would be backwards initally
        Array.Reverse(waypoints);

        return waypoints;
    }

    Vector3[] SimplyfyPath(List<Node> path)
    {
        List <Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        // Only includes the nodes where the direction changes
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].GridX - path[i].GridX, path[i - 1].GridY - path[i].GridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        // Functions use arrays so it has to be an array
        return waypoints.ToArray();
    }
    int getHeightPenalty(Node nodeA, Node nodeB)
    {
        float heightA = nodeA.height;
        float heightB = nodeB.height;
        float heightDiff = heightB - heightA;
        float dist = 1;
        if (nodeA.worldPosition.x != nodeB.worldPosition.x && nodeA.worldPosition.z != nodeB.worldPosition.z)
        {
            dist = 1.4f;
        }
        // Checks if the gradient is too steep, so units don't go up or down cliffs
        if (Mathf.Abs(heightDiff) > cliffThreshold)
        {
            return Mathf.RoundToInt(Mathf.Infinity);
        }
        else if (heightDiff > 0)
        {
            // Applies uphill penalty
            return Mathf.RoundToInt(heightDiff * uphillPenalty/dist);
        }
        else
        {
            // Applies downhill penalty
            return Mathf.RoundToInt(heightDiff * downhillPenalty/dist);
        }
    }
    int getDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int distY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }
}

