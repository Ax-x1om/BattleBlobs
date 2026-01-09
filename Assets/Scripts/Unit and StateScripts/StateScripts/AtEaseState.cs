using UnityEngine;

public class AtEaseState : MonoBehaviour
{
    BaseUnitScript baseScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseScript = GetComponent<BaseUnitScript>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ExecuteState()
    {
        baseScript.TurnToDirection(baseScript.TargetForwardTransform - transform.position);
    }
}
