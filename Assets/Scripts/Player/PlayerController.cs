using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    [Header("★ 플레이어")]
    [SerializeField] Player _player;

    [Header("★ 이동")]
    [SerializeField] Vector2 _moveDirection { get; set; }
    [SerializeField] private float _attackTransitionDelay = 0.1f; // <-- 추가: Attack 상태 전환 딜레이
    private Coroutine _attackStateTransitionCoroutine; // <-- 추가: 딜레이 코루틴 레퍼런스

    [Header("★ 공격")]
    [SerializeField] Transform _firePoint;              // 화살 발사 위치 (빈 오브젝트)
    [SerializeField] GameObject _arrowPrefab;           // 화살 프리팹

    [Header("★ 컴포넌트")]
    [SerializeField] Rigidbody2D _rigidbody2D;

    // --- 스킬 1 (속사) 관련 변수 추가 ---
    [Header("★ 스킬 1 (속사)")]
    [SerializeField] private float _skill1Duration = 3.0f;          // 스킬 1 지속 시간
    // _skill1Cooldown은 Player.cs의 skillCooldowns 배열에 0번 인덱스로 관리됩니다.
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

    #region Unity 생명주기

    void Awake()
    {
        _player.SetPlayerState(PlayerState.Attack);
    }

    void FixedUpdate()
    {
        Move();
    }

    #endregion


    #region Input Action 함수

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if(_player.State == PlayerState.Skill1Active 
            || _player.State == PlayerState.Skill2Active 
            || _player.State == PlayerState.Skill3Active 
            || _player.State == PlayerState.Skill4Active
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
        if (context.performed) // 1번 키를 눌렀을 때
        {
            // Player.cs의 TryUseSkill을 사용하여 쿨다운 체크 및 시작
            // skillIndex 0번이 스킬 1이라고 가정합니다. (Inspector의 skillCooldowns 배열에서 첫 번째 스킬)
            if (_player.TryUseSkill(0) && _player.State != PlayerState.Skill1Active)
            {
                _rigidbody2D.linearVelocity = Vector2.zero; // 캐릭터 속도 0
                _moveDirection = Vector2.zero;       // 이동 입력 초기화

                // --- 스프라이트 방향 오른쪽으로 고정 ---
                Vector3 currentScale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);

                // 기존의 스킬 1 코루틴이 있다면 중지 (중복 실행 방지)
                if (_skill1ActiveCoroutine != null)
                {
                    StopCoroutine(_skill1ActiveCoroutine);
                }
                // 스킬 1 활성화 코루틴 시작
                _skill1ActiveCoroutine = StartCoroutine(Skill1ActiveCoroutine());
                Debug.Log("스킬 1(속사) 발동!");
            }
            // else 문은 Player.cs의 TryUseSkill에서 Debug.Log로 처리됨
        }
    }

    // --- 스킬 2 (10발 동시 발사) 입력 처리 함수 ---
    public void OnSkill2(InputAction.CallbackContext context)
    {
        if (context.performed) // 2번 키를 눌렀을 때
        {
            // _player.TryUseSkill(1)을 통해 쿨다운 체크 및 시작
            // 현재 다른 스킬이 활성화 중이 아닐 때만 발동
            if (_player.TryUseSkill(1) && _player.State != PlayerState.Skill1Active && _player.State != PlayerState.Skill2Active)
            {
                _rigidbody2D.linearVelocity = Vector2.zero; // 캐릭터 속도 0
                _moveDirection = Vector2.zero;       // 이동 입력 초기화

                // --- 스프라이트 방향 오른쪽으로 고정 ---
                Vector3 currentScale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);

                /*
                // Attack 상태 전환 딜레이 코루틴이 실행 중이면 스킬 발동 시 중단
                if (_attackStateTransitionCoroutine != null)
                {
                    StopCoroutine(_attackStateTransitionCoroutine);
                    _attackStateTransitionCoroutine = null;
                }*/

                // 플레이어 상태를 Skill2Active로 변경 (애니메이션이 재생될 것)
                _player.SetPlayerState(PlayerState.Skill2Active);
                Debug.Log("스킬 2(10발 동시 발사) 발동! (애니메이션 이벤트 대기)");

                // 단발성 스킬이므로 애니메이션 이벤트가 끝나면 원래 상태로 복구 (다음 스킬 발동 전까지 기다릴 필요 X)
                // 현재 이동 중이면 Move, 아니면 Attack 상태로 복구
                // 하지만 애니메이션이 끝나야 복구되므로, 스킬 애니메이션 클립의 마지막에 이벤트를 추가하는 게 좋습니다.
                // 일단 여기서는 스킬 발동 후 바로 기본 상태로 돌려놓겠습니다.
                // 더 정확한 복구를 위해 아래 `AnimationEvent_Skill2End()` 함수를 스킬 애니메이션 마지막에 연결하세요.
                // RestorePlayerStateAfterSkill2(_moveDirection.sqrMagnitude > 0.01f ? PlayerState.Move : PlayerState.Attack);
            }
        }
    }

    public void OnSkill3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_player.TryUseSkill(2) && _player.State != PlayerState.Skill1Active && _player.State != PlayerState.Skill2Active && _player.State != PlayerState.Skill3Active) // skillIndex 2번이 스킬 3
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _moveDirection = Vector2.zero;
                Vector3 currentScale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);

                _player.SetPlayerState(PlayerState.Skill3Active);
                Debug.Log("스킬 3(불화살) 발동! (애니메이션 이벤트 대기)");
            }
        }
    }

    #endregion

    #region Public 함수

    #endregion

    #region Private 함수

    private void Move()
    {
        _rigidbody2D.linearVelocity = _moveDirection * _player.MoveSpeed;
    }

    // --- 스킬 1 활성화 중 화살 발사 코루틴 ---
    private IEnumerator Skill1ActiveCoroutine()
    {
        // 스킬 사용 전 플레이어 상태를 기억해 두어 스킬 종료 후 복구할 때 사용
        //PlayerState previousState = _player.State;

        // 플레이어 상태를 Skill1Active로 변경 및 애니메이터 Bool 파라미터 설정
        _player.SetPlayerState(PlayerState.Skill1Active);

        /*
        _player._animator.SetBool("isAttack", false);
        _player._animator.SetBool("isMove", false);
        _player._animator.SetBool("Skill1", true); // "Skill1" Bool 파라미터 활성화 (애니메이터에 이 이름으로 Bool이 있어야 함)
        */

        // 현재 적용 중인 공격 쿨다운을 저장해두어 스킬 종료 후 원래대로 복구
        float originalCurrentShootCooldown = _player.ShootCooldown;



        // 속사 스킬: 기본 공격 쿨다운을 2배 빠르게 (즉, 절반으로 줄임)
        //_player.ShootCooldown = _player.BaseShootCooldown / 2.0f;

        Debug.Log($"스킬 1(속사) 시작! 원래 공격 쿨다운: {originalCurrentShootCooldown:F2}초, 변경 후: {_player.ShootCooldown:F2}초");

        float skillTimer = _skill1Duration; // 스킬 지속 시간 타이머
        float timer = 0f; // 스킬 지속 시간 확인용 타이머 (skillTimer와 같은 역할)



        while (timer < _skill1Duration) // 지정된 _skill1Duration 만큼 루프 반복
        {
            //ShootArrow(true); // 스킬 공격 발사

            yield return new WaitForSeconds(_skill1ShootInterval); // <--- 여기! _skill1ShootInterval 만큼 대기

            timer += _skill1ShootInterval; // 타이머 업데이트 (정확도를 위해 대기 시간만큼 더함)
            // 만약 _skill1ShootInterval보다 실제 지나간 시간이 더 정확하다면,
            // timer += (yield return new WaitForSeconds(_skill1ShootInterval)의 실제 경과 시간); 
            // 또는 timer += Time.deltaTime; 으로 계속 누적
            // 하지만 이 경우는 _skill1ShootInterval이 fixed된 간격이므로 이렇게 하는 것이 명확합니다.
        }

        // --- 스킬 종료 ---
        //_player.ShootCooldown = originalCurrentShootCooldown; // 공격 쿨다운 원래대로 복구

        _player.SetPlayerState(PlayerState.Attack);

        
        //_player._animator.SetBool("Skill1", false); // 스킬 1 애니메이션 비활성화

        //// 스킬 종료 후 플레이어 상태 복구
        //// 스킬 사용 전 상태가 Move였다면 Idle로 복구, 아니면 원래 상태로 복구
        //if (previousState == PlayerState.Move)
        //{
        //    _player.SetPlayerState(PlayerState.Attack);
        //    // 이때 애니메이터도 Idle 상태로 돌아가도록 설정이 필요할 수 있습니다.
        //    /*
        //    _player._animator.SetBool("isMove", false);
        //    */
        //}
        //else
        //{
        //    _player.SetPlayerState(previousState);
        //}

        Debug.Log("스킬 1(속사) 종료.");
        _skill1ActiveCoroutine = null; // 코루틴 레퍼런스 초기화 (다음 스킬 사용을 위해)
    }

    // ShootArrow 함수를 오버로드하여 스킬 공격 여부 플래그를 받을 수 있게 수정
    // 이 함수는 플레이어 상태에 따라 호출될 수도 있고, 스킬 코루틴에서 직접 호출될 수도 있습니다.
    public void ShootArrow(bool isSkillAttack = false)
    {
        if (_arrowPrefab == null || _firePoint == null) return;

        // 일반 공격이면서 쿨다운 중이라면 발사하지 않음 (스킬 공격은 Player.cs에서 쿨다운이 이미 조절되었으므로 이곳에서 재체크 필요 없음)
        if (!isSkillAttack && !_player.CanShoot) return;

        // 화살 프리팹을 발사 위치에서 생성
        GameObject arrowInstance = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);

        BasicArrow arrowScript = arrowInstance.GetComponent<BasicArrow>();
        if (arrowScript != null)
        {
            // 발사 속도 설정: 스킬 공격일 경우 Player.ArrowLaunchSpeed의 2배, 아니면 기본 속도 사용
            float launchSpeed = isSkillAttack ? _player.ArrowLaunchSpeed * 1.2f : _player.ArrowLaunchSpeed;
            arrowScript.Launch(50f, launchSpeed); // BasicArrow의 Launch 함수 호출 (각도 50도 고정)
        }

        // 일반 공격일 경우에만 쿨다운 리셋 (스킬 공격은 스킬 활성화 기간 동안 Player.ShootCooldown이 자동으로 짧아져 있음)
        if (!isSkillAttack)
        {
            _player.ResetShootCooldown();
        }
    }

    // 애니메이션에 직접 붙어있는 함수.... 매우 중요!!! 삭제 ㄴㄴㄴㄴㄴㄴㄴ
    public void AnimationEvent_ShootArrow()
    {
        if (_arrowPrefab == null || _firePoint == null) return;

        // 일반 공격이면서 쿨다운 중이라면 발사하지 않음 (스킬 공격은 Player.cs에서 쿨다운이 이미 조절되었으므로 이곳에서 재체크 필요 없음)
        if (!false && !_player.CanShoot) return;

        // 화살 프리팹을 발사 위치에서 생성
        GameObject arrowInstance = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);

        BasicArrow arrowScript = arrowInstance.GetComponent<BasicArrow>();
        if (arrowScript != null)
        {
            // 발사 속도 설정: 스킬 공격일 경우 Player.ArrowLaunchSpeed의 2배, 아니면 기본 속도 사용
            float launchSpeed = false ? _player.ArrowLaunchSpeed * 1.2f : _player.ArrowLaunchSpeed;
            arrowScript.Launch(50f, launchSpeed); // BasicArrow의 Launch 함수 호출 (각도 50도 고정)
        }

        // 일반 공격일 경우에만 쿨다운 리셋 (스킬 공격은 스킬 활성화 기간 동안 Player.ShootCooldown이 자동으로 짧아져 있음)
        if (!false)
        {
            _player.ResetShootCooldown();
        }
    }

    // 애니메이션에 직접 붙어있는 함수.... 매우 중요!!! 삭제 ㄴㄴㄴㄴㄴㄴㄴ
    public void AnimationEvent_ShootArrow_Skill1()
    {
        if (_arrowPrefab == null || _firePoint == null) return;

        // 일반 공격이면서 쿨다운 중이라면 발사하지 않음 (스킬 공격은 Player.cs에서 쿨다운이 이미 조절되었으므로 이곳에서 재체크 필요 없음)
        if (!true && !_player.CanShoot) return;

        // 화살 프리팹을 발사 위치에서 생성
        GameObject arrowInstance = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);

        BasicArrow arrowScript = arrowInstance.GetComponent<BasicArrow>();
        if (arrowScript != null)
        {
            // 발사 속도 설정: 스킬 공격일 경우 Player.ArrowLaunchSpeed의 2배, 아니면 기본 속도 사용
            float launchSpeed = true ? _player.ArrowLaunchSpeed * 1.1f : _player.ArrowLaunchSpeed;
            arrowScript.LaunchSkill1(30f, 65f, launchSpeed); // BasicArrow의 Launch 함수 호출 (각도 50도 고정)
        }
    }

    // --- 스킬 2 애니메이션 이벤트 함수 (10발 동시 발사) ---
    public void AnimationEvent_ShootArrow_Skill2()
    {
        if (_arrowPrefab == null || _firePoint == null) return;

        // 현재 _player.State가 Skill2Active일 때만 발동되도록 안전장치
        if (_player.State != PlayerState.Skill2Active) return;

        float baseAngle = 50f; // 중심 발사 각도 (캐릭터가 오른쪽을 보고 있다고 가정)
        float totalSpread = _skill2SpreadAngle; // 총 퍼지는 각도 (예: 30도)
        int arrowCount = _skill2ArrowCount;     // 발사할 화살 개수 (예: 10개)
        float launchSpeed = _player.ArrowLaunchSpeed * _skill2LaunchSpeedModifier; // 스킬 2 발사 속도 배율 적용

        // 시작 각도 (퍼지는 각도의 중앙을 기준으로 - 절반만큼 이동)
        float startAngle = baseAngle - (totalSpread / 2f);

        // 화살이 1개일 경우 각도 간격은 0, 2개 이상일 경우 계산
        float angleStep = (arrowCount > 1) ? totalSpread / (arrowCount - 1) : 0f;

        for (int i = 0; i < arrowCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);

            // 현재 캐릭터의 방향 (localScale.x)에 따라 발사 각도를 조절
            // 캐릭터가 왼쪽을 보고 있다면 (X 스케일이 음수) 발사 각도를 수평 기준으로 반전
            if (transform.localScale.x < 0)
            {
                currentAngle = 180f - currentAngle;
            }

            GameObject arrowInstance = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);
            BasicArrow arrowScript = arrowInstance.GetComponent<BasicArrow>();
            if (arrowScript != null)
            {
                arrowScript.Launch(currentAngle, launchSpeed);
            }
        }
        // 스킬 2는 단발성 발사이므로 ResetShootCooldown은 필요 없습니다.
        // 스킬 애니메이션이 끝나면 원래 상태로 복구하는 함수를 호출합니다.
        // 이 함수를 스킬 애니메이션의 마지막 프레임에 애니메이션 이벤트로 등록하는 것이 가장 이상적입니다.
        // 여기에 직접 호출할 경우, 발사 후 즉시 상태가 복구되어 애니메이션이 제대로 끝까지 재생되지 않을 수 있습니다.
        _player.SetPlayerState(PlayerState.Attack);
    }

    // --- 스킬 3 애니메이션 이벤트 함수 (불화살) 추가 ---
    public void AnimationEvent_ShootArrow_Skill3()
    {
        if (fireArrowPrefab == null || _firePoint == null) return; // <-- fireArrowPrefab (불화살 전용 프리팹) 사용
        if (_player.State != PlayerState.Skill3Active) return;

        float launchSpeed = _player.ArrowLaunchSpeed * _skill3LaunchSpeedModifier; // 스킬 3 발사 속도 배율 적용
        float baseAngle = 50f; // 불화살의 기본 발사 각도

        // 현재 캐릭터의 방향에 따라 발사 각도를 조절
        float currentAngle = baseAngle;
        if (transform.localScale.x < 0) // 캐릭터가 왼쪽을 보고 있다면
        {
            currentAngle = 180f - currentAngle;
        }

        // 불화살 프리팹을 발사 위치에서 생성
        GameObject arrowInstance = Instantiate(fireArrowPrefab, _firePoint.position, _firePoint.rotation); // <-- fireArrowPrefab 사용

        FireArrow fireArrowScript = arrowInstance.GetComponent<FireArrow>(); // <-- BasicArrow 대신 FireArrow로 GetComponent
        if (fireArrowScript != null)
        {
            // FireArrow 스크립트에 fireEffectPrefab 정보를 전달
            fireArrowScript.fireEffectPrefab = groundFireEffectPrefab;
            fireArrowScript.Launch(currentAngle, launchSpeed);
        }

        _player.SetPlayerState(PlayerState.Attack);
    }
    // ------------------------------------


    // 스프라이트 좌우 반전 함수
    private void FlipSprite(float moveX)
    {
        if (moveX == 0) return; // 0이면 처리 안함 (Input System은 0 값도 계속 보낼 수 있음)

        Vector3 currentScale = transform.localScale;
        if (moveX > 0 && currentScale.x < 0) // 오른쪽으로 가는데 현재 왼쪽을 보고 있다면
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else if (moveX < 0 && currentScale.x > 0) // 왼쪽으로 가는데 현재 오른쪽을 보고 있다면
        {
            transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    #endregion
}
