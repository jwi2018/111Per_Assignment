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

    [Header("★ 타격 효과")]
    [SerializeField] private GameObject hitEffectPrefab; // 타격 이펙트 프리팹 연결

    [Header("★ 생성 효과")]
    [SerializeField] private ParticleSystem createEffectPrefab; // 타격 이펙트 프리팹 연결

    void Start()
    {
        TestSpawnHitEffect();

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

        SpawnHitEffect();

        foreach (GameObject target in _targetsInFire)
        {
            if (target != null)
            {
                PlayerManager.Instance.GetEnemyData().TakeDamage(_damagePerTick);
                SpawnHitEffect();

                Debug.Log($"{target.name}이(가) 불길에 의해 {_damagePerTick}의 피해를 입었습니다.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // TODO: 캐릭터 태그 확인 필요. "Enemy" 또는 "Player" 태그를 가진 오브젝트만 영향
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyShield"))
        {
            _targetsInFire.Add(other.gameObject);
            Debug.Log($"불길에 {other.name} 진입. 현재 타겟 수: {_targetsInFire.Count}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // TODO: 캐릭터 태그 확인 필요
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyShield"))
        {
            if (_targetsInFire.Contains(other.gameObject))
            {
                _targetsInFire.Remove(other.gameObject);
                Debug.Log($"불길에서 {other.name} 이탈. 현재 타겟 수: {_targetsInFire.Count}");
            }
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y = -2f;  // Y축 고정

            GameObject effect = Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play(); // 파티클 시스템이 자동 재생 안될 경우 수동으로 호출
            }
        }
    }

    private void TestSpawnHitEffect()
    {
        if (createEffectPrefab != null)
        {
            createEffectPrefab.Play(); // 파티클 시스템이 자동 재생 안될 경우 수동으로 호출
        }
    }
}
