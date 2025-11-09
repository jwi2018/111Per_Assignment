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
    //Skill4Active, // 버프라서 필요 없을 듯
    Skill5Active
}

public class Player : MonoBehaviour
{
    [Header("★ 상태")]
    private PlayerState _state = PlayerState.None;

    [Header("★ 이동")]
    [SerializeField] private float _baseMoveSpeed = 3.5f; // 기본 이동 속도 (Base 값으로)
    private float _currentMoveSpeed;                      // 현재 적용되는 이동 속도

    [Header("★ 공격")]
    [SerializeField] private bool _canShoot = true;          // 현재 발사 가능 여부
    [SerializeField] private float _baseShootCooldown = 0f; // 기본 공격 쿨다운 시간 (변하지 않는 고정값)
    private float _currentShootCooldown;                     // 현재 적용 중인 공격 쿨다운 (Skill1Active 시 변경됨)
    private float _shootTimer = 0f;                          // 공격 쿨다운 타이머
    [SerializeField] private float _arrowLaunchSpeed = 9.8f;  // 화살 발사 기본 속도 (Inspector에서 조절)

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
    public float MoveSpeed
    {
        get => _currentMoveSpeed;
        set => _currentMoveSpeed = value; // currentMoveSpeed를 외부에서 변경 가능하게 함
    }
    public float BaseMoveSpeed => _baseMoveSpeed; // 기본 이동 속도를 얻기 위한 속성

    public bool CanShoot => _canShoot;

    // 현재 적용 중인 공격 쿨다운 (ShootCooldown Property)
    // Getter는 현재 적용된 _currentShootCooldown 값을 반환하며, Setter로 변경 가능
    public float ShootCooldown
    {
        get => _currentShootCooldown;
        set => _currentShootCooldown = value;
    }

    // 변경되지 않는 기본 공격 쿨다운 값 (BaseShootCooldown Property)
    public float BaseShootCooldown => _baseShootCooldown;

    public float ShootTimer => _shootTimer; // 쿨다운 타이머의 남은 시간 (읽기 전용)
    public int CurrentHealth => _currentHealth; // 현재 체력 (읽기 전용)
    public float ArrowLaunchSpeed => _arrowLaunchSpeed; // 화살 발사 기본 속도 (읽기 전용)


    #endregion

    #region Unity 생명주기

    void Awake()
    {
        _currentHealth = _maxHealth;
        _currentMoveSpeed = _baseMoveSpeed;
        _currentShootCooldown = _baseShootCooldown;

        if (skillCooldownTimers == null || skillCooldownTimers.Length != skillCooldowns.Length)
        {
            skillCooldownTimers = new int[skillCooldowns.Length];
            skillDeltaTimeAccumulators = new float[skillCooldowns.Length];
        }
        for (int i = 0; i < skillCooldowns.Length; i++)
        {
            skillCooldownTimers[i] = 0; // 시작 시 모든 스킬 쿨다운은 0
            skillDeltaTimeAccumulators[i] = 0f;
        }

        SetPlayerState(_state);
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

       // Debug.Log("플레이어 업데이트 공격중 1 ");

        if (!_canShoot) // 발사 불가능 상태 (쿨다운 중)일 때
        {
            //Debug.Log("플레이어 업데이트 공격중 2 ");
            _shootTimer -= Time.deltaTime; // 타이머 감소
            if (_shootTimer <= 0f)
            {
               // Debug.Log("플레이어 업데이트 공격중 3 ");
                _shootTimer = 0f; // 음수 방지
                _canShoot = true; // 쿨다운 끝, 발사 가능
            }
        }
    }

    #endregion


    #region Public 함수

    /*
    public void SetPlayerState(PlayerState newState)
    {
        if (_state == newState) return;

        _state = newState;
    }*/

    public void ResetShootCooldown()
    {
        _canShoot = false;
        _shootTimer = _currentShootCooldown;
    }

