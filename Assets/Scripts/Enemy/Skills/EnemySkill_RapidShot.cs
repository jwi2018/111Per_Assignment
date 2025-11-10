using UnityEngine;
using System.Collections;

public class EnemySkill_RapidShot : EnemySkillBase
{
    [Header("★ 스킬 1 (속사) 설정")]
    [SerializeField] private float _skill1Duration = 3.0f;
    [SerializeField] private float _skill1ShootInterval = 0.2f;
    [SerializeField] private float _skill1ArrowSpeedModifier = 1.2f;

    private Coroutine _skillActiveCoroutine;

    public override bool CanActivate()
    {
        return _enemy.State != EnemyState.Skill1Active;
    }

    public override void Activate()
    {
        _enemyRigidbody.linearVelocity = Vector2.zero;
        _enemy.SetEnemyState(EnemyState.Skill1Active);

        if (_skillActiveCoroutine != null)
        {
            StopCoroutine(_skillActiveCoroutine);
        }
        _skillActiveCoroutine = StartCoroutine(Skill1ActiveCoroutine());
    }

    private IEnumerator Skill1ActiveCoroutine()
    {
        float previousAttackCooldown = _enemy.AttackCooldown;
        _enemy.AttackCooldown = _enemy.BaseAttackCooldown / 2.0f;

        float timer = 0f;
        while (timer < _skill1Duration)
        {
            yield return new WaitForSeconds(_skill1ShootInterval);
            timer += _skill1ShootInterval;
        }

        _enemy.AttackCooldown = previousAttackCooldown;

        OnSkillEnd();
        _skillActiveCoroutine = null;
    }

    public void ShootArrowAnimationEvent()
    {
        if (_enemyArrowPrefab == null || _firePoint == null) return;
        if (!_enemy.CanAttack && _enemy.State != EnemyState.Skill1Active) return;

        float minAngle = 130f; 
        float maxAngle = 150f; 


        GameObject arrowInstance = Instantiate(_enemyArrowPrefab, _firePoint.position, _firePoint.rotation);

        EnemyBasicArrow arrowScript = arrowInstance.GetComponent<EnemyBasicArrow>();
        if (arrowScript != null)
        {
            float launchSpeed = _enemy.ArrowLaunchSpeed * _skill1ArrowSpeedModifier;
            arrowScript.LaunchSkill1(minAngle, maxAngle, launchSpeed);
            PlayAttackSound();
        }
    }
}