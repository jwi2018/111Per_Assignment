using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    None,
    Move,
    Attack,
    Skill1Active,
    Skill2Active,
    Skill3Active,
    Skill5Active,
    Death
}

public class Player : MonoBehaviour
{
    [Header("★ 상태")]
    private PlayerState _state = PlayerState.None;

    [Header("★ 이동")]
    [SerializeField] private float _baseMoveSpeed = 3.5f; 
    private float _currentMoveSpeed; 

    [Header("★ 공격")]
    [SerializeField] private bool _canShoot = true;          
    [SerializeField] private float _baseShootCooldown = 0.5f; 
    private float _currentShootCooldown;                     
    private float _shootTimer = 0f;                 
    [SerializeField] private float _arrowLaunchSpeed = 9.8f; 

    [Header("★ 체력")]
    [SerializeField] private int _maxHealth = 1000;
    private int _currentHealth;

    [Header("★ 스킬")]
    [SerializeField] public float[] skillBaseCooldowns = new float[5];    
    private float[] currentSkillCooldowns = new float[5];                 

    [Header("★ 컴포넌트")]
    public Animator _animator; 
    public PlayerInput _playerInput; 

    [Header("★ 사운드")]
    public AudioClip attackSoundClip;     
    public AudioSource audioSource;       

    #region Property

    public PlayerState State => _state;
    public float MoveSpeed
    {
        get => _currentMoveSpeed;
        set => _currentMoveSpeed = value; 
    }
    public float BaseMoveSpeed => _baseMoveSpeed;

    public bool CanShoot => _canShoot;

    public float ShootCooldown
    {
        get => _currentShootCooldown;
        set => _currentShootCooldown = value;
    }

    public float BaseShootCooldown => _baseShootCooldown;

    public float ShootTimer => _shootTimer;
    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;
    public float ArrowLaunchSpeed => _arrowLaunchSpeed;

    public float GetSkillRemainingCoolTime(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= currentSkillCooldowns.Length) return 0f;
        return currentSkillCooldowns[skillIndex];
    }

    #endregion

    #region Unity 생명주기

    void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        _currentHealth = _maxHealth;
        _currentMoveSpeed = _baseMoveSpeed;
        _currentShootCooldown = _baseShootCooldown;

        if (skillBaseCooldowns == null || skillBaseCooldowns.Length != 5)
        {
            skillBaseCooldowns = new float[5];
        }
        currentSkillCooldowns = new float[skillBaseCooldowns.Length];

        for (int i = 0; i < skillBaseCooldowns.Length; i++)
        {
            currentSkillCooldowns[i] = 0f;
        }

        SetPlayerState(PlayerState.Attack);
    }

    void Update()
    {
        for (int i = 0; i < currentSkillCooldowns.Length; i++)
        {
            if (currentSkillCooldowns[i] > 0f)
            {
                currentSkillCooldowns[i] -= Time.deltaTime;
                if (currentSkillCooldowns[i] < 0f)
                {
                    currentSkillCooldowns[i] = 0f;
                }
            }
        }

        if (!_canShoot)
        {
            _shootTimer -= Time.deltaTime;
            if (_shootTimer <= 0f)
            {
                _shootTimer = 0f;
                _canShoot = true;
            }
        }
    }

    #endregion


    #region Public 함수

    public void ResetShootCooldown()
    {
        _canShoot = false;
        _shootTimer = _currentShootCooldown;
    }

    public void ResetSkillCooldown(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < skillBaseCooldowns.Length)
        {
            currentSkillCooldowns[skillIndex] = skillBaseCooldowns[skillIndex];
        }
    }

    public void SetPlayerState(PlayerState newState)
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
            _animator.SetBool("Skill5", false);
            _animator.SetBool("Death", false);

            switch (newState)
            {
                case PlayerState.Move:
                    _animator.SetBool("isMove", true);
                    break;
                case PlayerState.Attack:
                    _animator.SetBool("isAttack", true); 
                    break;
                case PlayerState.Skill1Active:
                    _animator.SetBool("Skill1", true);
                    _animator.SetBool("isMove", false);
                    break;
                case PlayerState.Skill2Active:
                    _animator.SetBool("Skill2", true);
                    _animator.SetBool("isMove", false);
                    break;
                case PlayerState.Skill3Active:
                    _animator.SetBool("Skill3", true);
                    _animator.SetBool("isMove", false);
                    break;
                case PlayerState.Skill5Active:
                    _animator.SetBool("Skill5", true);
                    _animator.SetBool("isMove", false);
                    break;
                case PlayerState.Death:
                    _animator.SetBool("Death", true);
                    break;
            }
        }
    }

    public bool TryUseSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillBaseCooldowns.Length) return false;

        if (currentSkillCooldowns[skillIndex] <= 0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            SetPlayerState(PlayerState.Death);
        }
    }

    public void PlayAttackSound()
    {
        if (audioSource != null && attackSoundClip != null)
        {
            audioSource.PlayOneShot(attackSoundClip);
        }
    }

    public bool IsSkillCoolingDown(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= currentSkillCooldowns.Length) return false;
        return currentSkillCooldowns[skillIndex] > 0f;
    }

    #endregion
}