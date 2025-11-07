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
}
