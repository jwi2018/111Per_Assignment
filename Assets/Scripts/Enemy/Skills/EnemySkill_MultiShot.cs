using UnityEngine;

public class EnemySkill_MultiShot : EnemySkillBase
{
    [Header("★ 스킬 2 (동시 발사) 설정")]
    [SerializeField] private int _skill2ArrowCount = 10;
    [SerializeField] private float _skill2SpreadAngle = 30f;
    [SerializeField] private float _skill2LaunchSpeedModifier = 0.8f;

    public override bool CanActivate()
    {
        return _enemy.State != EnemyState.Skill1Active && _enemy.State != EnemyState.Skill2Active;
    }

    public override void Activate()
    {
        _enemyRigidbody.linearVelocity = Vector2.zero;
        _enemy.SetEnemyState(EnemyState.Skill2Active);
    }

    public void ShootArrowAnimationEvent()
    {
        if (_enemyArrowPrefab == null || _firePoint == null) return;
        if (_enemy.State != EnemyState.Skill2Active) return;

        float baseAngle = 50f;
        float totalSpread = _skill2SpreadAngle;
        int arrowCount = _skill2ArrowCount;
        float launchSpeed = _enemy.ArrowLaunchSpeed * _skill2LaunchSpeedModifier;

        float startAngle = baseAngle - (totalSpread / 2f);
        float angleStep = (arrowCount > 1) ? totalSpread / (arrowCount - 1) : 0f;

        for (int i = 0; i < arrowCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            if (_enemy.transform.localScale.x < 0)
            {
                currentAngle = 180f - currentAngle;
            }

            GameObject arrowInstance = Instantiate(_enemyArrowPrefab, _firePoint.position, Quaternion.Euler(0, 0, currentAngle));
            EnemyBasicArrow arrowScript = arrowInstance.GetComponent<EnemyBasicArrow>();
            if (arrowScript != null)
            {
                arrowScript.Launch(currentAngle, launchSpeed);
                PlayAttackSound();
            }
        }
        OnSkillEnd();
    }
}