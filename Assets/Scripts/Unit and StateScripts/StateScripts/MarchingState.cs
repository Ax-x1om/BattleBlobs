using UnityEngine;

public class MarchingState : MonoBehaviour
{
    BaseUnitScript baseScript;
    bool stuck = false;
    float stucktimer = 0.0f;
    CapsuleCollider mainBody;
    Vector3 CollisionPoint = Vector3.zero;
    float maxSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainBody = GetComponent<CapsuleCollider>();
        baseScript = GetComponent<BaseUnitScript>();
        maxSpeed = baseScript.getMaxSpeed();
    }

    private void OnCollisionEnter(Collision collision)
    {
        stuck = true;
        // Sets the collision point
        CollisionPoint = collision.contacts[0].point - transform.position;
        // Sets a random time for the unit to be stuck for
        stucktimer = Random.Range(0.8f, 1.2f);
    }

    public void ExecuteState()
    {
        if (stuck)
        {
            // Moves the unit away from the collision
            baseScript.MoveInAnyDirection(-maxSpeed * CollisionPoint.normalized);
            // Increments the time
            stucktimer -= Time.deltaTime;
            if (stucktimer < 0.0f)
            {
                // Sets stuck to false and the timer to zero when time runs out
                stucktimer = 0.0f;
                stuck = false;
            }
        }
        else
        {
            // Sets direction
            Vector3 baseDirection = (baseScript.TargetLocation - transform.position).normalized;
            // Turns to direction
            // Doesn't do obstacle avoidace checks because real soldiers don't do that, they just march
            baseScript.TurnToDirection(baseDirection);
            // scales down the speed if the unit is far away from the where it needs to be and scales it up if it's close
            // The speed is doubled so it can move on the outside turn without being left behind
            float marchspeed = 2 * Mathf.Clamp(baseScript.DistanceToTarget(), 0.0f, 2f * maxSpeed);
            baseScript.MoveInAnyDirection(transform.forward * marchspeed);

            // No code for getting out of this state because the formation will be the one responsible for that
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
