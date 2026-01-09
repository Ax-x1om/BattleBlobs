using UnityEngine;

public class PassiveDetection : MonoBehaviour
{
    protected BaseUnitScript baseScript;
    protected SphereCollider range;
    protected GameObject unit;
    protected GameObject avoidanceRange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        baseScript = GetComponentInParent<BaseUnitScript>();
        range = GetComponent<SphereCollider>();
        range.radius = baseScript.ObstacleDetectionRange;
        unit = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // So the collider doesn't get left behind
        transform.position = baseScript.transform.position;
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

    private void OnTriggerStay(Collider other)
    {
        // Toggle the variable to true if there is an obstacle
        // Does this so we don't do the expensive raycast thing if there's nothing nearby
        if (other.gameObject != unit)
        {
            // Does this check so it doesn't detect itself
            if (LayerIsInLayerMask(baseScript.enemy, other.gameObject.layer))
            {
                // Sets the state to fighting if the unit is an enemy and the unit is not in formation
                if (baseScript.getState() != "Forming Up" && baseScript.getState() != "At Ease" && baseScript.getState() != "Marching")
                {
                    baseScript.TargetLocation = other.gameObject.transform.position;
                    baseScript.setState("Fighting");
                }
            }
            else
            {
                // makes the unit attempt to avoid obstacles
                baseScript.isThereObstacle = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == baseScript.enemy)
        {
            // Sets the state to fighting if the unit is an enemy and the unit is not in formation
            if (baseScript.getState() != "Forming Up" & baseScript.getState() != "At Ease" & baseScript.getState() != "Marching")
            {
                baseScript.TargetLocation = other.gameObject.transform.position;
                baseScript.setState("Idle");
            }
        }
        else
        {
            // makes the unit attempt to avoid obstacles
            baseScript.isThereObstacle = false;
        }
    }
}
