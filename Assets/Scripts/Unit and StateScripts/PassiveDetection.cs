using UnityEngine;

public class PassiveDetectionScript : MonoBehaviour
{
    BaseUnitScript bsu;
    SphereCollider range;
    GameObject unit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bsu = GetComponentInParent<BaseUnitScript>();
        range = GetComponent<SphereCollider>();
        // for some reason, this isn't reading the correct ObstacleDetectionRange
        range.radius = bsu.ObstacleDetectionRange;
        unit = this.transform.parent.gameObject;
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
            bsu.isThereObstacle = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        bsu.isThereObstacle = false;
    }
}
