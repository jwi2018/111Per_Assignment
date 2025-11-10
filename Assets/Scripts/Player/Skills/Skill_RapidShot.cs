using System.Collections;
using UnityEngine;

public class Skill_RapidShot : SkillBase
{
    [Header("★ 스킬 1 (속사) 설정")]
    [SerializeField] private float _skill1Duration = 3.0f;
    [SerializeField] private float _skill1ShootInterval = 0.2f;
    [SerializeField] private float _skill1ArrowSpeedModifier = 1.1f;

    private Coroutine _skillActiveCoroutine;

    public override bool CanActivate()
    {
        return _player.State != PlayerState.Skill1Active;
    }

    public override void Activate()
    {
        _playerRigidbody.linearVelocity = Vector2.zero;

        _player.SetPlayerState(PlayerState.Skill1Active);

        if (_skillActiveCoroutine != null)
        {
            StopCoroutine(_skillActiveCoroutine);
        }
        _skillActiveCoroutine = StartCoroutine(Skill1ActiveCoroutine());
    }

    private IEnumerator Skill1ActiveCoroutine()
    {
        float timer = 0f;
        while (timer < _skill1Duration)
        {
            yield return new WaitForSeconds(_skill1ShootInterval);
            timer += _skill1ShootInterval;
        }

        OnSkillEnd();
        _skillActiveCoroutine = null;
    }

    public void ShootArrowAnimationEvent()
    {
        if (_arrowPrefab == null || _firePoint == null) return;

        GameObject arrowInstance = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);

        BasicArrow arrowScript = arrowInstance.GetComponent<BasicArrow>();
        if (arrowScript != null)
        {
            float launchSpeed = _player.ArrowLaunchSpeed * _skill1ArrowSpeedModifier;
            arrowScript.LaunchSkill1(30f, 65f, launchSpeed);
            PlayAttackSound();
        }
    }

    public override void OnSkillEnd()
    {
        _player.SetPlayerState(PlayerState.Attack);
        _player.ResetSkillCooldown(SkillID); // SkillBase에서 호출하도록 옮겨졌으나, 이 스킬에 특수 로직이 있다면 여기에 유지
    }
}