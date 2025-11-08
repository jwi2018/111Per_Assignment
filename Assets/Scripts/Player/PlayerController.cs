using UnityEngine;
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

    #region Unity 생명주기

    void Awake()
    {
        _player.SetPlayerState(PlayerState.Attack);
        _player._animator.SetBool("isAttack", true);
        _player._animator.SetBool("isMove", false);
    }

    void Update()
    {
        if (_player.State == PlayerState.Attack && _player.CanShoot)
        {
            ShootArrow();
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

        if(_moveDirection.sqrMagnitude > 0.01f)
        {
            _player.SetPlayerState(PlayerState.Move);
            _player._animator.SetBool("isMove", true);
            _player._animator.SetBool("isAttack", false);

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
    }

    #endregion

    #region Public 함수

    #endregion

    #region Private 함수

    private void Move()
    {
        _rigidbody2D.linearVelocity = _moveDirection * _player.MoveSpeed;
    }

    private void ShootArrow()
    {
        if (_arrowPrefab == null || _firePoint == null) return;

        if (!_player.CanShoot) return; // 쿨다운 중이면 발사하지 않음

        _player._animator.SetBool("isAttack", true);
        _player._animator.SetBool("isMove", false);

        //PlayerState originalState = _player.State; // 원래 상태 기억
        //_player.SetPlayerState(PlayerState.Attack); // Attack 상태로 전환

        // 화살을 발사 위치에서 생성
        GameObject arrowInstance = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);

        //화살 스크립트에 발사 각도 및 속도 설정(예시, 화살에 Launch 함수 가정)
        BasicArrow arrowScript = arrowInstance.GetComponent<BasicArrow>();
        if (arrowScript != null)
        {
            arrowScript.Launch(50f);   // 50도 각도로 발사, 런치 속도는 Arrow 스크립트 내에서 설정
        }

        _player.ResetShootCooldown();
    }

    // 스프라이트 좌우 반전 함수
    private void FlipSprite(float moveX)
    {
        if (moveX > 0) // 오른쪽 이동
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveX < 0) // 왼쪽 이동
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    #endregion
}
