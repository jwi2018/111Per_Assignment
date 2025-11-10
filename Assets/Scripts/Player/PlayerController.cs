using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    [Header("★ 플레이어")]
    [SerializeField] Player _player;

    [Header("★ 이동")]
    [SerializeField] Vector2 _moveDirection { get; set; }
    [SerializeField] private float _attackTransitionDelay = 0.1f; // <-- 추가: Attack 상태 전환 딜레이

    [Header("★ 공격")]
    [SerializeField] Transform _firePoint;              // 화살 발사 위치 (빈 오브젝트)
    [SerializeField] GameObject _arrowPrefab;           // 화살 프리팹

    [Header("★ 컴포넌트")]
    [SerializeField] Rigidbody2D _rigidbody2D;

    // --- 스킬 1 (속사) 관련 변수 추가 ---
    [Header("★ 스킬 1 (속사)")]
    [SerializeField] private float _skill1Duration = 3.0f;          // 스킬 1 지속 시간
    [SerializeField] private float _skill1ShootInterval = 0.2f;     // <--- 스킬 발사 간격 (Inspector에서 조절)
    private Coroutine _skill1ActiveCoroutine;                       // 스킬 1 지속 코루틴 레퍼런스
    // --

    // --- 스킬 2 (10발 동시 발사) 관련 변수 ---
    [Header("★ 스킬 2 (동시 발사)")]
    [SerializeField] private int _skill2ArrowCount = 10;            // 동시에 발사할 화살 개수
    [SerializeField] private float _skill2SpreadAngle = 30f;        // 화살이 퍼지는 총 각도 (예: 30도)
    [SerializeField] private float _skill2LaunchSpeedModifier = 1f; // 스킬 2 발사 속도 배율 (예: 0.8배)
    // ------------------------------------

    // --- 스킬 3 (불화살) 관련 변수 ---
    [Header("★ 스킬 3 (불화살)")]
    public GameObject fireArrowPrefab; // <-- 수정: 불화살 "타입" 프리팹 (Inspector에서 연결)
    public GameObject groundFireEffectPrefab; // <-- 추가: 지면에 생성될 불길 이펙트 프리팹 (Inspector에서 연결)
    [SerializeField] private float _skill3LaunchSpeedModifier = 1.0f; // 스킬 3 발사 속도 배율 (예: 1.0배)
    // ------------------------------------

    // --- 스킬 4 (이동 속도 증가) 관련 변수 ---
    [Header("★ 스킬 4 (이동 속도 증가)")]
    [SerializeField] private float _skill4Duration = 6.0f;          // 스킬 4 지속 시간
    [SerializeField] private float _skill4SpeedMultiplier = 1.5f;   // 이동 속도 배율
    private Coroutine _skill4ActiveCoroutine;                       // 스킬 4 지속 코루틴 레퍼런스

    // --- 스킬 5 (방어막 생성) 관련 변수 ---
    [Header("★ 스킬 5 (방어막 생성)")]
    public GameObject shieldPrefab;         // <-- 방패 프리팹 (Shield.cs가 붙어있는)
    [SerializeField] private float _skill5ShieldDuration = 6.0f; // 방패 지속 시간
    [SerializeField] private int _skill5ShieldHealth = 100;      // 방패 체력

    #region Unity 생명주기

    void Awake()
    {
        _player.SetPlayerState(PlayerState.Attack);
    }

    void FixedUpdate()
    {
        Move(_moveDirection);
    }

    #endregion


    #region Input Action 함수

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if(_player.State == PlayerState.Skill1Active 
            || _player.State == PlayerState.Skill2Active 
            || _player.State == PlayerState.Skill3Active 
            || _player.State == PlayerState.Skill5Active)
        {
            return;
        }

        _moveDirection = context.ReadValue<Vector2>();

        if (context.performed)
        {
            _player.SetPlayerState(PlayerState.Move);

            FlipSprite(_moveDirection.x);
        }
        else if(context.canceled)
        {
            _player.SetPlayerState(PlayerState.Attack);

            Vector3 scale = transform.localScale;
            if (scale.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(scale.x), scale.y, scale.z);
            }
        }
    }

    // --- 스킬 1 (속사) 입력 처리 함수 추가 ---
    public void OnSkill1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_player.TryUseSkill(0) && _player.State != PlayerState.Skill1Active)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _moveDirection = Vector2.zero;

                Vector3 currentScale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);

                if (_skill1ActiveCoroutine != null)
                {
                    StopCoroutine(_skill1ActiveCoroutine);
                }

                _skill1ActiveCoroutine = StartCoroutine(Skill1ActiveCoroutine());
            }
        }
    }

    // --- 스킬 2 (10발 동시 발사) 입력 처리 함수 ---
    public void OnSkill2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_player.State == PlayerState.Skill1Active)
                return;

            if (_player.TryUseSkill(1) && _player.State != PlayerState.Skill1Active && _player.State != PlayerState.Skill2Active)
            {
                _rigidbody2D.linearVelocity = Vector2.zero; 
                _moveDirection = Vector2.zero;

                Vector3 currentScale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);

                _player.SetPlayerState(PlayerState.Skill2Active);
            }
        }
    }

    public void OnSkill3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_player.State == PlayerState.Skill1Active)
                return;

            if (_player.TryUseSkill(2) && _player.State != PlayerState.Skill1Active && _player.State != PlayerState.Skill2Active && _player.State != PlayerState.Skill3Active)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _moveDirection = Vector2.zero;
                Vector3 currentScale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);

                _player.SetPlayerState(PlayerState.Skill3Active);
            }
        }
    }

    public void OnSkill4(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_player.State == PlayerState.Skill1Active)
                return;

            if (_player.TryUseSkill(3) && _player.State != PlayerState.Skill1Active && _player.State != PlayerState.Skill2Active && _player.State != PlayerState.Skill3Active)
            {
                if (_skill4ActiveCoroutine != null) StopCoroutine(_skill4ActiveCoroutine);

                _skill4ActiveCoroutine = StartCoroutine(Skill4ActiveCoroutine());
            }
        }
    }

    public void OnSkill5(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_player.State == PlayerState.Skill1Active)
                return;

            if (_player.TryUseSkill(4) && _player.State != PlayerState.Skill1Active && _player.State != PlayerState.Skill2Active && _player.State != PlayerState.Skill3Active && _player.State != PlayerState.Skill5Active)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _moveDirection = Vector2.zero;
                Vector3 currentScale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);

                _player.SetPlayerState(PlayerState.Skill5Active);
            }
        }
    }

    #endregion

    #region Public 함수

    private void PlayHitSound()
    {
        if (_player.audioSource != null && _player.attackSoundClip != null)
        {
            _player.audioSource.PlayOneShot(_player.attackSoundClip);
        }
    }

    #endregion

    #region Private 함수

    private void Move(Vector2 currentMoveDirection)
    {
        _rigidbody2D.linearVelocity = currentMoveDirection * _player.MoveSpeed;
    }

    private IEnumerator Skill1ActiveCoroutine()
    {
        _player.SetPlayerState(PlayerState.Skill1Active);

        float originalCurrentShootCooldown = _player.ShootCooldown;
        float skillTimer = _skill1Duration; // 스킬 지속 시간 타이머
        float timer = 0f; // 스킬 지속 시간 확인용 타이머 (skillTimer와 같은 역할)

        while (timer < _skill1Duration)
        {

            yield return new WaitForSeconds(_skill1ShootInterval);

            timer += _skill1ShootInterval;
        }

        _player.SetPlayerState(PlayerState.Attack);
        _skill1ActiveCoroutine = null;
    }

    private IEnumerator Skill4ActiveCoroutine()
    {
        float originalMoveSpeed = _player.MoveSpeed; // 현재 이동 속도 저장
        _player.MoveSpeed = originalMoveSpeed * _skill4SpeedMultiplier; // 이동 속도 증가

        float timer = 0f;
        while (timer < _skill4Duration)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        _player.MoveSpeed = originalMoveSpeed;

        _skill4ActiveCoroutine = null;
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

        if (!false)
        {
            _player.ResetShootCooldown();
        }
    }

    public void AnimationEvent_ShootArrow_Skill1()
    {
        if (_arrowPrefab == null || _firePoint == null) return;
        if (!true && !_player.CanShoot) return;

        GameObject arrowInstance = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);

        BasicArrow arrowScript = arrowInstance.GetComponent<BasicArrow>();
        if (arrowScript != null)
        {
            float launchSpeed = _player.ArrowLaunchSpeed * 1.1f;
            arrowScript.LaunchSkill1(30f, 65f, launchSpeed);

            PlayHitSound();
        }
    }

    public void AnimationEvent_ShootArrow_Skill2()
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

            if (transform.localScale.x < 0)
            {
                currentAngle = 180f - currentAngle;
            }

            GameObject arrowInstance = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);
            BasicArrow arrowScript = arrowInstance.GetComponent<BasicArrow>();
            if (arrowScript != null)
            {
                arrowScript.Launch(currentAngle, launchSpeed);

                PlayHitSound();
            }
        }

        _player.SetPlayerState(PlayerState.Attack);
    }

    public void AnimationEvent_ShootArrow_Skill3()
    {
        if (fireArrowPrefab == null || _firePoint == null) return;
        if (_player.State != PlayerState.Skill3Active) return;

        float launchSpeed = _player.ArrowLaunchSpeed * _skill3LaunchSpeedModifier;
        float baseAngle = 50f; 
        float currentAngle = baseAngle;
        if (transform.localScale.x < 0)
        {
            currentAngle = 180f - currentAngle;
        }

        GameObject arrowInstance = Instantiate(fireArrowPrefab, _firePoint.position, _firePoint.rotation);

        FireArrow fireArrowScript = arrowInstance.GetComponent<FireArrow>();
        if (fireArrowScript != null)
        {
            fireArrowScript.fireEffectPrefab = groundFireEffectPrefab;
            fireArrowScript.Launch(currentAngle, launchSpeed);

            PlayHitSound();
        }

        _player.SetPlayerState(PlayerState.Attack);
    }

    public void AnimationEvent_SpawnShield()
    {
        if (shieldPrefab == null || _player == null) return; 
        if (_player.State != PlayerState.Skill5Active) return; 

        Vector3 spawnPosition = transform.position;
        GameObject shieldInstance = Instantiate(shieldPrefab, spawnPosition, Quaternion.identity);

        Shield shieldScript = shieldInstance.GetComponent<Shield>();
        if (shieldScript != null)
        {
            shieldScript.SetShieldProperties(_skill5ShieldHealth, _skill5ShieldDuration);
        }

        if (_moveDirection.sqrMagnitude > 0.01f)
        {
            _player.SetPlayerState(PlayerState.Move);
        }
        else
        {
            _player.SetPlayerState(PlayerState.Attack);
        }
    }

    private void FlipSprite(float moveX)
    {
        if (moveX == 0) return;

        Vector3 currentScale = transform.localScale;
        if (moveX > 0 && currentScale.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else if (moveX < 0 && currentScale.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    #endregion
}
