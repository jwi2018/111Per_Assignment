using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] Player player;
    [SerializeField] Enemy enemy;

    protected override void Awake()
    {
        base.Awake();

        if (this != Instance)
        {
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<Player>();
            }
        }

        if (enemy == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Enemy");
            if (playerObject != null)
            {
                enemy = playerObject.GetComponent<Enemy>();
            }
        }
    }

    #region Player 관련 함수

    public Player GetPlayerData()
    {
        return player;
    }

    #endregion

    #region Enemy 관련 함수

    public Enemy GetEnemyData()
    {
        return enemy;
    }

    #endregion
}
