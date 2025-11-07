using UnityEngine;
using System.Collections; // Coroutine을 사용하기 위해 필요

[RequireComponent(typeof(Enemy), typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("★ 적군 기본 정보")]
    [SerializeField] private Enemy _enemy; // Enemy 스탯/상태 관리 스크립트

    [Header("★ 컴포넌트")]
    [SerializeField] private Rigidbody2D _rigidbody2D;

    [Header("★ 이동 설정")]
    [SerializeField] private float _patrolMoveDirection = 1f; // 순찰 방향 (1f:오른쪽, -1f:왼쪽)
    [SerializeField] private float _minXBound = -8f; // 적군이 이동할 수 있는 최소 X 좌표
    [SerializeField] private float _maxXBound = 8f;  // 적군이 이동할 수 있는 최대 X 좌표

    [Header("★ 낭떠러지 감지 설정")]
    [SerializeField] private LayerMask _whatIsGround; // 'Ground' 또는 'Platform' 레이어 설정
    [SerializeField] private Transform _edgeCheck;    // 낭떠러지 감지용 빈 오브젝트의 Transform
    [SerializeField] private float _edgeCheckDistance = 0.5f; // 낭떠러지 감지 거리

    [Header("★ AI 행동 설정")]
    [SerializeField] private float _detectPlayerRange = 7f; // 플레이어 탐지 범위
    [SerializeField] private float _attackRange = 5f;       // 플레이어 공격 가능 범위
    [SerializeField] private float _attackDurationMin = 1.0f; // 공격 행동 지속 최소 시간 (멈춰서 공격)
    [SerializeField] private float _attackDurationMax = 3.0f; // 공격 행동 지속 최대 시간
    [SerializeField] private float _patrolDurationMin = 2.0f; // 순찰 행동 지속 최소 시간
    [SerializeField] private float _patrolDurationMax = 5.0f; // 순찰 행동 지속 최대 시간

    // 공격 페이즈 종료 후 순찰로 전환될 확률 (확률이 높을수록 이동 선호)
    [SerializeField][Range(0f, 1f)] private float _postAttackPatrolChance = 0.8f;

    private Transform _playerTransform; // 탐지된 플레이어의 트랜스폼
    private Coroutine _aiBehaviorCoroutine; // AI 행동 코루틴 레퍼런스

    // 디버그용 플래그
    // private bool _isPatrolling = false; 
    // private bool _isAttacking = false;

    #region Unity 생명주기

    void Awake()
    {
        if (_enemy == null) _enemy = GetComponent<Enemy>();
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();

        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rigidbody2D.gravityScale = 3f;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }

        // AI 행동 코루틴 시작
        _aiBehaviorCoroutine = StartCoroutine(AIBehaviorCoroutine());
    }

    void OnDisable() // 오브젝트 비활성화 시 코루틴 중지
    {
        if (_aiBehaviorCoroutine != null)
        {
            StopCoroutine(_aiBehaviorCoroutine);
        }
    }

    void FixedUpdate()
    {
        // 적군이 죽은 상태면 모든 행동 중단
        if (_enemy.CurrentHealth <= 0)
        {
            if (_aiBehaviorCoroutine != null) StopCoroutine(_aiBehaviorCoroutine);
            _enemy.SetEnemyState(EnemyState.None);
            _rigidbody2D.linearVelocity = Vector2.zero;
            return;
        }

        // 경계 감지는 모든 이동 관련 상태에서 필요
        if (_enemy.State == EnemyState.Move)
        {
            CheckForBoundsAndEdges();
        }

        MoveEnemy(); // 이동 상태에 따라 실제로 움직임

        // 공격 상태일 때 플레이어 바라보며 자동 공격
        if (_enemy.State == EnemyState.Attack)
        {
            if (_playerTransform != null)
            {
                FacePlayer();
            }

            if (_enemy.CanAttack)
            {
                AttackPlayer();
            }
        }
    }
    #endregion

    #region AI 행동 코루틴

    IEnumerator AIBehaviorCoroutine()
    {
        // 무한루프: 적군이 죽을 때까지 행동을 반복
        while (true)
        {
            // 플레이어 탐지 여부 (범위 내에 있을 경우)
            bool isPlayerDetected = (_playerTransform != null && Vector2.Distance(transform.position, _playerTransform.position) <= _detectPlayerRange);
            bool isPlayerInAttackRange = (_playerTransform != null && Vector2.Distance(transform.position, _playerTransform.position) <= _attackRange);

            // 우선 순위 1: 플레이어가 공격 범위 내에 있으면 공격
            if (isPlayerDetected && isPlayerInAttackRange)
            {
                yield return StartCoroutine(AttackState());

                // 공격 후 일정 확률로 강제 순찰 (공격만 계속하는 것을 방지)
                if (Random.value < _postAttackPatrolChance)
                {
                    yield return StartCoroutine(PatrolState()); // 순찰 후 다시 행동 결정
                }
            }
            else // 우선 순위 2: 플레이어가 감지되지 않거나 공격 범위 밖에 있으면 순찰
            {
                yield return StartCoroutine(PatrolState());
            }

            // 한 사이클의 행동이 끝났으므로 다음 행동까지 약간의 텀 (선택 사항)
            yield return null; // 다음 프레임까지 대기하여 불필요한 즉시 반복 방지
        }
    }

    // 공격 상태 코루틴
    IEnumerator AttackState()
    {
        _enemy.SetEnemyState(EnemyState.Attack);
        // _isAttacking = true; // 디버그용
        float attackDuration = Random.Range(_attackDurationMin, _attackDurationMax);
        Debug.Log($"Enemy: 공격 시작! ({attackDuration:F2}초 동안)");

        // 공격 상태 지속 시간만큼 대기 (이 시간 동안 FixedUpdate에서 공격 실행)
        yield return new WaitForSeconds(attackDuration);

        // _isAttacking = false; // 디버그용
        Debug.Log("Enemy: 공격 종료.");
    }

    // 순찰 상태 코루틴
    IEnumerator PatrolState()
    {
        _enemy.SetEnemyState(EnemyState.Move);
        // _isPatrolling = true; // 디버그용
        float patrolDuration = Random.Range(_patrolDurationMin, _patrolDurationMax);
        Debug.Log($"Enemy: 순찰 시작! ({patrolDuration:F2}초 동안)");

        // 순찰 방향 랜덤으로 설정
        _patrolMoveDirection = (Random.value > 0.5f) ? 1f : -1f;
        FlipEnemySprite(); // 스프라이트 반전

        // 순찰 상태 지속 시간만큼 대기 (이 시간 동안 FixedUpdate에서 이동 실행)
        yield return new WaitForSeconds(patrolDuration);

        // _isPatrolling = false; // 디버그용
        Debug.Log("Enemy: 순찰 종료.");
    }

    #endregion

    #region Private 기타 함수 (CheckForBoundsAndEdges, MoveEnemy, AttackPlayer, FacePlayer, FlipEnemySprite)

    // CheckForBoundsAndEdges 함수 (낭떠러지 감지 시 방향 전환)
    private void CheckForBoundsAndEdges()
    {
        // 1. 맵 가장자리 X축 좌표 제한 검사
        float targetNextPosX = transform.position.x + _patrolMoveDirection * _enemy.MoveSpeed * Time.fixedDeltaTime;

        if (targetNextPosX < _minXBound || targetNextPosX > _maxXBound)
        {
            _patrolMoveDirection *= -1f; // 이동 방향 반전
            FlipEnemySprite(); // 스프라이트도 반전
            // _enemy.SetEnemyState(EnemyState.Move); // 이미 PatrolState에서 Move로 설정
            return;
        }

        // 2. 낭떠러지 감지 (이동 방향 앞의 발밑에 바닥이 없는지 검사)
        if (_edgeCheck != null)
        {
            Vector3 rayOrigin = _edgeCheck.position;
            if (transform.localScale.x > 0) // 현재 오른쪽을 보고 있음
            {
                rayOrigin.x = transform.position.x + (Mathf.Abs(transform.localScale.x) * GetComponent<Collider2D>().bounds.extents.x) + 0.1f;
            }
            else // 현재 왼쪽을 보고 있음
            {
                rayOrigin.x = transform.position.x - (Mathf.Abs(transform.localScale.x) * GetComponent<Collider2D>().bounds.extents.x) - 0.1f;
            }

            bool isNearEdge = !Physics2D.Raycast(rayOrigin, Vector2.down, _edgeCheckDistance, _whatIsGround);

            Debug.DrawRay(rayOrigin, Vector2.down * _edgeCheckDistance, isNearEdge ? Color.red : Color.green);

            if (isNearEdge)
            {
                _patrolMoveDirection *= -1f; // 이동 방향 반전
                FlipEnemySprite(); // 스프라이트도 반전
                // _enemy.SetEnemyState(EnemyState.Move); // 이미 PatrolState에서 Move로 설정
            }
        }
    }

    // MoveEnemy 함수 (실제 리지드바디 속도 제어)
    private void MoveEnemy()
    {
        if (_enemy.State == EnemyState.Move)
        {
            _rigidbody2D.linearVelocity = new Vector2(_patrolMoveDirection * _enemy.MoveSpeed, _rigidbody2D.linearVelocity.y);
        }
        else // 이동 외 상태 (Attack 등)일 경우 X축 움직임 정지
        {
            _rigidbody2D.linearVelocity = new Vector2(0f, _rigidbody2D.linearVelocity.y);
        }
    }

    // AttackPlayer 함수 (투사체 발사 로직)
    private void AttackPlayer()
    {
        if (_enemy.EnemyBasicArrowPrefab == null || _enemy._attackPoint == null)
        {
            Debug.LogWarning("Enemy: EnemyBasicArrowPrefab 또는 Attack Point가 연결되지 않았습니다!", this);
            return;
        }

        if (!_enemy.CanAttack) return; // 쿨다운 중이면 발사하지 않음

        GameObject projectileInstance = Instantiate(_enemy.EnemyBasicArrowPrefab, _enemy._attackPoint.position, Quaternion.identity);

        EnemyBasicArrow projectileScript = projectileInstance.GetComponent<EnemyBasicArrow>();
        if (projectileScript != null)
        {
            float actualFireAngle = _enemy.FireAngle;

            if (transform.localScale.x < 0)
            {
                actualFireAngle = 180f - _enemy.FireAngle;
            }

            projectileScript.Launch(actualFireAngle);
        }

        _enemy.ResetAttackCooldown(); // 공격 쿨다운 시작

        if (_enemy._animator != null) _enemy._animator.SetTrigger("Attack"); // 애니메이터에 Attack 트리거 설정 필요
    }

    // FacePlayer 함수 (플레이어 방향 바라보기)
    private void FacePlayer()
    {
        if (_playerTransform == null) return;

        float playerX = _playerTransform.position.x;
        float enemyX = transform.position.x;

        if (playerX < enemyX)
        {
            if (transform.localScale.x > 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            if (transform.localScale.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    // FlipEnemySprite 함수 (이동 방향에 따른 스프라이트 반전)
    private void FlipEnemySprite()
    {
        Vector3 currentScale = transform.localScale;
        // _patrolMoveDirection에 따라 스프라이트 반전
        // (주의: FacePlayer와 충돌할 수 있으니, 공격 중에는 FacePlayer만, 이동 중에는 FlipEnemySprite만 사용하도록 AI 상태에서 조절 필요)
        if (_patrolMoveDirection > 0 && currentScale.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else if (_patrolMoveDirection < 0 && currentScale.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    #endregion

    #region 디버그 시각화 (선택 사항)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(_minXBound, transform.position.y - 1f, 0), new Vector3(_minXBound, transform.position.y + 1f, 0));
        Gizmos.DrawLine(new Vector3(_maxXBound, transform.position.y - 1f, 0), new Vector3(_maxXBound, transform.position.y + 1f, 0));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _detectPlayerRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        if (_edgeCheck != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(_edgeCheck.position, 0.1f);
        }
    }
    #endregion
}
