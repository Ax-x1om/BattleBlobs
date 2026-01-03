using UnityEngine;

public class EnemyPassiveDetection : PassiveDetection
{
    BaseEnemyScript bsu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bsu = GetComponentInParent<BaseEnemyScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
