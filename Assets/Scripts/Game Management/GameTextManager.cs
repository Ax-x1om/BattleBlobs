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

    public TMP_Text youWin;
    public TMP_Text youLose;
    public Button playAgain;
    public Button quit;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        unitsLeft.enabled = UnitSpawningManager.Instance.Active;
        survivingEnemyUnits.enabled = !(UnitSpawningManager.Instance.Active);
        survivingFriendlyUnits.enabled = !(UnitSpawningManager.Instance.Active);

        youLose.enabled = UnitSelectionManager.Instance.playerLost;
        youWin.enabled = EnemyManager.Instance.enemyLost;

        playAgain.enabled = (UnitSelectionManager.Instance.playerLost | EnemyManager.Instance.enemyLost);
        quit.enabled = (UnitSelectionManager.Instance.playerLost | EnemyManager.Instance.enemyLost);

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
