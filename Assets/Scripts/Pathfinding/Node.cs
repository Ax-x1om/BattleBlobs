using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector3 worldPosition;
    public bool walkable;
    public int gCost;
    public int hCost;
    public float height;

    public int GridX;
    public int GridY;
    public Node parent;
    int heapIndex;
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, float _height)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        GridX = _gridX;
        GridY = _gridY;
        height = _height;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }
    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
