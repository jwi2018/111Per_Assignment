using UnityEngine;

public class EnemyFireArrow : EnemyBasicArrow
{
    [Header("★ 불화살 고유 설정")]
    public GameObject enemyFireEffectPrefab;
    public int fireEffectDamage = 5;
    public float fireEffectDuration = 2f;
    public float groundFireEffectYPosition = -2.5f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("Player") || collision.CompareTag("Shield"))
        {
            if (enemyFireEffectPrefab != null)
            {
                Vector3 spawnPosition = collision.ClosestPoint(transform.position);
                spawnPosition.y = groundFireEffectYPosition;

                GameObject fire = Instantiate(enemyFireEffectPrefab, spawnPosition, Quaternion.identity);

                EnemyFireEffect fireEffect = fire.GetComponent<EnemyFireEffect>();
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
