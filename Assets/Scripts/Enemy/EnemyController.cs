using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Enemy))]
public class EnemyController : MonoBehaviour
{
    [Header("★ 적군 기본")]
    [SerializeField] private Enemy _enemy;
    [SerializeField] private Transform _playerTransform;

    [Header("★ 탐지 및 공격 범위")]
    [SerializeField] private float _detectPlayerRange = 5f;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _postAttackPatrolChance = 0.3f;

    [Header("★ 순찰 설정")]
    [SerializeField] private float _patrolDurationMin = 2f;
    [SerializeField] private float _patrolDurationMax = 5f;
    [SerializeField] private float _minXBound = -10f;
    [SerializeField] private float _maxXBound = 10f;
    [SerializeField] private Transform _edgeCheck;
    [SerializeField] private float _edgeCheckDistance = 0.5f;
    [SerializeField] private LayerMask _whatIsGround;

    private Rigidbody2D _rigidbody2D;
    private Coroutine _aiBehaviorCoroutine;
    private float _patrolMoveDirection = 1f;
    private bool _isPatrolling = false;

    [Header("★ 공격")]
    [SerializeField] private float _attackDurationMin = 1.0f;
    [SerializeField] private float _attackDurationMax = 2.0f;


    private bool _isSkillActive = false;
    private EnemySkillsManager _enemySkillsManager;


    void Awake()
    {
        if (_enemy == null) _enemy = GetComponent<Enemy>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _enemySkillsManager = GetComponent<EnemySkillsManager>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }
        _aiBehaviorCoroutine = StartCoroutine(AIBehaviorCoroutine());
    }

    void FixedUpdate()
    {
        if (_enemy.CurrentHealth <= 0)
        {
            if (_aiBehaviorCoroutine != null) StopCoroutine(_aiBehaviorCoroutine);
            _enemy.SetEnemyState(EnemyState.None);
            _rigidbody2D.linearVelocity = Vector2.zero;
            return;
        }

        if (_enemy.State == EnemyState.Death) return;

        if (_isSkillActive)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
        }
        else 
        {
            if (_enemy.State == EnemyState.Move || _enemy.State == EnemyState.Skill4Active)
            {
                CheckForBoundsAndEdges();
                MoveEnemy();
                FlipEnemySprite();
            }
            else if (_enemy.State == EnemyState.Attack)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                FacePlayer();
            }
            else
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
            }
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

        for (int i = 0; i < _enemy.skillCooldowns.Length; i++)
        {
            if (_enemy.IsSkillReady(i))
            {
                availableSkills.Add(i);
            }
        }

        if (availableSkills.Count == 0)
        {
            return false;
        }

        int selectedSkillIndex = availableSkills[Random.Range(0, availableSkills.Count)];

        if (_enemySkillsManager.TryActivateSkill(selectedSkillIndex))
        {
            if (selectedSkillIndex != 3)
            {
                _isSkillActive = true;
            }
            return true;
        }
        else
        {
            return false;
        }
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
        if (_isPatrolling || _enemy.State == EnemyState.Skill4Active)
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
        float direction = _patrolMoveDirection;

        if (direction > 0 && currentScale.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else if (direction < 0 && currentScale.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    #endregion
}