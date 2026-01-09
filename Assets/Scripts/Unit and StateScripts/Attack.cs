using Unity.VisualScripting;
using UnityEngine;

public class Attack : MonoBehaviour
{
    protected BaseUnitScript baseScript;
    protected float TBA;
    protected float AttackTimer;
    protected float timeVariation;
    protected SphereCollider range;
    protected GameObject unit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Awake()
    {
        baseScript = GetComponentInParent<BaseUnitScript>();
        TBA = baseScript.timeBetweenAttacks;
        timeVariation = TBA * 0.2f;
        AttackTimer = TBA + Random.Range(-timeVariation, timeVariation);
        range = GetComponent<SphereCollider>();
        range.radius = baseScript.attackRange;
        unit = transform.parent.gameObject;
    }

    // Update is called once per frame
    protected void Update()
    {
        // Timer for attacks
        if (AttackTimer <= 0 && baseScript.getState() == "Fighting")
        {
            AttackTimer = TBA + Random.Range(-timeVariation, timeVariation);
        }
        else
        {
            AttackTimer -= Time.deltaTime;
        }
        // Prevents collider from being left behind
        transform.position = baseScript.transform.position;
    }

    protected void OnTriggerStay(Collider other)
    {
        // Triggers the attack method in Unit
        if (other.gameObject != unit)
        {
            // Prevents it from detecting and attacking itself
            if (AttackTimer <= 0 && baseScript.getState() == "Fighting")
            {
                baseScript.Attack(other);
            }
        }
    }
}
