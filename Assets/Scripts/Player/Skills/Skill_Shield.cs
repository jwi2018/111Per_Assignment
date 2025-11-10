using UnityEngine;

public class Skill_Shield : SkillBase
{
    [SerializeField] private float _skill5ShieldDuration = 6.0f;
    [SerializeField] private int _skill5ShieldHealth = 100;

    private GameObject _shieldPrefab;

    public void SetSpecificPrefabs(GameObject shield)
    {
        _shieldPrefab = shield;
    }

    public override bool CanActivate()
    {
        return _player.State != PlayerState.Skill1Active && _player.State != PlayerState.Skill2Active && _player.State != PlayerState.Skill3Active && _player.State != PlayerState.Skill5Active;
    }

    public override void Activate()
    {
        _playerRigidbody.linearVelocity = Vector2.zero;
        _player.SetPlayerState(PlayerState.Skill5Active);
    }

    public void SpawnShieldAnimationEvent()
    {
        if (_shieldPrefab == null || _player == null) return;
        if (_player.State != PlayerState.Skill5Active) return;

        Vector3 spawnPosition = _player.transform.position;
        GameObject shieldInstance = Instantiate(_shieldPrefab, spawnPosition, Quaternion.identity);

        Shield shieldScript = shieldInstance.GetComponent<Shield>();
        if (shieldScript != null)
        {
            shieldScript.SetShieldProperties(_skill5ShieldHealth, _skill5ShieldDuration);
        }

        if (_playerRigidbody.linearVelocity.sqrMagnitude > 0.01f)
        {
            _player.SetPlayerState(PlayerState.Move);
        }
        else
        {
            _player.SetPlayerState(PlayerState.Attack);
        }
        OnSkillEnd();
    }
}