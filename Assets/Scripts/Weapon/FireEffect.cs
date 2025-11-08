using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireEffect : MonoBehaviour
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
        // Debug.Log($"불길이 {Time.time}에 데미지를 적용합니다. 타겟 수: {_targetsInFire.Count}");
        // null인 타겟을 미리 제거하여 오류 방지
        _targetsInFire.RemoveWhere(item => item == null);

        foreach (GameObject target in _targetsInFire)
        {
            if (target != null)
            {
                // TODO: 여기에 캐릭터(플레이어나 적군) 체력 닳게 하는 로직 추가
                // 예시: Enemy 클래스가 TakeDamage(int amount) 함수를 가지고 있다고 가정
                // target.GetComponent<Enemy>()?.TakeDamage(_damagePerTick);
                // target.GetComponent<Player>()?.TakeDamage(_damagePerTick); 
                Debug.Log($"{target.name}이(가) 불길에 의해 {_damagePerTick}의 피해를 입었습니다.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // TODO: 캐릭터 태그 확인 필요. "Enemy" 또는 "Player" 태그를 가진 오브젝트만 영향
        if (other.CompareTag("Enemy") || other.CompareTag("Player"))
        {
            _targetsInFire.Add(other.gameObject);
            Debug.Log($"불길에 {other.name} 진입. 현재 타겟 수: {_targetsInFire.Count}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // TODO: 캐릭터 태그 확인 필요
        if (other.CompareTag("Enemy") || other.CompareTag("Player"))
        {
            if (_targetsInFire.Contains(other.gameObject))
            {
                _targetsInFire.Remove(other.gameObject);
                Debug.Log($"불길에서 {other.name} 이탈. 현재 타겟 수: {_targetsInFire.Count}");
            }
        }
    }
}
