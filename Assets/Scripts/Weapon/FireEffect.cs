using UnityEngine;
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
        _targetsInFire.RemoveWhere(item => item == null);

        SpawnHitEffect();

        foreach (GameObject target in _targetsInFire)
        {
            if (target != null)
            {
                PlayerManager.Instance.GetEnemyData().TakeDamage(_damagePerTick);
                SpawnHitEffect();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyShield"))
        {
            _targetsInFire.Add(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyShield"))
        {
            if (_targetsInFire.Contains(other.gameObject))
            {
                _targetsInFire.Remove(other.gameObject);
            }
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y = -2f;

            GameObject effect = Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
        }
    }

    private void TestSpawnHitEffect()
    {
        if (createEffectPrefab != null)
        {
            createEffectPrefab.Play();
        }
    }
}
