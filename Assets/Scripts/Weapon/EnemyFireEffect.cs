using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyFireEffect : MonoBehaviour
{
    [Header("★ 불길 효과 설정")]
    [SerializeField] private int _damagePerTick = 5;          // 틱당 데미지
    [SerializeField] private float _damageTickInterval = 1.0f; // 데미지 틱 간격 (1f초)
    [SerializeField] private float _effectDuration = 2.0f;    // 불길 총 지속 시간 (2초)

    private float _damageTimer = 0f;
    private HashSet<GameObject> _targetsInFire = new HashSet<GameObject>();

    void Start()
    {
        Destroy(gameObject, _effectDuration);
    }

    void Update()
    {
        _damageTimer -= Time.deltaTime;
        if (_damageTimer <= 0f)
        {
            ApplyDamageToTargets();
            _damageTimer = _damageTickInterval;
        }
    }

    public void SetFireProperties(int damage, float duration)
    {
        _damagePerTick = damage;
        _effectDuration = duration;
    }

    private void ApplyDamageToTargets()
    {
        _targetsInFire.RemoveWhere(item => item == null);

        foreach (GameObject target in _targetsInFire)
        {
            if (target != null)
            {
                if (target.CompareTag("Player")) // <-- 수정: 플레이어에게 데미지
                {
                    Player player = target.GetComponent<Player>();
                    if (player != null)
                    {
                        player.TakeDamage(_damagePerTick);
                        Debug.Log($"{target.name}이(가) 적 불길에 의해 {_damagePerTick}의 피해를 입었습니다. (남은 체력: {player.CurrentHealth})");
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Shield")) // <-- 수정: 플레이어만 감지
        {
            _targetsInFire.Add(other.gameObject);
            Debug.Log($"적 불길에 {other.name} 진입. 현재 타겟 수: {_targetsInFire.Count}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Shield")) // <-- 수정: 플레이어만 감지
        {
            if (_targetsInFire.Contains(other.gameObject))
            {
                _targetsInFire.Remove(other.gameObject);
                Debug.Log($"적 불길에서 {other.name} 이탈. 현재 타겟 수: {_targetsInFire.Count}");
            }
        }
    }
}