using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    None,
    Idle,
    Move,
    Attack,
    Skill
}

public class Player : MonoBehaviour
{
    [Header("★ 상태")]
    private PlayerState _state = PlayerState.Idle;

    [Header("★ 이동")]
    private float _moveSpeed = 8.0f;

    [Header("★ 공격")]
    private bool _canShoot = true;
    private float _shootCooldown = 0.5f;
    private float _shootTimer = 0f;

    [Header("★ 체력")]
    private int _maxHealth = 100;
    private int _currentHealth;

    [Header("★ 스킬")]
    [SerializeField] private int[] skillCooldowns = new int[5];     // 스킬별 쿨다운 시간
    private int[] skillCooldownTimers = new int[5];                 // 스킬별 쿨다운 타이머
    private float[] skillDeltaTimeAccumulators = new float[5];

    [Header("★ 컴포넌트")]
    public Animator _animator;
    public PlayerInput _playerInput;

    #region Property

    public PlayerState State => _state;

    public float MoveSpeed => _moveSpeed;

    public bool CanShoot => _canShoot;

    public float ShootCooldown => _shootCooldown;

    public float ShootTimer => _shootTimer;


    #endregion

    #region Unity 생명주기

    void Awake()
    {
        _currentHealth = _maxHealth;
    }

    void Update()
    {
        for (int i = 0; i < skillCooldownTimers.Length; i++)
        {
            if (skillCooldownTimers[i] > 0)
            {
                skillDeltaTimeAccumulators[i] += Time.deltaTime; // 시간 누적
                if (skillDeltaTimeAccumulators[i] >= 1.0f) // 1초가 넘으면
                {
                    skillCooldownTimers[i]--; // 쿨다운 1초 감소
                    skillDeltaTimeAccumulators[i] -= 1.0f; // 1초를 빼고 남은 시간은 유지
                }
            }
        }

        if (!_canShoot) // 발사 불가능 상태 (쿨다운 중)일 때
        {
            _shootTimer -= Time.deltaTime; // 타이머 감소
            if (_shootTimer <= 0f)
            {
                _shootTimer = 0f; // 음수 방지
                _canShoot = true; // 쿨다운 끝, 발사 가능
            }
        }
    }

    #endregion


    #region Public 함수

    public void SetPlayerState(PlayerState newState)
    {
        if (_state == newState) return;

        _state = newState;
    }

    public void ResetShootCooldown()
    {
        _canShoot = false;
        _shootTimer = _shootCooldown;
    }

    public void SetCanShoot(bool _bool)
    {
        _canShoot = _bool;
    }

    public void SetShootTimer(float Time)
    {
        _shootTimer -= Time;
    }


    public bool UseSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillCooldowns.Length) return false;

        if (skillCooldownTimers[skillIndex] <= 0)
        {
            // 스킬 발동 처리 (프로젝트에 맞게 커스터마이징)
            _animator.SetTrigger($"Skill{skillIndex + 1}");
            skillCooldownTimers[skillIndex] = skillCooldowns[skillIndex];
            skillDeltaTimeAccumulators[skillIndex] = 0f;
            // 데미지 처리 등 추가 코드 삽입 가능
            return true;
        }
        return false; // 쿨다운 중일 경우
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            // 플레이어 사망 처리 로직
            Debug.Log("플레이어 사망!");
        }
    }

    #endregion
}
