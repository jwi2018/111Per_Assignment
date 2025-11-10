using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private UIController uiController;
    private bool gameEnded = false;

    protected override void Awake()
    {
        base.Awake();

        if (this != Instance)
        {
            return;
        }

        GameObject uiObj = GameObject.FindGameObjectWithTag("UIController");
        if (uiObj != null)
        {
            uiController = uiObj.GetComponent<UIController>();
        }
    }

    void Update()
    {
        if (gameEnded) return;

        CheckEndCondition();
    }

    public void CheckEndCondition()
    {
        int playerHP = PlayerManager.Instance.GetPlayerData().CurrentHealth;
        int enemyHP = PlayerManager.Instance.GetEnemyData().CurrentHealth;

        if (playerHP <= 0 || enemyHP <= 0)
        {
            gameEnded = true;

            bool playerWin = playerHP > 0;
            uiController?.SetResult(playerWin);
            return;
        }

        if (uiController != null && uiController.CurrentTime <= 0)
        {
            gameEnded = true;

            bool playerWin = playerHP >= enemyHP;
            uiController.SetResult(playerWin);
            return;
        }
    }
}