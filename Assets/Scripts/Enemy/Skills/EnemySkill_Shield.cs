using UnityEngine;

public class EnemySkill_Shield : EnemySkillBase
{
    [Header("★ 스킬 5 (방어막 생성) 설정")]
    [SerializeField] private float _skill5ShieldDuration = 6.0f;
    [SerializeField] private int _skill5ShieldHealth = 100;

    private GameObject _enemyShieldPrefab;

    public void SetSpecificPrefabs(GameObject shield)
    {
        _enemyShieldPrefab = shield;
    }

    public override bool CanActivate()
    {
        return _enemy.State != EnemyState.Skill1Active && _enemy.State != EnemyState.Skill2Active && _enemy.State != EnemyState.Skill3Active && _enemy.State != EnemyState.Skill5Active;
    }

    public override void Activate()
    {
        _enemyRigidbody.linearVelocity = Vector2.zero;
        _enemy.SetEnemyState(EnemyState.Skill5Active);
    }

    public void SpawnShieldAnimationEvent()
    {
        if (_enemyShieldPrefab == null || _enemy == null) return;
        if (_enemy.State != EnemyState.Skill5Active) return;

        Vector3 spawnPosition = _enemy.transform.position;
        GameObject shieldInstance = Instantiate(_enemyShieldPrefab, spawnPosition, Quaternion.identity);

        EnemyShield shieldScript = shieldInstance.GetComponent<EnemyShield>();
        if (shieldScript != null)
        {
            shieldScript.SetShieldProperties(_skill5ShieldHealth, _skill5ShieldDuration);
        }
        OnSkillEnd();
    }
}