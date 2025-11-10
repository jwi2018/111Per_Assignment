using UnityEngine;

public class EnemyBasicArrow : MonoBehaviour
{
    [Header("★ 화살 기본 설정")]
    [SerializeField] protected float _defaultLaunchSpeed = 10f;
    [SerializeField] protected float _arrowLifeTime = 3f;

    [Header("★ 컴포넌트")]
    [SerializeField] protected Rigidbody2D _rigidbody2D;

    [Header("★ 타격 효과")]
    [SerializeField] private GameObject hitEffectPrefab; 
    [SerializeField] private AudioClip hitSoundClip; 
    [SerializeField] private AudioSource audioSource; 

    protected virtual void Start()
    {
        Destroy(gameObject, _arrowLifeTime);
    }

    public virtual void Launch(float angleDegrees, float speed)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        Vector2 launchDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        _rigidbody2D.linearVelocity = launchDirection.normalized * speed;
        transform.rotation = Quaternion.Euler(0, 0, angleDegrees - 90f);
    }

    public virtual void LaunchSkill1(float minAngle, float maxAngle, float speed)
    {
        float randomAngle = Random.Range(minAngle, maxAngle);
        Launch(randomAngle, speed);
    }

    protected virtual void FixedUpdate()
    {
        Vector2 velocity = _rigidbody2D.linearVelocity;
        if (velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerManager.Instance.GetPlayerData().TakeDamage(10);

            SpawnHitEffect();
            PlayHitSound();

            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
        else if (other.CompareTag("Shield"))
        {
            SpawnHitEffect();
            PlayHitSound();

            Destroy(gameObject);
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
        }
    }

    private void PlayHitSound()
    {
        if (hitSoundClip != null)
        {
            GameObject tempAudioObject = new GameObject("TempAudio");
            tempAudioObject.transform.position = transform.position;

            AudioSource tempAudioSource = tempAudioObject.AddComponent<AudioSource>();
            tempAudioSource.clip = hitSoundClip;
            tempAudioSource.volume = 1f;
            tempAudioSource.spatialBlend = 0f;

            tempAudioSource.Play();
            Destroy(tempAudioObject, hitSoundClip.length);
        }
    }
}
