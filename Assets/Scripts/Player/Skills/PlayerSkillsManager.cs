using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillsManager : MonoBehaviour
{
    [Header("★ 공통 참조")]
    [SerializeField] private Player _player;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _arrowPrefab;
    [SerializeField] private GameObject _fireArrowPrefab;
    [SerializeField] private GameObject _groundFireEffectPrefab;
    [SerializeField] private GameObject _shieldPrefab;

    private Rigidbody2D _playerRigidbody;
    private Dictionary<int, SkillBase> _skillsDictionary = new Dictionary<int, SkillBase>();

    [Header("★ 스킬 컴포넌트 연결")]
    [SerializeField] private Skill_RapidShot _skillRapidShot;
    [SerializeField] private Skill_MultiShot _skillMultiShot;
    [SerializeField] private Skill_FireArrow _skillFireArrow;
    [SerializeField] private Skill_SpeedBoost _skillSpeedBoost;
    [SerializeField] private Skill_Shield _skillShield;


    void Awake()
    {
        if (_player == null) _player = GetComponentInParent<Player>();
        if (_playerRigidbody == null) _playerRigidbody = GetComponentInParent<Rigidbody2D>();

        SkillBase[] skills = GetComponents<SkillBase>();
        foreach (var skill in skills)
        {
            if (skill == null) continue;

            skill.Initialize(_player, _firePoint, _arrowPrefab, _playerRigidbody);
            if (!_skillsDictionary.ContainsKey(skill.SkillID))
            {
                _skillsDictionary.Add(skill.SkillID, skill);
            }
        }

        if (_skillFireArrow != null) _skillFireArrow.SetSpecificPrefabs(_fireArrowPrefab, _groundFireEffectPrefab);
        if (_skillShield != null) _skillShield.SetSpecificPrefabs(_shieldPrefab);
    }

    public bool TryActivateSkill(int skillID)
    {
        if (_skillsDictionary.TryGetValue(skillID, out SkillBase skill))
        {
            if (_player.TryUseSkill(skillID) && skill.CanActivate())
            {
                skill.Activate();
                return true;
            }
            return false;
        }
        return false;
    }

    public void AnimationEvent_ShootArrow()
    {
        if (_arrowPrefab == null || _firePoint == null) return;
        if (!_player.CanShoot) return;

        GameObject arrowInstance = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);

        BasicArrow arrowScript = arrowInstance.GetComponent<BasicArrow>();
        if (arrowScript != null)
        {
            float launchSpeed = _player.ArrowLaunchSpeed;
            arrowScript.Launch(50f, launchSpeed);
            PlayHitSound();
        }
        _player.ResetShootCooldown();
    }

    public void AnimationEvent_ShootArrow_Skill1()
    {
        _skillRapidShot?.ShootArrowAnimationEvent();
    }

    public void AnimationEvent_ShootArrow_Skill2()
    {
        _skillMultiShot?.ShootArrowAnimationEvent();
    }

    public void AnimationEvent_ShootArrow_Skill3()
    {
        _skillFireArrow?.ShootArrowAnimationEvent();
    }

    public void AnimationEvent_SpawnShield()
    {
        _skillShield?.SpawnShieldAnimationEvent();
    }

    private void PlayHitSound()
    {
        _player.PlayAttackSound();
    }
}