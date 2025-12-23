using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShuffleState : MonoBehaviour
{
    public float ShuffleTime = 1f;
    BaseUnitScript bsu;
    Vector3 RandomDirection;
    public bool enteringState = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bsu = GetComponent<BaseUnitScript>();
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

        bsu.MoveInAnyDirection(bsu.maximumSpeed * RandomDirection);
        ShuffleTime -= Time.deltaTime;
        if (ShuffleTime <= 0.0f)
        {
            bsu.setState("Idle");
        }
    }
}
