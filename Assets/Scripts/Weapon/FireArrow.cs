using UnityEngine;

public class FireArrow : BasicArrow
{
    [Header("★ 불화살 고유 설정")]
    public GameObject fireEffectPrefab;
    public int fireEffectDamage = 5; 
    public float fireEffectDuration = 2f; 
    public float groundFireEffectYPosition = -3.0f;

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
