using System.Collections;
using UnityEngine;

public class Skill_SpeedBoost : SkillBase
{
    [SerializeField] private float _skill4Duration = 6.0f;
    [SerializeField] private float _skill4SpeedMultiplier = 1.5f;

    private Coroutine _skillActiveCoroutine;

    public override bool CanActivate()
    {
        return _player.State != PlayerState.Skill1Active;
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
        float originalMoveSpeed = _player.MoveSpeed;
        _player.MoveSpeed = originalMoveSpeed * _skill4SpeedMultiplier;

        float timer = 0f;
        while (timer < _skill4Duration)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        _player.MoveSpeed = originalMoveSpeed;

        OnSkillEnd();
        _skillActiveCoroutine = null;
    }

    public override void OnSkillEnd()
    {
        _player.ResetSkillCooldown(SkillID);
    }
}