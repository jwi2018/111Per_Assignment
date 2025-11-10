using UnityEngine;

public enum EnemyState
{
    None,
    Idle,
    Move,
    Attack, // 발사 또는 근접 공격
    Skill1Active, // <-- 스킬 1 (속사) 활성화 상태
    Skill2Active, // <-- 스킬 2 (샷건) 활성화 상태
    Skill3Active, // <-- 스킬 3 (불화살) 활성화 상태
    Skill4Active, // <-- 스킬 4 (이동 속도 증가) 활성화 상태 (AI도 상태를 가지고 시작과 종료를 알림)
    Skill5Active  // <-- 스킬 5 (방어막) 활성화 상태
}

public class Enemy : MonoBehaviour
{
    [Header("★ 상태")]
    [SerializeField] private EnemyState _state = EnemyState.Idle;

    [Header("★ 기본 능력치")]
    private int _maxHealth = 800;
    private int _currentHealth;
    [SerializeField] private float _moveSpeed = 1.0f;
    [SerializeField] private float _attackDamage = 5f;

    [Header("★ 공격")]
    [SerializeField] private bool _canAttack = true; // 공격 가능 여부
    [SerializeField] private float _baseAttackCooldown = 2.0f; // 기본 공격 쿨다운
    private float _currentAttackCooldown; // 현재 적용되는 공격 쿨다운
    private float _attackTimer = 0f; // 다음 공격까지 남은 시간
    [SerializeField] private float _arrowLaunchSpeed = 8f; // 화살 발사 속도 (적용될 화살이 없으므로 주석 처리)

    [Header("★ 스킬")]
    [SerializeField] public int[] skillCooldowns = new int[5];     // 스킬별 쿨다운 시간
    private int[] skillCooldownTimers = new int[5];                 // 현재 스킬별 남은 쿨다운 시간
    private float[] skillDeltaTimeAccumulators = new float[5];      // 쿨다운 타이머 정밀도를 위한 누적 시간

    [Header("★ 컴포넌트")]
    public Animator _animator;        // 애니메이터 컴포넌트
    public Rigidbody2D _rigidbody2D;

    [Header("★ 타격 효과")]
    public AudioClip attackSoundClip;     // 타격 사운드
    public AudioSource audioSource;   // AudioSource 컴포넌트


    #region Property (읽기 전용)

    public EnemyState State => _state;
    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;
    public float MoveSpeed // <-- 수정
    {
        get => _moveSpeed; // 이제 _moveSpeed는 직접적인 필드 이름이므로 프로퍼티 이름과 다르게
        set => _moveSpeed = value; // 값을 설정할 수 있도록 set 접근자 추가
    }
    public bool CanAttack => _canAttack;

    // 공격 쿨다운 관련 Property
    public float AttackCooldown
    {
        get => _currentAttackCooldown;
        set => _currentAttackCooldown = value;
    }
    public float BaseAttackCooldown => _baseAttackCooldown;
    public float ArrowLaunchSpeed => _arrowLaunchSpeed;

    #endregion

    #region Unity 생명주기

    void Awake()
    {
        _currentHealth = _maxHealth;
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();

        _currentAttackCooldown = _baseAttackCooldown; // 공격 쿨다운 초기화

        // 스킬 쿨다운 타이머 초기화 (스킬 0~4까지 5개)
        if (skillCooldownTimers == null || skillCooldownTimers.Length != 5)
        {
            skillCooldownTimers = new int[5];
            skillDeltaTimeAccumulators = new float[5];
            // skillCooldowns도 Inspector에서 설정된 값이 없으면 5개로 초기화
            if (skillCooldowns == null || skillCooldowns.Length != 5) skillCooldowns = new int[5];
        }

        for (int i = 0; i < skillCooldowns.Length; i++)
        {
            skillCooldownTimers[i] = 0;
            skillDeltaTimeAccumulators[i] = 0f;
        }

        SetEnemyState(_state); // 초기 상태 설정
    }

