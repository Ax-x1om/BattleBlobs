using UnityEngine;

public class EnemyPassiveDetection : PassiveDetection
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        baseScript = GetComponentInParent<BaseEnemyScript>();
        range = GetComponent<SphereCollider>();
        range.radius = baseScript.ObstacleDetectionRange;
        unit = transform.parent.gameObject;
    }
}
