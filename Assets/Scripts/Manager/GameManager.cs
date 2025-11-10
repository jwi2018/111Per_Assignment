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
            Destroy(gameObject);
            return;
        }

        // 태그로 UIController를 찾아 참조
        GameObject uiObj = GameObject.FindGameObjectWithTag("UIController");
        if (uiObj != null)
        {
            uiController = uiObj.GetComponent<UIController>();
        }
        else
        {
            Debug.LogError("UIController 객체를 태그로 찾지 못했습니다.");
        }
    }

    void Update()
    {
        if (gameEnded) return;

        CheckEndCondition();
    }

    private void CheckEndCondition()
    {
        int playerHP = PlayerManager.Instance.GetPlayerData().CurrentHealth;
        int enemyHP = PlayerManager.Instance.GetEnemyData().CurrentHealth;

        // 1. 한쪽 체력 0 이하
        if (playerHP <= 0 || enemyHP <= 0)
        {
            gameEnded = true;

            bool playerWin = playerHP > 0;
            uiController?.SetResult(playerWin);
            return;
        }

        // 2. 타이머 종료
        if (uiController != null && uiController.CurrentTime <= 0)
        {
            gameEnded = true;

            bool playerWin = playerHP >= enemyHP;
            uiController.SetResult(playerWin);
            return;
        }
    }
}