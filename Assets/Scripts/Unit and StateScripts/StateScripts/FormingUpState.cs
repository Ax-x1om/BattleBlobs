using UnityEngine;

public class FormingUpState : MonoBehaviour
{
    BaseUnitScript baseScript;
    float vicinityRadius = 0.2f;
    float slowdownRadius = 4.0f;
    bool stuck = false;
    float stucktimer = 0.0f;
    CapsuleCollider mainBody;
    Vector3 CollisionPoint = Vector3.zero;
    public bool AtLocation = false;
    float maxSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseScript = GetComponent<BaseUnitScript>();
        mainBody = GetComponent<CapsuleCollider>();
        maxSpeed = baseScript.getMaxSpeed();
    }

    // All of this stuff is pretty much the same as Moving State
    Vector3 RotateByAngle(Vector3 InitalVector, float angle)
    {
        // Positive Angles are clockwise
        // Angles must be given in radians
        // Based on a rotation matrix
        Vector3 Output = Vector3.zero;
        Output.x = Mathf.Cos(angle) * InitalVector.x + Mathf.Sin(angle) * InitalVector.z;
        Output.z = Mathf.Cos(angle) * InitalVector.z - Mathf.Sin(angle) * InitalVector.x;
        return Output;
    }

    Vector3 AvoidObstacles(Vector3 TargetDirection)
    {
        RaycastHit ObstacleDetection;
        TargetDirection = TargetDirection.normalized;
        // Calculates the maximum amount of raycasts at one side to do
        baseScript.n_raycasts = Mathf.FloorToInt((baseScript.MaxRayCastAngle + Vector3.Angle(transform.forward, TargetDirection)) / baseScript.RayCastAngle);
        // Does the central raycast

        // This one has the special check for if the target is in front of the obstacle
        if (Physics.Raycast(transform.position, TargetDirection, out ObstacleDetection, baseScript.ObstacleDetectionRange, baseScript.Avoid))
        {
            if (ObstacleDetection.distance > baseScript.DistanceToTarget())
            {
                // Returns the vector if the target is in front of the obstacle
                return TargetDirection;
            }
        }
        else
        {
            return TargetDirection;
        }
        for (int i = 1; i <= baseScript.n_raycasts; i++)
        {
            Vector3 rightVector = RotateByAngle(TargetDirection, baseScript.dtheta * i);
            Vector3 leftVector = RotateByAngle(TargetDirection, -baseScript.dtheta * i);
            if (Vector3.Angle(rightVector, transform.forward) < baseScript.MaxRayCastAngle)
            {
                // Only does the raycast if the angle between them is less than MaxRayCastAngle
                // Meaning it's in the cone of vision
                if (!(Physics.Raycast(transform.position, rightVector, out ObstacleDetection, baseScript.ObstacleDetectionRange, baseScript.Avoid)))
                {
                    // Returns the vector if the raycast doesn't hit anything
                    return rightVector;
                }
                else
                {
                    if (i == baseScript.n_raycasts)
                    {
                        // Returns the vector if the code fails to find a valid path
                        return rightVector;
                    }
                }
            }

            if (Vector3.Angle(leftVector, transform.forward) < baseScript.MaxRayCastAngle)
            {
                // Only does the raycast if the raycast is within the cone of vision
                if (!(Physics.Raycast(transform.position, leftVector, out ObstacleDetection, baseScript.ObstacleDetectionRange, baseScript.Avoid)))
                {
                    // Returns the vector if the raycast doesn't hit anything
                    return leftVector;
                }
                else
                {
                    if (i == baseScript.n_raycasts)
                    {
                        // Returns the vector if the code fails to find a valid path
                        return leftVector;
                    }
                }
            }
        }
        // returns the inital value of the vector if somehow the code fails to find a valid route
        // This code shouldn't run but Visual Studio throws a hissy fit if I don't put this here
        return TargetDirection;
    }

    private void OnCollisionEnter(Collision collision)
    {
        stuck = true;
        // Sets the collision point
        CollisionPoint = collision.contacts[0].point - transform.position;
        // Sets a random time for the unit to be stuck for
        stucktimer = Random.Range(0.6f, 0.9f);
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void ExecuteState()
    {
        if (!AtLocation)
        {
            if (baseScript.DistanceToTarget() <= vicinityRadius)
            {
                // Sets the AtLocation to be true if it is at the location
                AtLocation = true;
            }
            else
            {
                // MovingScript Stuff
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
                    // Executes the code if the unit is *not* stuck
                    if (baseScript.DistanceToTarget() > slowdownRadius)
                    {
                        // Does the movement normally if the unit is far away from the point
                        Vector3 baseDirection = (baseScript.TargetLocation - transform.position).normalized;

                        if (baseScript.isThereObstacle)
                        {
                            // Changes the direction the unit will turn to based on the turn script
                            baseDirection = AvoidObstacles(baseDirection);
                        }

                        // Make the unit turn to the direction we evaluated to be the best to avoid the obstacle
                        baseScript.TurnToDirection(baseDirection);
                        // Moves forward
                        baseScript.MoveForward();
                    }
                    else
                    {
                        // Makes the unit slow down when it's close to the point
                        Vector3 baseDirection = (baseScript.TargetLocation - transform.position).normalized;
                        baseScript.TurnToDirection(baseDirection, 5f);
                        baseScript.MoveInAnyDirection(transform.forward * maxSpeed * baseScript.DistanceToTarget() / slowdownRadius);
                    }
                }
            }
        }
        else
        {
            baseScript.TurnToDirection(baseScript.TargetForwardTransform - transform.position);
            if (Vector3.Angle(transform.forward, baseScript.TargetForwardTransform) < 2f)
            {
                baseScript.setState("At Ease");
            }
        }
    }
}
