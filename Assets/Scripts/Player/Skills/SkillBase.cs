using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour
{
    public int SkillID;

    protected Player _player;
    protected Transform _firePoint;
    protected GameObject _arrowPrefab;
    protected Rigidbody2D _playerRigidbody;

    public virtual void Initialize(Player player, Transform firePoint, GameObject arrowPrefab, Rigidbody2D playerRigidbody)
    {
        _player = player;
        _firePoint = firePoint;
        _arrowPrefab = arrowPrefab;
        _playerRigidbody = playerRigidbody;
    }

    public abstract bool CanActivate();

    public abstract void Activate();

    public virtual void OnSkillEnd()
    {
        if (_player.State != PlayerState.Move)
        {
            _player.SetPlayerState(PlayerState.Attack);
        }
        _player.ResetSkillCooldown(SkillID);
    }

    protected void PlayAttackSound()
    {
        _player.PlayAttackSound();
    }
}