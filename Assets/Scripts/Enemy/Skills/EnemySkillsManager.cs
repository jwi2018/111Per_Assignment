using UnityEngine;
using System.Collections.Generic;

public class EnemySkillsManager : MonoBehaviour
{
    [Header("★ 공통 참조")]
    [SerializeField] private Enemy _enemy;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _enemyArrowPrefab;
    [SerializeField] private GameObject _enemyFireArrowPrefab;
    [SerializeField] private GameObject _enemyGroundFireEffectPrefab;
    [SerializeField] private GameObject _enemyShieldPrefab;

    private Rigidbody2D _enemyRigidbody;
    private EnemyController _enemyController;
    private Dictionary<int, EnemySkillBase> _skillsDictionary = new Dictionary<int, EnemySkillBase>();

    [Header("★ 스킬 컴포넌트 연결")]
    [SerializeField] private EnemySkill_RapidShot _skillRapidShot;
    [SerializeField] private EnemySkill_MultiShot _skillMultiShot;
    [SerializeField] private EnemySkill_FireArrow _skillFireArrow;
    [SerializeField] private EnemySkill_SpeedBoost _skillSpeedBoost;
    [SerializeField] private EnemySkill_Shield _skillShield;


    void Awake()
    {
        if (_enemy == null) _enemy = GetComponent<Enemy>();
        if (_enemyRigidbody == null) _enemyRigidbody = GetComponent<Rigidbody2D>();
        if (_enemyController == null) _enemyController = GetComponent<EnemyController>();

        EnemySkillBase[] skills = GetComponents<EnemySkillBase>();
        foreach (var skill in skills)
        {
            if (skill == null) continue;

            skill.Initialize(_enemy, _firePoint, _enemyArrowPrefab, _enemyRigidbody, _enemyController);

            if (!_skillsDictionary.ContainsKey(skill.SkillID))
            {
                _skillsDictionary.Add(skill.SkillID, skill);
            }
            else
            {
                Debug.LogWarning($"중복된 SkillID {skill.SkillID}가 존재합니다. {skill.name} 컴포넌트.");
            }
        }

        if (_skillFireArrow != null) _skillFireArrow.SetSpecificPrefabs(_enemyFireArrowPrefab, _enemyGroundFireEffectPrefab);
        if (_skillShield != null) _skillShield.SetSpecificPrefabs(_enemyShieldPrefab);
    }

    public bool TryActivateSkill(int skillID)
    {
        if (_skillsDictionary.TryGetValue(skillID, out EnemySkillBase skill))
        {
            if (_enemy.TryUseSkill(skillID) && skill.CanActivate())
            {
                skill.Activate();
                return true;
            }
            return false;
        }
        return false;
    }

    public void AnimationEvent_EnemyAttack()
    {
        if (_enemyArrowPrefab == null || _firePoint == null) return;
        if (!_enemy.CanAttack) return;

        float baseAngle = 50f;
        float currentAngle = baseAngle;
        if (_enemy.transform.localScale.x < 0)
        {
            currentAngle = 180f - currentAngle;
        }

        GameObject arrowInstance = Instantiate(_enemyArrowPrefab, _firePoint.position, Quaternion.Euler(0, 0, currentAngle));

        EnemyBasicArrow arrowScript = arrowInstance.GetComponent<EnemyBasicArrow>();
        if (arrowScript != null)
        {
            arrowScript.Launch(currentAngle, _enemy.ArrowLaunchSpeed);
            _enemy.PlayAttackSound();
        }
        _enemy.ResetAttackCooldown();
    }

    public void EnemyAnimationEvent_ShootArrow_Skill1()
    {
        _skillRapidShot?.ShootArrowAnimationEvent();
    }

    public void EnemyAnimationEvent_ShootArrow_Skill2()
    {
        _skillMultiShot?.ShootArrowAnimationEvent();
    }

    public void EnemyAnimationEvent_ShootArrow_Skill3()
    {
        _skillFireArrow?.ShootArrowAnimationEvent();
    }

    public void EnemyAnimationEvent_SpawnShield()
    {
        _skillShield?.SpawnShieldAnimationEvent();
    }
}