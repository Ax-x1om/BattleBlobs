using UnityEngine;

public class BaseEnemyScript : BaseUnitScript
{
    // Inherits from BaseUnitScript to massively decrease size

    EnemyMovingState enemymovingstate;
    EnemyFightingState enemyfightingstate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        enemymovingstate = GetComponent<EnemyMovingState>();
        enemyfightingstate = GetComponent<EnemyFightingState>();

        // Getting Layermasks
        floor = LayerMask.GetMask("Ground");
        Avoid = LayerMask.GetMask("Object") | LayerMask.GetMask("EnemyTeam") | LayerMask.GetMask("EnemyTeamAvoid");
        enemy = LayerMask.GetMask("PlayerTeam");

        terrainSize = TerrainGenerator.Instance.terrainScale;
        topLeft = TerrainGenerator.Instance.topLeftcorner;
        mudmap = TerrainGenerator.Instance.mudmap;

        dtheta = RayCastAngle * Mathf.Deg2Rad;
    }

    // Update is called once per frame
    protected override void Update()
    {
        Debug.Log(name + " update is working");
        Debug.Log(name + "'s onGround() test: " + base.isOnGround());
        if (base.isOnGround())
        {
            Debug.Log(name + " is on Ground");
            base.Hover();
            base.addResistiveForces();
            // Probably add a method for the units to move away from each other if they're too close while idle or moving
            // When moving, there should be an added vector to the direction vector to cause it to move away
            // This requires an empty child gameobject
            if (stunned)
            {
                base.whenStunned();
            }
            else
            {
                Debug.Log(name + " is not Stunned");
                if (state == "Moving")
                {
                    enemymovingstate.ExecuteState();
                }
                else if (state == "Fighting")
                {
                    Debug.Log(name + " should be fighting");
                    enemyfightingstate.ExecuteState();
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

        // Removes unit if it falls off the map
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    public override void Attack(Collider other)
    {
        // Overrides the function in BaseUnitScript as it has to trigger a different function in a different script
        Debug.Log(name + " has attacked");
        // Attacks by pushing their opponent away
        if (other.attachedRigidbody)
        {
            // Stuns the opponent, stopping the levitation for a bit, allowing it to tumble away
            // Change BaseUnitScript to whatever the enemy equivalent is
            other.gameObject.GetComponent<BaseUnitScript>().stun();
            //Vector3 random = RandomVector(attackStrength * 0.3f, true);
            other.attachedRigidbody.AddForce(base.AwayVector(other.attachedRigidbody.position) * attackStrength, ForceMode.Impulse);
            // Does damage
            other.gameObject.GetComponent<BaseUnitScript>().ModifyHealth(-attackDamage);
        }
    }
}
