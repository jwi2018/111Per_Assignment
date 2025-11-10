using UnityEngine;

public class Skill_MultiShot : SkillBase
{
    [SerializeField] private int _skill2ArrowCount = 10;
    [SerializeField] private float _skill2SpreadAngle = 30f;
    [SerializeField] private float _skill2LaunchSpeedModifier = 1f;

    public override bool CanActivate()
    {
        return _player.State != PlayerState.Skill1Active && _player.State != PlayerState.Skill2Active;
    }

    public override void Activate()
    {
        _playerRigidbody.linearVelocity = Vector2.zero;
        _player.SetPlayerState(PlayerState.Skill2Active);
    }

    public void ShootArrowAnimationEvent()
    {
        if (_arrowPrefab == null || _firePoint == null) return;
        if (_player.State != PlayerState.Skill2Active) return;

        float baseAngle = 50f;
        float totalSpread = _skill2SpreadAngle;
        int arrowCount = _skill2ArrowCount;
        float launchSpeed = _player.ArrowLaunchSpeed * _skill2LaunchSpeedModifier;

        float startAngle = baseAngle - (totalSpread / 2f);
        float angleStep = (arrowCount > 1) ? totalSpread / (arrowCount - 1) : 0f;

        for (int i = 0; i < arrowCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);

            if (_player.transform.localScale.x < 0)
            {
                currentAngle = 180f - currentAngle;
            }

            GameObject arrowInstance = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);
            BasicArrow arrowScript = arrowInstance.GetComponent<BasicArrow>();
            if (arrowScript != null)
            {
                arrowScript.Launch(currentAngle, launchSpeed);
                PlayAttackSound();
            }
        }
        OnSkillEnd();
    }
}