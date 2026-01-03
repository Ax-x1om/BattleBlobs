using UnityEngine;

public class EnemyFightingState : FightingState
{
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        baseScript = GetComponentInParent<BaseEnemyScript>();
        baseScript.DebugLog();
        mainBody = GetComponent<CapsuleCollider>();
        maxSpeed = baseScript.getMaxSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
