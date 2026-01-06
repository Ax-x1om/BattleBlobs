using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class GameTextManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMP_Text unitsLeft;
    public TMP_Text survivingEnemyUnits;
    public TMP_Text survivingFriendlyUnits;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        unitsLeft.enabled = UnitSpawningManager.Instance.Active;
        survivingEnemyUnits.enabled = !(UnitSpawningManager.Instance.Active);
        survivingFriendlyUnits.enabled = !(UnitSpawningManager.Instance.Active);
        if (unitsLeft.enabled)
        {
            // Only checks the text if the text is actually enabled
            unitsLeft.text = "Units left to place: " + UnitSpawningManager.Instance.num_soldiers;
        }
        else
        {
            survivingEnemyUnits.text = "Enemies left: " + EnemyManager.Instance.allEnemiesList.Count;
            survivingFriendlyUnits.text = "Units left: " + UnitSelectionManager.Instance.allUnitsList.Count;
        }
    }
}
