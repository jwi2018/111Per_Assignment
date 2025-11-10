using UnityEngine;

public class Skill_FireArrow : SkillBase
{
    [SerializeField] private float _skill3LaunchSpeedModifier = 1.0f;

    private GameObject _fireArrowPrefab;
    private GameObject _groundFireEffectPrefab;

    public void SetSpecificPrefabs(GameObject fireArrow, GameObject groundFireEffect)
    {
        _fireArrowPrefab = fireArrow;
        _groundFireEffectPrefab = groundFireEffect;
    }

    public override bool CanActivate()
    {
        return _player.State != PlayerState.Skill1Active && _player.State != PlayerState.Skill2Active && _player.State != PlayerState.Skill3Active;
    }

    public override void Activate()
    {
        _playerRigidbody.linearVelocity = Vector2.zero;
        _player.SetPlayerState(PlayerState.Skill3Active);
    }

    public void ShootArrowAnimationEvent()
    {
        if (_fireArrowPrefab == null || _firePoint == null) return;
        if (_player.State != PlayerState.Skill3Active) return;

        float launchSpeed = _player.ArrowLaunchSpeed * _skill3LaunchSpeedModifier;
        float baseAngle = 50f;
        float currentAngle = baseAngle;

        if (_player.transform.localScale.x < 0)
        {
            currentAngle = 180f - currentAngle;
        }

        GameObject arrowInstance = Instantiate(_fireArrowPrefab, _firePoint.position, _firePoint.rotation);

        FireArrow fireArrowScript = arrowInstance.GetComponent<FireArrow>();
        if (fireArrowScript != null)
        {
            fireArrowScript.fireEffectPrefab = _groundFireEffectPrefab;
            fireArrowScript.Launch(currentAngle, launchSpeed);
            PlayAttackSound();
        }
        OnSkillEnd();
    }
}