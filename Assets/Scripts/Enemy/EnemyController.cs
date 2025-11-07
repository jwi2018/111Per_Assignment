using UnityEngine;

[RequireComponent(typeof(Enemy), typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("★ 적군 기본 정보")]
    [SerializeField] private Enemy _enemy; // Enemy 스탯/상태 관리 스크립트

    [Header("★ 컴포넌트")]
    [SerializeField] private Rigidbody2D _rigidbody2D;

    [Header("★ 탐지 설정")]
    [SerializeField] private LayerMask _whatIsGround; // 땅 레이어
    [SerializeField] private Transform _groundCheck; // 땅 체크 위치
    [SerializeField] private float _groundCheckRadius = 0.2f; // 땅 체크 반경

    [SerializeField] private Transform _edgeCheck; // 낭떠러지(가장자리) 체크 위치
    [SerializeField] private float _edgeCheckDistance = 0.5f; // 낭떠러지 감지 거리

    [SerializeField] private float _playerDetectRange = 5f; // 플레이어 탐지 범위 (공격, 추적 등)
    private Transform _playerTransform; // 탐지된 플레이어의 트랜스폼

    [Header("★ AI 행동 설정")]
    [SerializeField] private float _patrolMoveDirection = 1f; // 순찰 방향 (1f:오른쪽, -1f:왼쪽)
    [SerializeField] private float _attackRange = 3f; // 플레이어 공격 가능 범위
    [SerializeField] private float _skillUseRange = 4f; // 스킬 사용 가능 범위 (예시)
    [SerializeField] private int _skillToUse = 0; // 사용할 스킬 인덱스 (예시)

    #region Unity 생명주기

    void Awake()
    {
        // 컴포넌트 연결 (Inspector에서 연결하지 않았을 경우 자동 연결)
        if (_enemy == null) _enemy = GetComponent<Enemy>();
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();

        // 플레이어 탐지 (FindObjectOfType은 성능 이슈 가능성 있음, 게임 시작 시 한 번만 호출하거나 GameManager 등에서 관리 권장)
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }

        // Rigidbody2D 세팅 확인 (중력 스케일 등)
        _rigidbody2D.gravityScale = 3f; // 적절한 중력 값
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전 고정 (넘어짐 방지)
    }

    void Update()
    {
        // 1. 플레이어 탐지
        DetectPlayer();

        // 2. 적군 상태에 따른 행동 결정
        DecideAction();

        // 3. 상태에 따라 애니메이션 설정은 Enemy.cs의 SetEnemyState에서 처리하는 것이 좋습니다.
    }

    void FixedUpdate()
    {
        // FixedUpdate에서 물리적인 움직임 처리
        MoveEnemy(_patrolMoveDirection);
    }

    #endregion

    #region Private AI 행동 함수

    private void DetectPlayer()
    {
        if (_playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

        if (distanceToPlayer < _playerDetectRange)
        {
            // 플레이어가 탐지 범위 안에 있음
            // 추적, 공격 등 복합적인 행동 로직 추가 가능
        }
        else
        {
            // 플레이어가 탐지 범위 밖에 있음
        }
    }

    private void DecideAction()
    {
        if (_enemy.CurrentHealth <= 0)
        {
            _enemy.SetEnemyState(EnemyState.None); // <--- EnemyState로 변경
            _rigidbody2D.linearVelocity = Vector2.zero; // 움직임 멈춤
            return;
        }

        // 1. 맵 경계 감지 및 회피 로직
        bool isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _whatIsGround);
        bool isNearEdge = !Physics2D.Raycast(_edgeCheck.position, Vector2.down, _edgeCheckDistance, _whatIsGround);

        if (!isGrounded || isNearEdge)
        {
            // 땅이 없거나 낭떠러지 근처 -> 방향 전환
            _patrolMoveDirection *= -1f; // 이동 방향 반대
            FlipEnemy(); // 캐릭터 이미지 반전 (필요하다면)
        }

        // 2. 플레이어 상태 및 거리 기반 행동 결정
        if (_playerTransform != null && Vector2.Distance(transform.position, _playerTransform.position) < _playerDetectRange)
        {
            // 플레이어가 탐지 범위 내에 있을 때
            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (distanceToPlayer <= _attackRange && _enemy.CanAttack)
            {
                // 플레이어 공격 범위 내 && 공격 가능
                _enemy.SetEnemyState(EnemyState.Attack); // <--- EnemyState로 변경
                AttackPlayer();
                // 공격 후 잠시 멈추거나 뒤로 물러나는 로직 추가
            }
            else if (distanceToPlayer <= _skillUseRange && _enemy.UseSkill(_skillToUse))
            {
                // 플레이어 스킬 사용 범위 내 && 스킬 사용 가능
                _enemy.SetEnemyState(EnemyState.Skill); // <--- EnemyState로 변경
                // 스킬 사용 후 추가 로직 (코루틴 등으로 스킬 애니메이션 시간 대기)
            }
            else
            {
                // 플레이어 추적 (이동)
                _enemy.SetEnemyState(EnemyState.Move); // <--- EnemyState로 변경
                // 플레이어를 향해 이동 방향 설정
                _patrolMoveDirection = Mathf.Sign(_playerTransform.position.x - transform.position.x);
            }
        }
        else
        {
            // 플레이어가 탐지 범위 밖에 있거나 없을 때 -> 순찰 또는 Idle
            _enemy.SetEnemyState(EnemyState.Move); // <--- EnemyState로 변경 (기본적으로 순찰)
        }
    }

    private void MoveEnemy(float direction)
    {
        if (_enemy.State == EnemyState.Move) // <--- EnemyState로 변경
        {
            // 맵 경계를 벗어나지 않는 한 계속 이동
            _rigidbody2D.linearVelocity = new Vector2(direction * _enemy.MoveSpeed, _rigidbody2D.linearVelocity.y);
            // SpriteRenderer 좌우 반전
            if (direction > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (direction < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (_enemy.State == EnemyState.Idle || _enemy.State == EnemyState.Attack || _enemy.State == EnemyState.Skill) // <--- EnemyState로 변경
        {
            _rigidbody2D.linearVelocity = new Vector2(0, _rigidbody2D.linearVelocity.y); // Idle, Attack, Skill 시 좌우 움직임 멈춤
        }
    }

    private void AttackPlayer()
    {
        if (_enemy._enemyBasicArrow == null || _enemy._attackPoint == null)
        {
            Debug.LogWarning("Enemy: Projectile Prefab 또는 Attack Point가 연결되지 않았습니다!", this);
            return;
        }

        if (_enemy._animator != null) _enemy._animator.SetTrigger("Attack");

        GameObject projectileInstance = Instantiate(_enemy._enemyBasicArrow, _enemy._attackPoint.position, _enemy._attackPoint.rotation);

        EnemyBasicArrow projectileScript = projectileInstance.GetComponent<EnemyBasicArrow>();
        if (projectileScript != null)
        {
            // 플레이어 방향으로 발사 또는 고정 각도로 발사
            Vector2 direction = (_playerTransform.position - _enemy._attackPoint.position).normalized;
            projectileScript.Launch(direction, 10f); // Launch(방향, 속도) 함수 가정
        }

        _enemy.ResetAttackCooldown(); // 공격 쿨다운 시작
    }

    private void FlipEnemy()
    {
        // 현재 이동 방향에 따라 적군 스프라이트 좌우 반전
        transform.localScale = new Vector3(_patrolMoveDirection * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    #endregion

    #region 디버그 시각화 (선택 사항)
    void OnDrawGizmosSelected()
    {
        // 땅 체크 위치
        if (_groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }

        // 낭떠러지 체크 (에디터에서 잘 보이도록)
        if (_edgeCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_edgeCheck.position, _edgeCheck.position + Vector3.down * _edgeCheckDistance);
        }

        // 플레이어 탐지 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _playerDetectRange);

        // 공격 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
    #endregion
}
