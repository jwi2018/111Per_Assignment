using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Enemy))]
public class EnemyController : MonoBehaviour
{
    [Header("★ 적군 기본")]
    [SerializeField] private Enemy _enemy;
    [SerializeField] private Transform _playerTransform; // 플레이어의 Transform (인스펙터에서 연결)

    [Header("★ 탐지 및 공격 범위")]
    [SerializeField] private float _detectPlayerRange = 5f; // 플레이어 탐지 범위
    [SerializeField] private float _attackRange = 2f;       // 공격 가능 범위
    [SerializeField] private float _postAttackPatrolChance = 0.3f; // 공격 후 순찰로 돌아갈 확률

    [Header("★ 순찰 설정")]
    [SerializeField] private float _patrolDurationMin = 2f; // 최소 순찰 시간
    [SerializeField] private float _patrolDurationMax = 5f; // 최대 순찰 시간
    [SerializeField] private float _minXBound = -10f; // 맵 좌측 X축 경계
    [SerializeField] private float _maxXBound = 10f;  // 맵 우측 X축 경계
    [SerializeField] private Transform _edgeCheck;     // 낭떠러지 감지용 오브젝트 (Enemy 발 아래에 자식 오브젝트로 생성)
    [SerializeField] private float _edgeCheckDistance = 0.5f; // 낭떠러지 감지 거리
    [SerializeField] private LayerMask _whatIsGround;  // 땅 레이어

    private Rigidbody2D _rigidbody2D;
    private Coroutine _aiBehaviorCoroutine;
    private float _patrolMoveDirection = 1f; // 순찰 방향 (1: 오른쪽, -1: 왼쪽)
    private bool _isPatrolling = false;

    [Header("★ 공격")]
    [SerializeField] private Transform _firePoint;              // 적 화살 발사 위치 (빈 오브젝트)
    [SerializeField] public GameObject enemyArrowPrefab;         // 적군 기본 화살 프리팹 (EnemyBasicArrow.cs)
    [SerializeField] private float _attackDurationMin = 1.0f;   // 최소 공격 지속 시간
    [SerializeField] private float _attackDurationMax = 2.0f;   // 최대 공격 지속 시간

    // --- 적군 스킬 1 (속사) 관련 변수 ---
    [Header("★ 스킬 1 (속사)")]
    [SerializeField] private float _skill1Duration = 3.0f;          // 스킬 1 지속 시간
    private Coroutine _skill1ActiveCoroutine;                       // 스킬 1 지속 코루틴 레퍼런스
    [SerializeField] private float _skill1ShootInterval = 0.2f;     // 스킬 발사 간격
    [SerializeField] private float _skill1Probability = 0.3f;       // 스킬 1 발동 확률 (0~1)

    // --- 적군 스킬 2 (10발 동시 발사) 관련 변수 ---
    [Header("★ 스킬 2 (동시 발사)")]
    [SerializeField] private int _skill2ArrowCount = 10;            // 동시에 발사할 화살 개수
    [SerializeField] private float _skill2SpreadAngle = 30f;        // 화살이 퍼지는 총 각도
    [SerializeField] private float _skill2LaunchSpeedModifier = 0.8f; // 스킬 2 발사 속도 배율
    [SerializeField] private float _skill2Probability = 0.3f;       // 스킬 2 발동 확률

    // --- 적군 스킬 3 (불화살) 관련 변수 ---
    [Header("★ 스킬 3 (불화살)")]
    public GameObject enemyFireArrowPrefab;         // 적군 불화살 프리팹 (EnemyFireArrow.cs)
    public GameObject enemyGroundFireEffectPrefab;  // 적군 불길 이펙트 프리팹 (EnemyFireEffect.cs)
    [SerializeField] private float _skill3LaunchSpeedModifier = 1.0f; // 스킬 3 발사 속도 배율
    [SerializeField] private float _skill3GroundFireEffectYPosition = -2.5f; // 불길 오브젝트 Y축 생성 위치
    [SerializeField] private float _skill3Probability = 0.3f;       // 스킬 3 발동 확률

