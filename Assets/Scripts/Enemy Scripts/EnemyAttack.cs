using System.ComponentModel;
using UnityEngine;

public class EnemyAttack : Attack
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
