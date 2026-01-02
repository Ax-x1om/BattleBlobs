using Unity.VisualScripting;
using UnityEngine;

public class Attack : MonoBehaviour
{
    BaseUnitScript bsu;
    float TBA;
    float AttackTimer;
    float timeVariation;
    SphereCollider range;
    GameObject unit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        bsu = GetComponentInParent<BaseUnitScript>();
        Debug.Log("BSU is now set");
        TBA = bsu.timeBetweenAttacks;
        timeVariation = TBA * 0.2f;
        AttackTimer = TBA + Random.Range(-timeVariation, timeVariation);
        range = GetComponent<SphereCollider>();
        range.radius = bsu.attackRange;
        unit = transform.parent.gameObject;
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
        if (other.gameObject != unit)
        {
            // Prevents it from detecting and attacking itself
            if (AttackTimer <= 0 && bsu.getState() == "Fighting")
            {
                bsu.Attack(other);
            }
        }
    }
}
