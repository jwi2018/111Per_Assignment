using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    [Header("★ 플레이어")]
    [SerializeField] Player _player;

    [Header("★ 이동")]
    [SerializeField] Vector2 _moveDirection { get; set; }

    [Header("★ 공격")]
    [SerializeField] Transform _firePoint;              // 화살 발사 위치 (빈 오브젝트)
    [SerializeField] GameObject _arrowPrefab;           // 화살 프리팹

    [Header("★ 컴포넌트")]
    [SerializeField] Rigidbody2D _rigidbody2D;

    // --- 스킬 1 (속사) 관련 변수 추가 ---
    [Header("★ 스킬 1 (속사)")]
    [SerializeField] private float _skill1Duration = 3.0f;          // 스킬 1 지속 시간
    // _skill1Cooldown은 Player.cs의 skillCooldowns 배열에 0번 인덱스로 관리됩니다.
    private Coroutine _skill1ActiveCoroutine;                       // 스킬 1 지속 코루틴 레퍼런스
    // --

    #region Unity 생명주기

    void Awake()
    {
        _player.SetPlayerState(PlayerState.Attack);

        _player._animator.SetBool("isAttack", true);
        _player._animator.SetBool("isMove", false);
        _player._animator.SetBool("Skill1", false); // 스킬 애니메이션 초기화
    }

    void Update()
    {
        if (_player.State == PlayerState.Attack && _player.CanShoot)
        {
            ShootArrow(false); // 일반 공격으로 호출
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    #endregion


    #region Input Action 함수

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();

        if (context.performed)
        {
            _player.SetPlayerState(PlayerState.Move);
            _player._animator.SetBool("isMove", true);
            _player._animator.SetBool("isAttack", false);
            _player._animator.SetBool("Skill1", false); // 스킬 1 사용 중이었으면 종료

            FlipSprite(_moveDirection.x);

            Debug.Log("이동 중입니다.");
        }
        else if(context.canceled)
        {
            _player.SetPlayerState(PlayerState.Attack);
            _player._animator.SetBool("isAttack", true);
            _player._animator.SetBool("isMove", false);
            _player._animator.SetBool("Skill1", false); // 스킬 1 사용 중이었으면 종료

            // 멈춤 상태일 때는 무조건 오른쪽 기준 바라보도록 설정 (localScale.x 양수)
            Vector3 scale = transform.localScale;
            if (scale.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(scale.x), scale.y, scale.z);
            }

            //_player._animator.SetFloat("AttackSpeed", 2.0f);
            //_player._animator.SetBool("isAttack", true);

            Debug.Log("이동 제외한 행동(Idle) 중입니다.");
        }
        /*
        _moveDirection = context.ReadValue<Vector2>();

        if (_moveDirection.sqrMagnitude > 0.01f)
        {
            _player.SetPlayerState(PlayerState.Move);
            _player._animator.SetBool("isMove", true);
            _player._animator.SetBool("isAttack", false);
            _player._animator.SetBool("Skill1", false); // 스킬 1 사용 중이었으면 종료

            FlipSprite(_moveDirection.x);

            Debug.Log("이동 중입니다.");
        }
        else // 이동 입력이 없다면 (키를 떼거나 아무것도 입력 안 할 때)
        {
            _player.SetPlayerState(PlayerState.Attack);

            // 멈춤 상태일 때는 무조건 오른쪽 기준 바라보도록 설정 (localScale.x 양수)
            Vector3 scale = transform.localScale;
            if (scale.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(scale.x), scale.y, scale.z);
            }

            //_player._animator.SetFloat("AttackSpeed", 2.0f);
            //_player._animator.SetBool("isAttack", true);

            Debug.Log("이동 제외한 행동(Idle) 중입니다.");
        }
        */
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
        PlayerState previousState = _player.State;

        // 플레이어 상태를 Skill1Active로 변경 및 애니메이터 Bool 파라미터 설정
        _player.SetPlayerState(PlayerState.Skill1Active);
        _player._animator.SetBool("isAttack", false);
        _player._animator.SetBool("isMove", false);
        _player._animator.SetBool("Skill1", true); // "Skill1" Bool 파라미터 활성화 (애니메이터에 이 이름으로 Bool이 있어야 함)

        // 현재 적용 중인 공격 쿨다운을 저장해두어 스킬 종료 후 원래대로 복구
        float originalCurrentShootCooldown = _player.ShootCooldown;
        // 속사 스킬: 기본 공격 쿨다운을 2배 빠르게 (즉, 절반으로 줄임)
        _player.ShootCooldown = _player.BaseShootCooldown / 2.0f;

        Debug.Log($"스킬 1(속사) 시작! 원래 공격 쿨다운: {originalCurrentShootCooldown:F2}초, 변경 후: {_player.ShootCooldown:F2}초");

        float skillTimer = _skill1Duration; // 스킬 지속 시간 타이머
        while (skillTimer > 0f)
        {
            // 스킬 활성화 기간 동안, 빨라진 쿨다운에 맞춰 화살을 발사
            if (_player.CanShoot)
            {
                ShootArrow(true); // 스킬 공격임을 알리는 플래그 전달
            }

            skillTimer -= Time.deltaTime; // 타이머 감소
            yield return null; // 다음 프레임까지 대기
        }

        // --- 스킬 종료 ---
        _player.ShootCooldown = originalCurrentShootCooldown; // 공격 쿨다운 원래대로 복구
        _player._animator.SetBool("Skill1", false); // 스킬 1 애니메이션 비활성화

        // 스킬 종료 후 플레이어 상태 복구
        // 스킬 사용 전 상태가 Move였다면 Idle로 복구, 아니면 원래 상태로 복구
        if (previousState == PlayerState.Move)
        {
            _player.SetPlayerState(PlayerState.Idle);
            // 이때 애니메이터도 Idle 상태로 돌아가도록 설정이 필요할 수 있습니다.
            _player._animator.SetBool("isMove", false);
        }
        else
        {
            _player.SetPlayerState(previousState);
        }

        Debug.Log("스킬 1(속사) 종료.");
        _skill1ActiveCoroutine = null; // 코루틴 레퍼런스 초기화 (다음 스킬 사용을 위해)
    }

    // ShootArrow 함수를 오버로드하여 스킬 공격 여부 플래그를 받을 수 있게 수정
    // 이 함수는 플레이어 상태에 따라 호출될 수도 있고, 스킬 코루틴에서 직접 호출될 수도 있습니다.
    private void ShootArrow(bool isSkillAttack)
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
            float launchSpeed = isSkillAttack ? _player.ArrowLaunchSpeed * 2f : _player.ArrowLaunchSpeed;
            arrowScript.Launch(50f, launchSpeed); // BasicArrow의 Launch 함수 호출 (각도 50도 고정)
        }

        // 일반 공격일 경우에만 쿨다운 리셋 (스킬 공격은 스킬 활성화 기간 동안 Player.ShootCooldown이 자동으로 짧아져 있음)
        if (!isSkillAttack)
        {
            _player.ResetShootCooldown();
        }
    }

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
