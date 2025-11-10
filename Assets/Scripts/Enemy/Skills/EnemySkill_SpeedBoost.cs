using UnityEngine;
using System.Collections;

public class EnemySkill_SpeedBoost : EnemySkillBase
{
    [Header("★ 스킬 4 (이동 속도 증가) 설정")]
    [SerializeField] private float _skill4Duration = 3.0f;
    [SerializeField] private float _skill4SpeedMultiplier = 2.0f;

    private Coroutine _skillActiveCoroutine;
    private float _originalMoveSpeed;

    public override bool CanActivate()
    {
        return _enemy.State != EnemyState.Skill1Active;
    }

    public override void Activate()
    {
        if (_skillActiveCoroutine != null)
        {
            StopCoroutine(_skillActiveCoroutine);
        }
        _skillActiveCoroutine = StartCoroutine(Skill4ActiveCoroutine());
    }

    private IEnumerator Skill4ActiveCoroutine()
    {
        _originalMoveSpeed = _enemy.MoveSpeed;
        _enemy.MoveSpeed = _originalMoveSpeed * _skill4SpeedMultiplier;
        _enemy.SetEnemyState(EnemyState.Skill4Active);

        float timer = 0f;
        while (timer < _skill4Duration)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        _enemy.MoveSpeed = _originalMoveSpeed;
        OnSkillEnd();
        _skillActiveCoroutine = null;
    }

    public override void OnSkillEnd()
    {
        _enemy.TryUseSkill(SkillID);
        _enemyController?.RestoreEnemyStateAfterSkillEnd();
    }
}