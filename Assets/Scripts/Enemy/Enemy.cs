using UnityEngine;

public enum EnemyState
{
    None,
    Idle,
    Move,
    Attack, // 발사 또는 근접 공격
    Skill   // 스킬 사용
}

public class Enemy : MonoBehaviour
{
    [Header("★ 상태")]
    private EnemyState _state = EnemyState.Idle; // 현재 적군의 상태

    [Header("★ 이동")]
    [SerializeField] private float _moveSpeed = 6.0f; // 적군 이동 속도 (Inspector에서 조절)

    [Header("★ 공격 (기본 공격)")]
    private bool _canAttack = true; // 현재 공격 가능한지 여부
    [SerializeField] private float _attackCooldown = 0.5f; // 기본 공격 쿨다운
    private float _attackTimer = 0f; // 기본 공격 쿨다운 타이머
    public GameObject _enemyBasicArrow;
    [SerializeField] private float _fireAngle = 50f;     // <--- 고정 발사 각도 추가 (Inspector에서 설정)

    [Header("★ 체력")]
    [SerializeField] private int _maxHealth = 50; // 최대 체력 (Inspector에서 조절)
    private int _currentHealth; // 현재 체력

    [Header("★ 스킬")]
    [SerializeField] private int[] _skillCooldowns = new int[5];     // 스킬별 쿨다운 시간 (예: 4개 스킬)
    private int[] _skillCooldownTimers = new int[5];                 // 스킬별 쿨다운 타이머
    private float[] _skillDeltaTimeAccumulators = new float[5]; // 1초 단위 계산용

    [Header("★ 컴포넌트")]
    public Animator _animator; // 적군 애니메이터
    public Transform _attackPoint; // 공격 발사 위치 (FirePoint와 유사)


    #region Property (읽기 전용)

    public EnemyState State => _state;
    public float MoveSpeed => _moveSpeed;
    public bool CanAttack => _canAttack;
    public float AttackCooldown => _attackCooldown;
    public float AttackTimer => _attackTimer;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public GameObject EnemyBasicArrowPrefab => _enemyBasicArrow; // <--- 프리팹 Property 추가
    public float FireAngle => _fireAngle;                       // <--- 발사 각도 Property 추가

    #endregion

    #region Unity 생명주기

    void Awake()
    {
        _currentHealth = _maxHealth; // 초기 체력 설정
        // _skillCooldownTimers와 _skillDeltaTimeAccumulators는 기본값이 0이므로 초기화 필요 없음.
    }

    void Update()
    {
        // === 스킬 쿨다운 타이머 업데이트 ===
        for (int i = 0; i < _skillCooldownTimers.Length; i++)
        {
            if (_skillCooldownTimers[i] > 0)
            {
                _skillDeltaTimeAccumulators[i] += Time.deltaTime;
                if (_skillDeltaTimeAccumulators[i] >= 1.0f)
                {
                    _skillCooldownTimers[i]--;
                    _skillDeltaTimeAccumulators[i] -= 1.0f;
                }
            }
        }

        // === 기본 공격 쿨다운 타이머 업데이트 ===
        if (!_canAttack)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackTimer = 0f;
                _canAttack = true; // 쿨다운 끝, 공격 가능
            }
        }
    }

    #endregion

    #region Public 함수

    public void SetEnemyState(EnemyState newState)
    {
        if (_state == newState) return;
        _state = newState;
        // Debug.Log($"Enemy State Changed to: {_state}");
        // if (_animator != null) _animator.SetInteger("EnemyState", (int)_state);
    }

    // 기본 공격 쿨다운 시작
    public void ResetAttackCooldown()
    {
        _canAttack = false;
        _attackTimer = _attackCooldown;
    }

    // 스킬 사용을 시도하는 메서드 (플레이어 UseSkill과 동일)
    public bool UseSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= _skillCooldowns.Length)
        {
            Debug.LogWarning($"Enemy: 유효하지 않은 스킬 인덱스: {skillIndex}", this);
            return false;
        }

        if (_skillCooldownTimers[skillIndex] <= 0)
        {
            if (_animator != null)
            {
                // Animator에 "EnemySkill1", "EnemySkill2" 등의 트리거 파라미터가 있어야 합니다.
                _animator.SetTrigger($"EnemySkill{skillIndex + 1}");
            }
            _skillCooldownTimers[skillIndex] = _skillCooldowns[skillIndex];
            _skillDeltaTimeAccumulators[skillIndex] = 0f;
            // 스킬 발동 시 추가 로직 (이펙트, 데미지 등)
            return true;
        }
        return false; // 쿨다운 중
    }

    // 데미지를 받는 함수 (플레이어 TakeDamage와 동일)
    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Debug.Log("Enemy 사망!");
            // 적군 사망 처리 로직 (비활성화, Destroy, 아이템 드랍 등)
            Destroy(gameObject); // 예시: 사망 시 오브젝트 제거
        }
    }

    #endregion
}
