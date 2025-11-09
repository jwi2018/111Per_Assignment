using UnityEngine;

public class EnemyFireArrow : EnemyBasicArrow
{
    [Header("★ 불화살 고유 설정")]
    public GameObject enemyFireEffectPrefab; // <-- 생성할 적 불길 이펙트 프리팹 (Inspector에서 연결)
    public int fireEffectDamage = 5;    // 불길의 틱당 데미지
    public float fireEffectDuration = 2f; // 불길 지속 시간
    public float groundFireEffectYPosition = -2.5f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            if (enemyFireEffectPrefab != null)
            {
                Vector3 spawnPosition = collision.ClosestPoint(transform.position);
                spawnPosition.y = groundFireEffectYPosition;

                GameObject fire = Instantiate(enemyFireEffectPrefab, spawnPosition, Quaternion.identity);

                EnemyFireEffect fireEffect = fire.GetComponent<EnemyFireEffect>(); // <-- 수정: EnemyFireEffect로 GetComponent
                if (fireEffect != null)
                {
                    fireEffect.SetFireProperties(fireEffectDamage, fireEffectDuration);
                }
            }
            Debug.Log($"적 불화살이 땅 {collision.name}에 닿아 불길을 생성하고 사라집니다.");
            Destroy(gameObject);
            return;
        }
        else if (collision.CompareTag("Player") || collision.CompareTag("Shield")) // <-- 수정: 플레이어나 플레이어 방패와 충돌 시
        {
            Debug.Log($"적 불화살이 {collision.name}와 충돌했습니다.");
            Destroy(gameObject);
            return;
        }

        // 지면, 플레이어, 방패가 아닌 다른 오브젝트와 충돌 시, 부모 클래스의 충돌 처리 로직 호출
        base.OnTriggerEnter2D(collision);
    }
}
