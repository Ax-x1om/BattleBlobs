using System.ComponentModel;
using UnityEngine;

public class EnemyMovingState : MovingState
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        mainBody = GetComponent<CapsuleCollider>();
        baseScript = GetComponentInParent<BaseEnemyScript>();
        maxSpeed = baseScript.getMaxSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
