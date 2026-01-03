using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyAttack : Attack
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        baseScript = GetComponentInParent<BaseEnemyScript>();
        Debug.Log("BSU is now set");
        TBA = baseScript.timeBetweenAttacks;
        timeVariation = TBA * 0.2f;
        AttackTimer = TBA + Random.Range(-timeVariation, timeVariation);
        range = GetComponent<SphereCollider>();
        range.radius = baseScript.attackRange;
        unit = transform.parent.gameObject;
    }

    // Update is called once per frame
}
