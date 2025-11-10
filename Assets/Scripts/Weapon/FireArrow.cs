using UnityEngine;

public class FireArrow : BasicArrow
{
    [Header("★ 불화살 고유 설정")]
    public GameObject fireEffectPrefab; // <-- 생성할 불길 이펙트 프리팹 (Inspector에서 연결)
    public int fireEffectDamage = 5;    // 불길의 초당 데미지
    public float fireEffectDuration = 2f; // 불길 지속 시간
    public float groundFireEffectYPosition = -3.0f; // <-- 추가: 불길 오브젝트 생성 시 Y축 고정 값

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("Enemy") || collision.CompareTag("EnemyShield"))
        {
            if (fireEffectPrefab != null)
            {
                Vector3 spawnPosition = collision.ClosestPoint(transform.position);
                spawnPosition.y = groundFireEffectYPosition; 
                GameObject fire = Instantiate(fireEffectPrefab, spawnPosition, Quaternion.identity);
                FireEffect fireEffect = fire.GetComponent<FireEffect>();
                if (fireEffect != null)
                {
                    fireEffect.SetFireProperties(fireEffectDamage, fireEffectDuration);
                }
            }
            Destroy(gameObject); 
            return; 
        }

        base.OnTriggerEnter2D(collision);
    }
}