    // --- 적군 스킬 4 (이동 속도 증가) 관련 변수 ---
    [Header("★ 스킬 4 (이동 속도 증가)")]
    [SerializeField] private float _skill4Duration = 3.0f;          // 스킬 4 지속 시간
    [SerializeField] private float _skill4SpeedMultiplier = 2.0f;   // 이동 속도 배율
    private Coroutine _skill4ActiveCoroutine;                       // 스킬 4 지속 코루틴 레퍼런스
    [SerializeField] private float _skill4Probability = 0.3f;       // 스킬 4 발동 확률

    // --- 적군 스킬 5 (방어막 생성) 관련 변수 ---
    [Header("★ 스킬 5 (방어막 생성)")]
    public GameObject enemyShieldPrefab;            // 적군 방패 프리팹 (EnemyShield.cs)
    [SerializeField] private float _skill5ShieldDuration = 6.0f; // 방패 지속 시간
    [SerializeField] private int _skill5ShieldHealth = 100;      // 방패 체력
    [SerializeField] private float _skill5Probability = 0.3f;       // 스킬 5 발동 확률

    private bool _isSkillActive = false; // 현재 스킬이 발동 중인지 여부 (AI 행동 제어용)


    void Awake()
    {
        if (_enemy == null) _enemy = GetComponent<Enemy>();
        _rigidbody2D = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }

