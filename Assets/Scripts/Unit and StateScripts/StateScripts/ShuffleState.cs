using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShuffleState : MonoBehaviour
{
    public float ShuffleTime = 1f;
    BaseUnitScript baseScript;
    Vector3 RandomDirection;
    public bool enteringState = true;
    float maxSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseScript = GetComponent<BaseUnitScript>();
        maxSpeed = baseScript.getMaxSpeed();
    }

    // Update is called once per frame
    void Update()
    {

    }

    Vector3 RandomVector()
    {
        float Angle = Random.Range(0.0f, 2 * Mathf.PI);
        Vector3 Out = Vector3.zero;
        Out.Set(Mathf.Cos(Angle), 0.0f, Mathf.Sin(Angle));
        return Out;
    }

    public void ExecuteState()
    {
        if (enteringState)
        {
            RandomDirection = RandomVector();
            enteringState = false;
        }

        baseScript.MoveInAnyDirection(maxSpeed * RandomDirection);
        ShuffleTime -= Time.deltaTime;
        if (ShuffleTime <= 0.0f)
        {
            baseScript.setState("Idle");
        }
    }
}
