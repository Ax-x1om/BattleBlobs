using System.IO;
using UnityEngine;

public class MovingState : MonoBehaviour
{
    BaseUnitScript bsu;
    public float vicinityRadius = 3f;
    bool stuck = false;
    float stucktimer = 0.0f;
    CapsuleCollider mainBody;
    Vector3 CollisionPoint = Vector3.zero;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainBody = GetComponent<CapsuleCollider>();
        bsu = GetComponent<BaseUnitScript>();
    }

    // Update is called once per frame
    void Update()
    {
        // Wow, such empty
    }

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
        bsu.n_raycasts = Mathf.FloorToInt((bsu.MaxRayCastAngle + Vector3.Angle(transform.forward, TargetDirection)) / bsu.RayCastAngle);
        // Does the central raycast

        // This one has the special check for if the target is in front of the obstacle
        if (Physics.Raycast(transform.position, TargetDirection, out ObstacleDetection, bsu.ObstacleDetectionRange, bsu.Avoid))
        {
            if (ObstacleDetection.distance > bsu.DistanceToTarget())
            {
                // Returns the vector if the target is in front of the obstacle
                return TargetDirection;
            }
        }
        else
        {
            return TargetDirection;
        }
        for (int i = 1; i <= bsu.n_raycasts; i++)
        {
            Vector3 rightVector = RotateByAngle(TargetDirection, bsu.dtheta * i);
            Vector3 leftVector = RotateByAngle(TargetDirection, -bsu.dtheta * i);
            if (Vector3.Angle(rightVector, transform.forward) < bsu.MaxRayCastAngle)
            {
                // Only does the raycast if the angle between them is less than MaxRayCastAngle
                // Meaning it's in the cone of vision
                if (!(Physics.Raycast(transform.position, rightVector, out ObstacleDetection, bsu.ObstacleDetectionRange, bsu.Avoid)))
                {
                    // Returns the vector if the raycast doesn't hit anything
                    return rightVector;
                }
                else
                {
                    if (i == bsu.n_raycasts)
                    {
                        // Returns the vector if the code fails to find a valid path
                        return rightVector;
                    }
                }
            }

            if (Vector3.Angle(leftVector, transform.forward) < bsu.MaxRayCastAngle)
            {
                // Only does the raycast if the raycast is within the cone of vision
                if (!(Physics.Raycast(transform.position, leftVector, out ObstacleDetection, bsu.ObstacleDetectionRange, bsu.Avoid)))
                {
                    // Returns the vector if the raycast doesn't hit anything
                    return leftVector;
                }
                else
                {
                    if (i == bsu.n_raycasts)
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

    public void ExecuteState()
    {
        // This is what would be in Update() if it wasn't a statescript

        Vector2 Pos2D = new Vector2(transform.position.x, transform.position.z);
        
        if (bsu.Waypoints.Length > 0)
        {
            // Will only execute the code if there is an actual path
            Vector3 targetPos = bsu.Waypoints[bsu.pointIndex];
            if (stuck)
            {
                // Moves the unit away from the collision
                bsu.MoveInAnyDirection(-bsu.maximumSpeed * CollisionPoint.normalized);
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
                Vector3 baseDirection = (targetPos - transform.position).normalized;

                if (bsu.isThereObstacle)
                {
                    // Changes the direction the unit will turn to based on the turn script
                    baseDirection = AvoidObstacles(baseDirection);
                }

                if (bsu.path.turnBoundaries[bsu.pointIndex].HasCrossedLine(Pos2D))
                {
                    if (bsu.pointIndex < bsu.Waypoints.Length - 1)
                    {
                        bsu.pointIndex++;
                    }
                    else
                    {
                        bsu.setState("Idle");
                    }
                }
                else
                {
                    // Make the unit turn to the direction we evaluated to be the best to avoid the obstacle

                    bsu.TurnToDirection(baseDirection);
                    bsu.MoveForward();
                }
            }
        }
    }
}
