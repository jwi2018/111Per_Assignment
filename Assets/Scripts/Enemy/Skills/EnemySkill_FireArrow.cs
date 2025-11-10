using UnityEngine;

public class EnemySkill_FireArrow : EnemySkillBase
{
    [Header("★ 스킬 3 (불화살) 설정")]
    [SerializeField] private float _skill3LaunchSpeedModifier = 1.0f;
    [SerializeField] private float _skill3GroundFireEffectYPosition = -2.5f;

    private GameObject _enemyFireArrowPrefab;
    private GameObject _enemyGroundFireEffectPrefab;

    public void SetSpecificPrefabs(GameObject fireArrow, GameObject groundFireEffect)
    {
        _enemyFireArrowPrefab = fireArrow;
        _enemyGroundFireEffectPrefab = groundFireEffect;
    }

    public override bool CanActivate()
    {
        return _enemy.State != EnemyState.Skill1Active && _enemy.State != EnemyState.Skill2Active && _enemy.State != EnemyState.Skill3Active;
    }

    public override void Activate()
    {
        _enemyRigidbody.linearVelocity = Vector2.zero;
        _enemy.SetEnemyState(EnemyState.Skill3Active);
    }

    public void ShootArrowAnimationEvent()
    {
        if (_enemyFireArrowPrefab == null || _firePoint == null) return;
        if (_enemy.State != EnemyState.Skill3Active) return;

        float launchSpeed = _enemy.ArrowLaunchSpeed * _skill3LaunchSpeedModifier;
        float baseAngle = 50f;

        float currentAngle = baseAngle;
        if (_enemy.transform.localScale.x < 0)
        {
            currentAngle = 180f - currentAngle;
        }

        GameObject arrowInstance = Instantiate(_enemyFireArrowPrefab, _firePoint.position, Quaternion.Euler(0, 0, currentAngle));

        EnemyFireArrow fireArrowScript = arrowInstance.GetComponent<EnemyFireArrow>();
        if (fireArrowScript != null)
        {
            fireArrowScript.enemyFireEffectPrefab = _enemyGroundFireEffectPrefab;
            fireArrowScript.groundFireEffectYPosition = _skill3GroundFireEffectYPosition;
            fireArrowScript.Launch(currentAngle, launchSpeed);
            PlayAttackSound();
        }
        OnSkillEnd();
    }
}