    void Update()
    {
        // 스킬 쿨다운 타이머 업데이트
        for (int i = 0; i < skillCooldownTimers.Length; i++)
        {
            if (skillCooldownTimers[i] > 0)
            {
                skillDeltaTimeAccumulators[i] += Time.deltaTime;
                if (skillDeltaTimeAccumulators[i] >= 1.0f)
                {
                    skillCooldownTimers[i]--;
                    skillDeltaTimeAccumulators[i] -= 1.0f;
                }
            }
        }

        // 공격 쿨다운 타이머 업데이트
        if (!_canAttack)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackTimer = 0f;
                _canAttack = true;
            }
        }
    }

    #endregion

    #region Public 함수

    public void SetEnemyState(EnemyState newState)
    {
        if (_state == newState) return;

        _state = newState;
        Debug.Log($"Enemy State changed to: {_state}");

        if (_animator != null)
        {
            _animator.SetBool("isMove", false);
            _animator.SetBool("isAttack", false);
            _animator.SetBool("Skill1", false);
            _animator.SetBool("Skill2", false);
            _animator.SetBool("Skill3", false);
            _animator.SetBool("Skill4", false); // 스킬 4도 애니메이션 상태가 됩니다.
            _animator.SetBool("Skill5", false); // <-- 추가: Skill5 애니메이터 Bool 파라미터 초기화

            switch (newState)
            {
                case EnemyState.Move:
                    _animator.SetBool("isMove", true);
                    break;
                case EnemyState.Attack:
                    _animator.SetBool("isAttack", true);
                    break;
                case EnemyState.Skill1Active:
                    _animator.SetBool("Skill1", true);
                    _animator.SetBool("isMove", false); // 스킬 발동 시 이동 애니메이션 중단
                    break;
                case EnemyState.Skill2Active:
                    _animator.SetBool("Skill2", true);
                    _animator.SetBool("isMove", false); // 스킬 발동 시 이동 애니메이션 중단
                    break;
                case EnemyState.Skill3Active:
                    _animator.SetBool("Skill3", true);
                    _animator.SetBool("isMove", false); // 스킬 발동 시 이동 애니메이션 중단
                    break;
                case EnemyState.Skill4Active: // <-- 수정: Skill4도 이제 상태를 가짐 (이동 속도 버프 이펙트 애니메이션 용)
                    _animator.SetBool("Skill4", true);
                    _animator.SetBool("isMove", true); // 이동 속도 버프 스킬이므로 isMove는 유지
                    break;
                case EnemyState.Skill5Active: // <-- 추가: Skill5Active 상태 처리
                    _animator.SetBool("Skill5", true);
                    _animator.SetBool("isMove", false); // 방어막 생성 중에는 이동 애니메이션 중단
                    break;
                case EnemyState.Idle:
                case EnemyState.None:
                    break;
            }
        }
    }

    /// 공격 쿨다운 리셋
    public void ResetAttackCooldown()
    {
        _canAttack = false;
        _attackTimer = _currentAttackCooldown;
    }

    // 스킬 사용 시도 및 쿨다운 관리 (플레이어와 동일한 함수 재사용)
    public bool TryUseSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillCooldowns.Length) return false;

        if (skillCooldownTimers[skillIndex] <= 0) // 쿨다운이 0 이하면 사용 가능
        {
            skillCooldownTimers[skillIndex] = skillCooldowns[skillIndex]; // 쿨다운 리셋
            skillDeltaTimeAccumulators[skillIndex] = 0f;
            Debug.Log($"Enemy Skill {skillIndex + 1} activated! Cooldown: {skillCooldowns[skillIndex]}s");
            return true;
        }
        else
        {
            Debug.Log($"Enemy Skill {skillIndex + 1} is on cooldown. {skillCooldownTimers[skillIndex]}s remaining.");
            return false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (_currentHealth <= 0) return; // 이미 죽었으면 데미지 받지 않음

        _currentHealth -= amount;
        Debug.Log($"Enemy takes {amount} damage. Current Health: {_currentHealth}");
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Debug.Log("적 사망!");
            // TODO: 적 사망 처리 로직 (애니메이션, 파티클, 오브젝트 비활성화 등)
        }
    }

    // 스킬이 현재 쿨다운 상태인지 여부를 반환하는 함수 (EnemyController에서 호출)
    public bool IsSkillReady(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillCooldownTimers.Length)
        {
            Debug.LogError($"Invalid skillIndex: {skillIndex} in IsSkillReady.");
            return false;
        }
        return skillCooldownTimers[skillIndex] <= 0;
    }

    #endregion
}
