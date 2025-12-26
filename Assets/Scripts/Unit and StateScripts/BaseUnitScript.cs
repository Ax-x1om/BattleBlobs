using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BaseUnitScript : MonoBehaviour
{
    // Maybe make accessor methods for the public variables

    // Variables for hovering
    readonly float rideHeight = 1.8f;
    readonly float springStrength = 30f;
    readonly float springDampening = 25f;
    readonly float springLength = 2.5f;
    // Variables for moving and turning
    public float maximumSpeed = 5f;
    readonly float CounterForce = 3f;
    readonly float RotationSpeed = 12f;
    readonly float RotationDamping = 2f;
    readonly float maximumResistiveForce = 2f;
    readonly float maximumForwardForce = 4f;
    readonly float TurnDst = 2f;
    Vector3 CounterMovement = Vector3.zero;
    public Vector3 TargetLocation = Vector3.zero;
    public Vector3 TargetForwardTransform = Vector3.forward;
    // Variables for obstacle avoidance
    public bool isThereObstacle = false;
    readonly public float RayCastAngle = 5f;
    readonly public float MaxRayCastAngle = 120f;
    readonly public float ObstacleDetectionRange = 12f;
    public float dtheta;
    public int n_raycasts;

    bool[,] mudmap;

    float terrainSize;
    Vector2 topLeft;

    public Vector3[] Waypoints;
    public Directions path;
    public int pointIndex;

    public string type = "Soldier";
    // Intial state
    public string state = "Idle";
    LayerMask floor;
    public LayerMask Avoid;
    public LayerMask enemy;
    Rigidbody m_rigidbody;
    // Getting references for all the state scripts
    MovingState movingstate;
    FormingUpState formingupstate;
    MarchingState marchingstate;
    ShuffleState shufflestate;
    AtEaseState ateasestate;
    FightingState fightingstate;
    // Variables to do with attacking and health
    public float timeBetweenAttacks;
    public float attackStrength;
    public float attackDamage;
    public float Health;
    public float attackRange;
    public float stunDuration;
    float stunTimer = 0;
    bool stunned = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        // Getting statescripts
        movingstate = GetComponent<MovingState>();
        formingupstate = GetComponent<FormingUpState>();
        marchingstate = GetComponent<MarchingState>();
        shufflestate = GetComponent<ShuffleState>();
        ateasestate = GetComponent<AtEaseState>();
        fightingstate = GetComponent<FightingState>();

        
        terrainSize = TerrainGenerator.Instance.terrainScale;
        topLeft = TerrainGenerator.Instance.topLeftcorner;
        mudmap = TerrainGenerator.Instance.mudmap;

        // Getting Layermasks
        floor = LayerMask.GetMask("Ground");
        Avoid = LayerMask.GetMask("Object") | LayerMask.GetMask("PlayerTeam");
        enemy = LayerMask.GetMask("EnemyTeam");
        // Adds unit to the unitList
        UnitSelectionManager.Instance.allUnitsList.Add(gameObject);
        // Used in obstacle avoidance
        dtheta = RayCastAngle * Mathf.Deg2Rad;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOnGround())
        {
            Hover();
            addResistiveForces();
            // Probably add a method for the units to move away from each other if they're too close while idle or moving
            // When moving, there should be an added vector to the direction vector to cause it to move away
            // This requires an empty child gameobject
            if (stunned)
            {
                whenStunned();
            }
            else
            {
                if (state == "Moving")
                {
                    movingstate.ExecuteState();
                }
                else if (state == "Forming Up")
                {
                    formingupstate.ExecuteState();
                }
                else if (state == "Marching")
                {
                    marchingstate.ExecuteState();
                }
                else if (state == "Shuffling")
                {
                    shufflestate.ExecuteState();
                }
                else if (state == "At Ease")
                {
                    ateasestate.ExecuteState();
                }
                else if (state == "Fighting")
                {
                    fightingstate.ExecuteState();
                }
            }
        }
        // This is here because Unity is a buggy peice of trash that refuses to do what it's told
        // *Somehow* the unit still rotates on the X and Z axes despite the rigidbody specifically telling it not to
        Vector3 c_rotation = m_rigidbody.rotation.eulerAngles;
        c_rotation.x = 0;
        c_rotation.z = 0;
        m_rigidbody.rotation = Quaternion.Euler(c_rotation);
        m_rigidbody.angularVelocity = new Vector3(0f, m_rigidbody.angularVelocity.y, 0f);
    }

    private void OnDestroy()
    {
        UnitSelectionManager.Instance.allUnitsList.Remove(gameObject);
    }

    bool isOnGround()
    {
        // Does a short raycast to the ground to see if it's on the ground
        RaycastHit check;
        return Physics.Raycast(transform.position, Vector3.down, out check, springLength, floor);
    }

    void Hover()
    {
        // Makes the unit hover above the ground by applying a force to make it levitate
        RaycastHit spring;
        if (Physics.Raycast(transform.position, Vector3.down, out spring, springLength, floor))
        {
            float levitationError = rideHeight - spring.distance;
            float verticalVelocity = m_rigidbody.linearVelocity.y;
            // Adds an upwards force to the capsule while dampening the oscillations caused.
            m_rigidbody.AddForce(Vector3.up * (levitationError * springStrength - verticalVelocity * springDampening), ForceMode.Acceleration);
        }
    }

    public float DistanceToTarget()
    {
        Vector3 DirectionTo = TargetLocation - transform.position;
        DirectionTo.y = 0;
        return DirectionTo.magnitude;
    }
    public void TurnToDirection(Vector3 Direction)
    {

        // Normalises the direction vector
        Direction = Direction.normalized;
        Direction.y = 0;
        // Rotates us in the right direction
        Vector3 rotationAxis = Vector3.Cross(transform.forward, Direction);
        // Makes it so the rotation gets smaller when the angles are small, while staying the same when the angles are large
        if (Vector3.Angle(transform.forward, Direction) > 90f)
        {
            rotationAxis = rotationAxis.normalized;
        }
        // Spin
        m_rigidbody.AddTorque(rotationAxis * RotationSpeed - m_rigidbody.angularVelocity * RotationDamping);
    }

    public void MoveForward()
    {
        // Applies a force to move the unit forward
        Vector3 TargetVelocity = transform.forward * maximumSpeed;
        Vector3 CurrentVelocity = m_rigidbody.linearVelocity;
        // Removes the vertical component of velocity
        CurrentVelocity.y = 0;
        TargetVelocity.y = 0;
        // Force gets smaller as the Target and Current Velocities get closer
        Vector3 MovementForce = Vector3.ClampMagnitude(TargetVelocity - CurrentVelocity, maximumForwardForce);
        if (OnMud())
        {
            // Slows down unit if it's on mud
            MovementForce *= 0.5f;
        }
        m_rigidbody.AddForce(MovementForce, ForceMode.Acceleration);
    }

    public void MoveInAnyDirection(Vector3 TargetVelocity)
    {
        // Applies a force to move the unit forward
        Vector3 CurrentVelocity = m_rigidbody.linearVelocity;
        // Removes the vertical component of velocity
        CurrentVelocity.y = 0;
        TargetVelocity.y = 0;
        // Force gets smaller as the Target and Current Velocities get closer
        Vector3 MovementForce = Vector3.ClampMagnitude(TargetVelocity - CurrentVelocity, maximumForwardForce);
        m_rigidbody.AddForce(MovementForce, ForceMode.Acceleration);
    }

    public void setPath(Vector3[] points)
    {
        Waypoints = points;
        path = new Directions(Waypoints, transform.position, TurnDst);
        pointIndex = 0;
    }

    // Adds resistive forces so the unit can stop and not accelerate forever
    void addResistiveForces()
    {
        // Sets the counter force to the the negative of the movement
        CounterMovement.Set(m_rigidbody.linearVelocity.x, 0f, m_rigidbody.linearVelocity.z);
        // Makes it to the forces don't exceed the maximum
        CounterMovement = Vector3.ClampMagnitude(-CounterMovement * CounterForce, maximumResistiveForce);
        // Clamp magnitude
        m_rigidbody.AddForce(CounterMovement, ForceMode.Acceleration);
    }

    public void Attack(Collider other)
    {
        // Attacks by pushing their opponent away
        if (other.attachedRigidbody)
        {
            // Stuns the opponent, stopping the levitation for a bit, allowing it to tumble away
            // Change BaseUnitScript to whatever the enemy equivalent is
            other.gameObject.GetComponent<BaseUnitScript>().stun();
            //Vector3 random = RandomVector(attackStrength * 0.3f, true);
            other.attachedRigidbody.AddForce(AwayVector(other.attachedRigidbody.position) * attackStrength, ForceMode.Impulse);
            // Does damage
            other.gameObject.GetComponent<BaseUnitScript>().ModifyHealth(-attackDamage);
        }
    }

    Vector3 AwayVector(Vector3 opponentPosition)
    {
        // Makes it so any attack pushes the enemy away
        Vector3 Relative_Location = opponentPosition - transform.position;
        return Relative_Location.normalized;
    }

    public void ModifyHealth(float amount)
    {
        Health += amount;
        // Destroys object if health dips below 0
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }

    void stun()
    {
        stunned = true;
        stunTimer = stunDuration;
    }

    void whenStunned()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer < 0)
        {
            stunTimer = 0;
            stunned = false;
        }
    }
    bool OnMud()
    {
        // returns true if the unit is over a cell that is mud, returns false otherwise
        float X = transform.position.x;
        float Z = transform.position.z;
        int x = Mathf.RoundToInt(X / terrainSize - topLeft.x);
        int z = Mathf.RoundToInt(topLeft.y - Z / terrainSize);
        return mudmap[x, z];
    }

    public string getState()
    {
        return state;
    }

    public string getType()
    {
        return type;
    }
    public void setState(string newstate)
    {
        state = newstate;
        if (newstate == "Shuffling")
        {
            shufflestate.enteringState = true;
            shufflestate.ShuffleTime = 1f;
        }
        else if (newstate == "Forming Up")
        {
            formingupstate.AtLocation = false;
        }
    }

    public void setTarget(Vector3 newtarget)
    {
        TargetLocation = newtarget;
    }
}
