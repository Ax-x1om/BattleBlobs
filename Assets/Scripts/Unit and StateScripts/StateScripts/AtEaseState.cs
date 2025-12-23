using UnityEngine;

public class AtEaseState : MonoBehaviour
{
    BaseUnitScript bsu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bsu = GetComponent<BaseUnitScript>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ExecuteState()
    {
        Debug.DrawRay(transform.position, bsu.TargetForwardTransform * 5f, Color.red, 0.0f, false);
        bsu.TurnToDirection(bsu.TargetForwardTransform - transform.position);
    }
}
