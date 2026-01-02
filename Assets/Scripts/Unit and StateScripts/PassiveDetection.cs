using UnityEngine;

public class PassiveDetectionScript : MonoBehaviour
{
    BaseUnitScript bsu;
    SphereCollider range;
    GameObject unit;
    GameObject avoidanceRange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        bsu = GetComponentInParent<BaseUnitScript>();
        range = GetComponent<SphereCollider>();
        // for some reason, this isn't reading the correct ObstacleDetectionRange
        range.radius = bsu.ObstacleDetectionRange;
        unit = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // So the collider doesn't get left behind
        transform.position = bsu.transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        // Toggle the variable to true if there is an obstacle
        // Does this so we don't do the expensive raycast thing if there's nothing nearby
        if (other.gameObject != unit)
        {
            // Does this check so it doesn't detect itself

            if (other.gameObject.layer == bsu.enemy)
            {
                // Sets the state to fighting if the unit is an enemy and the unit is not in formation
                if (bsu.getState() != "Forming Up" & bsu.getState() != "At Ease" & bsu.getState() != "Marching")
                {
                    bsu.TargetLocation = other.gameObject.transform.position;
                    bsu.setState("Fighting");
                }
            }
            else
            {
                // makes the unit attempt to avoid obstacles
                bsu.isThereObstacle = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == bsu.enemy)
        {
            // Sets the state to fighting if the unit is an enemy and the unit is not in formation
            if (bsu.getState() != "Forming Up" & bsu.getState() != "At Ease" & bsu.getState() != "Marching")
            {
                bsu.TargetLocation = other.gameObject.transform.position;
                bsu.setState("Idle");
            }
        }
        else
        {
            // makes the unit attempt to avoid obstacles
            bsu.isThereObstacle = false;
        }
    }
}