        // AI 행동 코루틴 시작
        _aiBehaviorCoroutine = StartCoroutine(AIBehaviorCoroutine());
    }

    void FixedUpdate()
    {
        if (_enemy.CurrentHealth <= 0)
        {
            if (_aiBehaviorCoroutine != null) StopCoroutine(_aiBehaviorCoroutine);
            if (_skill1ActiveCoroutine != null) StopCoroutine(_skill1ActiveCoroutine);
            if (_skill4ActiveCoroutine != null) StopCoroutine(_skill4ActiveCoroutine);
            _enemy.SetEnemyState(EnemyState.None);
            _rigidbody2D.linearVelocity = Vector2.zero;
            return;
        }

        if (_enemy.State != EnemyState.Skill1Active && _enemy.State != EnemyState.Skill2Active &&
            _enemy.State != EnemyState.Skill3Active && _enemy.State != EnemyState.Skill5Active)
        {
            if (_enemy.State == EnemyState.Move || _enemy.State == EnemyState.Skill4Active)
            {
                CheckForBoundsAndEdges();
            }

            MoveEnemy(); 

            if (_enemy.State == EnemyState.Attack)
            {
                if (_playerTransform != null)
                {
                    FacePlayer();
                }
            }
        }
        else if (_enemy.State == EnemyState.Skill4Active)
        {
            CheckForBoundsAndEdges();
            MoveEnemy();
        }

        if (_playerTransform != null && (_enemy.State == EnemyState.Attack || _enemy.State == EnemyState.Skill1Active || _enemy.State == EnemyState.Skill2Active || _enemy.State == EnemyState.Skill3Active || _enemy.State == EnemyState.Skill5Active))
        {
            FacePlayer();
        }
    }

    #region AI 행동 코루틴

    IEnumerator AIBehaviorCoroutine()
    {
        while (true)
        {
            while (_isSkillActive)
            {
                yield return null;
            }

            bool isPlayerDetected = (_playerTransform != null && Vector2.Distance(transform.position, _playerTransform.position) <= _detectPlayerRange);
            bool isPlayerInAttackRange = (_playerTransform != null && Vector2.Distance(transform.position, _playerTransform.position) <= _attackRange);

            if (isPlayerDetected && isPlayerInAttackRange)
            {
                if (TryActivateSkill())
                {
                    while (_isSkillActive)
                    {
                        yield return null;
                    }
                    if (Random.value < _postAttackPatrolChance)
                    {
                        yield return StartCoroutine(PatrolState());
                    }
                }
                else
                {
                    yield return StartCoroutine(AttackState());

                    if (Random.value < _postAttackPatrolChance)
                    {
                        yield return StartCoroutine(PatrolState());
                    }
                }
            }
            else 
            {
                yield return StartCoroutine(PatrolState());
            }

            yield return null;
        }
    }

    IEnumerator AttackState()
    {
        _enemy.SetEnemyState(EnemyState.Attack);

        float attackDuration = Random.Range(_attackDurationMin, _attackDurationMax);

        yield return new WaitForSeconds(attackDuration);
    }

    IEnumerator PatrolState()
    {
        _enemy.SetEnemyState(EnemyState.Move);
        _isPatrolling = true;

        float patrolDuration = Random.Range(_patrolDurationMin, _patrolDurationMax);

        _patrolMoveDirection = (Random.value > 0.5f) ? 1f : -1f;
        FlipEnemySprite();

        yield return new WaitForSeconds(patrolDuration);

        _isPatrolling = false;
    }

    private bool TryActivateSkill()
    {
        List<int> availableSkills = new List<int>();
        float totalProbability = 0f;

        // 스킬 1
        if (_enemy.IsSkillReady(0)) { availableSkills.Add(0); totalProbability += _skill1Probability; }
        // 스킬 2
        if (_enemy.IsSkillReady(1)) { availableSkills.Add(1); totalProbability += _skill2Probability; }
        // 스킬 3
        if (_enemy.IsSkillReady(2)) { availableSkills.Add(2); totalProbability += _skill3Probability; }
        // 스킬 4
        if (_enemy.IsSkillReady(3)) { availableSkills.Add(3); totalProbability += _skill4Probability; }
        // 스킬 5
        if (_enemy.IsSkillReady(4)) { availableSkills.Add(4); totalProbability += _skill5Probability; }

        if (availableSkills.Count == 0 || totalProbability == 0) return false;

        float randomValue = Random.Range(0f, totalProbability);
        int selectedSkillIndex = -1;

        float currentThreshold = 0f;
        foreach (int skillIndex in availableSkills)
        {
            float skillProb = 0f;
            switch (skillIndex)
            {
                case 0: skillProb = _skill1Probability; break;
                case 1: skillProb = _skill2Probability; break;
                case 2: skillProb = _skill3Probability; break;
                case 3: skillProb = _skill4Probability; break;
                case 4: skillProb = _skill5Probability; break;
            }

            currentThreshold += skillProb;
            if (randomValue <= currentThreshold)
            {
                selectedSkillIndex = skillIndex;
                break;
            }
        }

        if (selectedSkillIndex != -1)
        {
            if (_enemy.TryUseSkill(selectedSkillIndex))
            {
                if (selectedSkillIndex != 3)
                {
                    _rigidbody2D.linearVelocity = Vector2.zero;
                }

                switch (selectedSkillIndex)
                {
                    case 0: // Skill 1: 속사
                        if (_skill1ActiveCoroutine != null) StopCoroutine(_skill1ActiveCoroutine);
                        _skill1ActiveCoroutine = StartCoroutine(Skill1ActiveCoroutine());
                        break;
                    case 1: // Skill 2: 동시 발사
                        _enemy.SetEnemyState(EnemyState.Skill2Active);
                        _isSkillActive = true; 
                        break;
                    case 2: // Skill 3: 불화살
                        _enemy.SetEnemyState(EnemyState.Skill3Active);
                        _isSkillActive = true; 
                        break;
                    case 3: // Skill 4: 이동 속도 증가 (버프형)
                        _enemy.SetEnemyState(EnemyState.Skill4Active); 
                        if (_skill4ActiveCoroutine != null) StopCoroutine(_skill4ActiveCoroutine);
                        _skill4ActiveCoroutine = StartCoroutine(Skill4ActiveCoroutine());
                        break;
                    case 4: // Skill 5: 방어막 생성
                        _enemy.SetEnemyState(EnemyState.Skill5Active);
                        _isSkillActive = true;
                        break;
                }

                return true;
            }
        }
        return false;
    }

    private void PlayHitSound()
    {
        if (_enemy.audioSource != null && _enemy.attackSoundClip != null)
        {
            _enemy.audioSource.PlayOneShot(_enemy.attackSoundClip);
        }
    }

    private IEnumerator Skill1ActiveCoroutine()
    {
        _isSkillActive = true;

        float previousAttackCooldown = _enemy.AttackCooldown;
        _enemy.AttackCooldown = _enemy.BaseAttackCooldown / 2.0f;

        _enemy.SetEnemyState(EnemyState.Skill1Active);

        float timer = 0f;
        while (timer < _skill1Duration)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        _enemy.AttackCooldown = previousAttackCooldown;
        
        RestoreEnemyStateAfterSkillEnd();
        _skill1ActiveCoroutine = null;
    }

    private IEnumerator Skill4ActiveCoroutine()
    {
        _isSkillActive = true;

        float originalMoveSpeed = _enemy.MoveSpeed;
        _enemy.MoveSpeed = originalMoveSpeed * _skill4SpeedMultiplier;

        _enemy.SetEnemyState(EnemyState.Skill4Active);

        float timer = 0f;
        while (timer < _skill4Duration)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        _enemy.MoveSpeed = originalMoveSpeed;
        RestoreEnemyStateAfterSkillEnd();
        _skill4ActiveCoroutine = null;
    }

    public void RestoreEnemyStateAfterSkillEnd()
    {
        _isSkillActive = false; 
        _rigidbody2D.linearVelocity = Vector2.zero; 

        if (_playerTransform != null && Vector2.Distance(transform.position, _playerTransform.position) <= _attackRange)
        {
            _enemy.SetEnemyState(EnemyState.Attack);
        }
        else
        {
            _enemy.SetEnemyState(EnemyState.Move);
        }
    }

    #endregion

    #region AI 보조 함수

    void MoveEnemy()
    {
        if (_patrolMoveDirection != 0)
        {
            _rigidbody2D.linearVelocity = new Vector2(_patrolMoveDirection * _enemy.MoveSpeed, _rigidbody2D.linearVelocity.y);
        }
        else
        {
            _rigidbody2D.linearVelocity = new Vector2(0f, _rigidbody2D.linearVelocity.y);
        }
    }

    void FacePlayer()
    {
        if (_playerTransform == null) return;

        float direction = _playerTransform.position.x - transform.position.x;
        if (direction > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }


    private void CheckForBoundsAndEdges()
    {
        float targetNextPosX = transform.position.x + _patrolMoveDirection * _enemy.MoveSpeed * Time.fixedDeltaTime;

        if (targetNextPosX < _minXBound || targetNextPosX > _maxXBound)
        {
            _patrolMoveDirection *= -1f;
            FlipEnemySprite(); 
            return;
        }

        if (_edgeCheck != null)
        {
            Vector2 rayOrigin = (Vector2)_edgeCheck.position;
            Collider2D enemyCollider = GetComponent<Collider2D>();
            float colliderExtentsX = enemyCollider != null ? enemyCollider.bounds.extents.x : 0.5f;

            if (transform.localScale.x > 0)
            {
                rayOrigin.x = transform.position.x + (Mathf.Abs(transform.localScale.x) * colliderExtentsX) + 0.1f;
            }
            else
            {
                rayOrigin.x = transform.position.x - (Mathf.Abs(transform.localScale.x) * colliderExtentsX) - 0.1f;
            }

            bool isNearEdge = !Physics2D.Raycast(rayOrigin, Vector2.down, _edgeCheckDistance, _whatIsGround);

            if (isNearEdge)
            {
                _patrolMoveDirection *= -1f; 
                FlipEnemySprite();
            }
        }
    }

    void FlipEnemySprite()
    {
        Vector3 currentScale = transform.localScale;
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

    #region Animation Events (플레이어의 Animation Event 함수들을 적군용으로 변환)

    public void AnimationEvent_EnemyAttack()
    {
        if (enemyArrowPrefab == null || _firePoint == null) return;
        if (!_enemy.CanAttack) return;

        float baseAngle = 50f;
        float currentAngle = baseAngle;
        if (transform.localScale.x < 0)
        {
            currentAngle = 180f - currentAngle;
        }

        GameObject arrowInstance = Instantiate(enemyArrowPrefab, _firePoint.position, Quaternion.Euler(0, 0, currentAngle));

        EnemyBasicArrow arrowScript = arrowInstance.GetComponent<EnemyBasicArrow>();
        if (arrowScript != null)
        {
            arrowScript.Launch(currentAngle, _enemy.ArrowLaunchSpeed);

            PlayHitSound();
        }

        _enemy.ResetAttackCooldown();
    }

    public void EnemyAnimationEvent_ShootArrow_Skill1()
    {
        if (enemyArrowPrefab == null || _firePoint == null) return;
        if (!_enemy.CanAttack) return;

        float minAngle = 30f;
        float maxAngle = 60f;
        if (transform.localScale.x < 0)
        {
            minAngle = 180f - maxAngle;
            maxAngle = 180f - 30f;
        }

        GameObject arrowInstance = Instantiate(enemyArrowPrefab, _firePoint.position, _firePoint.rotation);

        EnemyBasicArrow arrowScript = arrowInstance.GetComponent<EnemyBasicArrow>();
        if (arrowScript != null)
        {
            float launchSpeed = _enemy.ArrowLaunchSpeed * 1.2f;
            arrowScript.LaunchSkill1(minAngle, maxAngle, launchSpeed);

            PlayHitSound();
        }
        _enemy.ResetAttackCooldown();
    }

    public void EnemyAnimationEvent_ShootArrow_Skill2()
    {
        if (enemyArrowPrefab == null || _firePoint == null) return;
        if (_enemy.State != EnemyState.Skill2Active) return;

        float baseAngle = 50f;
        float totalSpread = _skill2SpreadAngle;
        int arrowCount = _skill2ArrowCount;
        float launchSpeed = _enemy.ArrowLaunchSpeed * _skill2LaunchSpeedModifier;

        float startAngle = baseAngle - (totalSpread / 2f);
        float angleStep = (arrowCount > 1) ? totalSpread / (arrowCount - 1) : 0f;

        for (int i = 0; i < arrowCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            if (transform.localScale.x < 0)
            {
                currentAngle = 180f - currentAngle;
            }

            GameObject arrowInstance = Instantiate(enemyArrowPrefab, _firePoint.position, Quaternion.Euler(0, 0, currentAngle));
            EnemyBasicArrow arrowScript = arrowInstance.GetComponent<EnemyBasicArrow>();
            if (arrowScript != null)
            {
                arrowScript.Launch(currentAngle, launchSpeed);

                PlayHitSound();
            }
        }
        RestoreEnemyStateAfterSkillEnd();
    }

    public void EnemyAnimationEvent_ShootArrow_Skill3()
    {
        if (enemyFireArrowPrefab == null || _firePoint == null) return;
        if (_enemy.State != EnemyState.Skill3Active) return;

        float launchSpeed = _enemy.ArrowLaunchSpeed * _skill3LaunchSpeedModifier;
        float baseAngle = 50f;

        float currentAngle = baseAngle;
        if (transform.localScale.x < 0)
        {
            currentAngle = 180f - currentAngle;
        }

        GameObject arrowInstance = Instantiate(enemyFireArrowPrefab, _firePoint.position, Quaternion.Euler(0, 0, currentAngle));

        EnemyFireArrow fireArrowScript = arrowInstance.GetComponent<EnemyFireArrow>();
        if (fireArrowScript != null)
        {
            fireArrowScript.enemyFireEffectPrefab = enemyGroundFireEffectPrefab;
            fireArrowScript.groundFireEffectYPosition = _skill3GroundFireEffectYPosition;
            fireArrowScript.Launch(currentAngle, launchSpeed);

            PlayHitSound();
        }
        RestoreEnemyStateAfterSkillEnd();
    }

    public void EnemyAnimationEvent_SpawnShield()
    {
        if (enemyShieldPrefab == null || _enemy == null) return;
        if (_enemy.State != EnemyState.Skill5Active) return;

        Vector3 spawnPosition = transform.position;
        GameObject shieldInstance = Instantiate(enemyShieldPrefab, spawnPosition, Quaternion.identity);

        EnemyShield shieldScript = shieldInstance.GetComponent<EnemyShield>();
        if (shieldScript != null)
        {
            shieldScript.SetShieldProperties(_skill5ShieldHealth, _skill5ShieldDuration);
        }
        RestoreEnemyStateAfterSkillEnd();
    }

    #endregion
}