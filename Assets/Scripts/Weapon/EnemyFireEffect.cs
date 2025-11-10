using System.Collections.Generic;
using UnityEngine;

public class EnemyFireEffect : MonoBehaviour
{
    [Header("★ 불길 효과 설정")]
    [SerializeField] private int _damagePerTick = 5;
    [SerializeField] private float _damageTickInterval = 1.0f; 
    [SerializeField] private float _effectDuration = 2.0f;  

    private float _damageTimer = 0f;
    private HashSet<GameObject> _targetsInFire = new HashSet<GameObject>();

    [Header("★ 타격 효과")]
    [SerializeField] private GameObject hitEffectPrefab; 

    [Header("★ 생성 효과")]
    [SerializeField] private ParticleSystem createEffectPrefab;

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

        foreach (GameObject target in _targetsInFire)
        {
            if (target != null)
            {
                if (target.CompareTag("Player"))
                {
                    PlayerManager.Instance.GetPlayerData().TakeDamage(_damagePerTick);
                    SpawnHitEffect();
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Shield"))
        {
            _targetsInFire.Add(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Shield"))
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