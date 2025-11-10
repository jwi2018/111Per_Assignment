using UnityEngine;

public class GameManager : Singleton<PlayerManager>
{
    protected override void Awake()
    {
        base.Awake();

        if (this != Instance)
        {
            return;
        }
    }
}
