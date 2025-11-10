using UnityEngine;

public enum EnemyState
{
    None,
    Idle,
    Move,
    Attack,
    Skill1Active,
    Skill2Active,
    Skill3Active,
    Skill4Active,
    Skill5Active,
    Death
}

public class Enemy : MonoBehaviour
{
    [Header("★ 상태")]
    [SerializeField] private EnemyState _state = EnemyState.Idle;

    [Header("★ 기본 능력치")]
    private int _maxHealth = 1000;
    private int _currentHealth;
    [SerializeField] private float _moveSpeed = 1.0f;
    [SerializeField] private float _attackDamage = 5f;

    [Header("★ 공격")]
    [SerializeField] private bool _canAttack = true;
    [SerializeField] private float _baseAttackCooldown = 2.0f;
    private float _currentAttackCooldown;
    private float _attackTimer = 0f;
    [SerializeField] private float _arrowLaunchSpeed = 8f;

    [Header("★ 스킬")]
    [SerializeField] public int[] skillCooldowns = new int[5];
    private int[] skillCooldownTimers = new int[5];
    private float[] skillDeltaTimeAccumulators = new float[5];

    [Header("★ 컴포넌트")]
    public Animator _animator;
    public Rigidbody2D _rigidbody2D;

    [Header("★ 타격 효과")]
    public AudioClip attackSoundClip;
    public AudioSource audioSource;


    #region Property

    public EnemyState State => _state;
    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;
    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }
    public float AttackDamage => _attackDamage;
    public bool CanAttack => _canAttack;

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
        if (_animator == null) _animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();

        _currentHealth = _maxHealth;
        _currentAttackCooldown = _baseAttackCooldown;

        if (skillCooldownTimers == null || skillCooldownTimers.Length != 5)
        {
            skillCooldownTimers = new int[5];
            skillDeltaTimeAccumulators = new float[5];
            if (skillCooldowns == null || skillCooldowns.Length != 5) skillCooldowns = new int[5];
        }

        for (int i = 0; i < skillCooldowns.Length; i++)
        {
            skillCooldownTimers[i] = 0;
            skillDeltaTimeAccumulators[i] = 0f;
        }

        SetEnemyState(_state);
    }

    void Update()
    {
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

        if (_animator != null)
        {
            _animator.SetBool("isMove", false);
            _animator.SetBool("isAttack", false);
            _animator.SetBool("Skill1", false);
            _animator.SetBool("Skill2", false);
            _animator.SetBool("Skill3", false);
            _animator.SetBool("Skill4", false);
            _animator.SetBool("Skill5", false);
            _animator.SetBool("Death", false);

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
                    _animator.SetBool("isMove", false);
                    break;
                case EnemyState.Skill2Active:
                    _animator.SetBool("Skill2", true);
                    _animator.SetBool("isMove", false);
                    break;
                case EnemyState.Skill3Active:
                    _animator.SetBool("Skill3", true);
                    _animator.SetBool("isMove", false);
                    break;
                case EnemyState.Skill4Active:
                    _animator.SetBool("Skill4", true);
                    break;
                case EnemyState.Skill5Active:
                    _animator.SetBool("Skill5", true);
                    _animator.SetBool("isMove", false);
                    break;
                case EnemyState.Death:
                    _animator.SetBool("Death", true);
                    break;
            }
        }
    }

    public void ResetAttackCooldown()
    {
        _canAttack = false;
        _attackTimer = _currentAttackCooldown;
    }

    public bool TryUseSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillCooldowns.Length) return false;

        if (skillCooldownTimers[skillIndex] <= 0)
        {
            skillCooldownTimers[skillIndex] = skillCooldowns[skillIndex];
            skillDeltaTimeAccumulators[skillIndex] = 0f;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            SetEnemyState(EnemyState.Death);
        }
    }
    public bool IsSkillReady(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillCooldownTimers.Length)
        {
            return false;
        }
        return skillCooldownTimers[skillIndex] <= 0;
    }

    public void PlayAttackSound()
    {
        if (audioSource != null && attackSoundClip != null)
        {
            audioSource.PlayOneShot(attackSoundClip);
        }
    }

    #endregion
}