    /*
    public void SetCanShoot(bool _bool)
    {
        Debug.Log("SetCanShoot  :  " + _bool);
        _canShoot = _bool;

        Debug.Log("SetCanShoot  _canShoot  :  " + _canShoot);
    }

    public void SetShootTimer(float Time)
    {
        _shootTimer -= Time;
    }
    */

    public void SetPlayerState(PlayerState newState)
    {
        // 동일한 상태로의 불필요한 전환 방지
        if (_state == newState) return;

        _state = newState;
        // Debug.Log($"Player State changed to: {_state}"); // 상태 변경 확인용

        // 애니메이터 파라미터 업데이트
        if (_animator != null)
        {
            // 모든 관련 Bool 파라미터를 일단 false로 초기화하고,
            // 현재 상태에 맞는 파라미터만 true로 설정합니다.
            _animator.SetBool("isMove", false);
            _animator.SetBool("isAttack", false);
            _animator.SetBool("Skill1", false); // 스킬 관련 파라미터도 여기에 포함 (스킬 상태 전용 애니메이션은 별도)
            _animator.SetBool("Skill2", false); // <-- 추가: Skill2 애니메이터 Bool 파라미터 초기화
            _animator.SetBool("Skill3", false); // <-- 추가: Skill2 애니메이터 Bool 파라미터 초기화
            _animator.SetBool("Skill5", false);

            switch (newState)
            {
                case PlayerState.Move:
                    _animator.SetBool("isMove", true);
                    break;
                case PlayerState.Attack:
                    // Attack 상태는 Update에서 지속적으로 발사되므로, 애니메이션도 켜둠
                    _animator.SetBool("isAttack", true);
                    break;
                case PlayerState.Skill1Active:
                    // Skill1Active는 Skill1 Bool 파라미터를 사용합니다.
                    _animator.SetBool("Skill1", true);
                    _animator.SetBool("isMove", false);
                    // 스킬 중에도 Attack 애니메이션이 오버레이되기를 원하면 여기서 isAttack도 true
                    // _animator.SetBool("isAttack", true); 
                    break;
                case PlayerState.Skill2Active: // <-- 추가: Skill2Active 상태 처리
                    _animator.SetBool("Skill2", true); // "Skill2" Bool 파라미터 활성화
                    _animator.SetBool("isMove", false);
                    break;
                case PlayerState.Skill3Active: // <-- 추가: Skill2Active 상태 처리
                    _animator.SetBool("Skill3", true); // "Skill2" Bool 파라미터 활성화
                    _animator.SetBool("isMove", false);
                    break;
                case PlayerState.Skill5Active: // <-- 추가: Skill5Active 상태 처리
                    _animator.SetBool("Skill5", true);
                    _animator.SetBool("isMove", false); // 방어막 생성 중에는 이동 애니메이션 중단
                    break;
            }
        }
    }


    // 스킬 사용 가능 여부 확인 및 쿨다운 시작
    public bool TryUseSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillCooldowns.Length) return false;

        if (skillCooldownTimers[skillIndex] <= 0)
        {
            // 스킬 쿨다운 시작
            skillCooldownTimers[skillIndex] = skillCooldowns[skillIndex];
            skillDeltaTimeAccumulators[skillIndex] = 0f;
            Debug.Log($"Skill {skillIndex + 1} activated! Cooldown: {skillCooldowns[skillIndex]}s");
            return true;
        }
        else
        {
            Debug.Log($"Skill {skillIndex + 1} is on cooldown. {skillCooldownTimers[skillIndex]}s remaining.");
            return false; // 쿨다운 중일 경우
        }
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

    // SkillCoolTime에서 스킬의 남은 쿨타임을 가져갈 수 있도록 함수 추가
    public int GetSkillRemainingCoolTime(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillCooldownTimers.Length)
        {
            Debug.LogError($"Invalid skillIndex: {skillIndex} in GetSkillRemainingCoolTime.");
            return 0;
        }
        return skillCooldownTimers[skillIndex];
    }

    #endregion
}
