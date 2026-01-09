using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class GameTextManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMP_Text soldiersLeft;
    public TMP_Text brutesLeft;
    public TMP_Text survivingEnemyUnits;
    public TMP_Text survivingFriendlyUnits;

    public TMP_Text youWin;
    public TMP_Text youLose;
    public Button playAgain;
    public Button quit;

    void Start()
    {
        youLose.enabled = false;
        youWin.enabled = false;
        playAgain.gameObject.SetActive(false);
        quit.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        soldiersLeft.enabled = UnitSpawningManager.Instance.Active;
        brutesLeft.enabled = UnitSpawningManager.Instance.Active;
        survivingEnemyUnits.enabled = !(UnitSpawningManager.Instance.Active);
        survivingFriendlyUnits.enabled = !(UnitSpawningManager.Instance.Active);
        if (!(UnitSpawningManager.Instance.Active))
        {
            // Only does this when UnitSelectionManager is active to prevent a NullReferenceExecption
            youLose.enabled = UnitSelectionManager.Instance.playerLost;
            youWin.enabled = EnemyManager.Instance.enemyLost;
            playAgain.gameObject.SetActive((UnitSelectionManager.Instance.playerLost | EnemyManager.Instance.enemyLost));
            quit.gameObject.SetActive((UnitSelectionManager.Instance.playerLost | EnemyManager.Instance.enemyLost));
        }
       

        if (soldiersLeft.enabled)
        {
            // Only checks the text if the text is actually enabled
            soldiersLeft.text = "Soldiers left to place: " + UnitSpawningManager.Instance.num_soldiers;
            brutesLeft.text = "Brutes left to place: " + UnitSpawningManager.Instance.num_brutes;
        }
        else
        {
            survivingEnemyUnits.text = "Enemies left: " + EnemyManager.Instance.allEnemiesList.Count;
            survivingFriendlyUnits.text = "Units left: " + UnitSelectionManager.Instance.allUnitsList.Count;
        }
    }
}
