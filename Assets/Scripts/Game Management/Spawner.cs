using UnityEngine;

public class Spawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject unit;

    public void SpawnUnit()
    {
        GameObject NewUnit = Instantiate(unit, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
