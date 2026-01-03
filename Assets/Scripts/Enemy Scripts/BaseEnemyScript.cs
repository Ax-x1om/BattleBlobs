using UnityEngine;

public class BaseEnemyScript : BaseUnitScript
{
    // Inherits from BaseUnitScript to massively decrease size

    MovingState movingstate;
    FightingState fightingstate;

    Rigidbody m_rigidbody;

    public LayerMask Avoid;
    public LayerMask enemy;

    public float timeBetweenAttacks;
    public float attackStrength;
    public float attackDamage;
    public float Health;
    public float attackRange;
    public float stunDuration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        movingstate = GetComponent<MovingState>();
        fightingstate = GetComponent<FightingState>();

        // Getting Layermasks
        Avoid = LayerMask.GetMask("Object") | LayerMask.GetMask("EnemyTeam") | LayerMask.GetMask("EnemyTeamAvoid");
        enemy = LayerMask.GetMask("PlayerTeam");
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

        // Removes unit if it falls off the map
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    public override void Attack(Collider other)
    {
        // Overrides the function in BaseUnitScript as it has to trigger a different function in a different script

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
}
