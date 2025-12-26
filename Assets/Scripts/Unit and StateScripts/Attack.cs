using UnityEngine;

public class Attack : MonoBehaviour
{
    BaseUnitScript bsu;
    float TBA;
    float AttackTimer;
    float timeVariation;
    SphereCollider range;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bsu = GetComponentInParent<BaseUnitScript>();
        TBA = bsu.timeBetweenAttacks;
        timeVariation = TBA * 0.2f;
        AttackTimer = TBA + Random.Range(-timeVariation, timeVariation);
        range = GetComponent<SphereCollider>();
        range.radius = bsu.attackRange;
    }

    // Update is called once per frame
    void Update()
    {
        // Timer for attacks
        if (AttackTimer <= 0 && bsu.getState() == "Fighting")
        {
            AttackTimer = TBA + Random.Range(-timeVariation, timeVariation);
        }
        else
        {
            AttackTimer -= Time.deltaTime;
        }
        // Prevents collider from being left behind
        transform.position = bsu.transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        // Triggers the attack method in Unit
        if (AttackTimer <= 0 && bsu.getState() == "Fighting")
        {
            bsu.Attack(other);
        }
    }
}
