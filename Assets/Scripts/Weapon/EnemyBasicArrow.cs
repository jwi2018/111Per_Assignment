using UnityEngine;

public class EnemyBasicArrow : MonoBehaviour
{
    [Header("★ 화살 기본 설정")]
    [SerializeField] protected float _defaultLaunchSpeed = 10f;
    [SerializeField] protected float _arrowLifeTime = 3f;

    [Header("★ 컴포넌트")]
    [SerializeField] protected Rigidbody2D _rigidbody2D;

    [Header("★ 타격 효과")]
    [SerializeField] private GameObject hitEffectPrefab; // 타격 이펙트 프리팹 연결
    [SerializeField] private AudioClip hitSoundClip;     // 타격 사운드
    [SerializeField] private AudioSource audioSource;   // AudioSource 컴포넌트

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



    /// <summary>
    /// 방향 벡터와 속도를 받아 투사체를 발사하는 함수 (직선 발사 시)    삭제 예정
    /// </summary>
    /// <param name="direction">발사 방향 벡터</param>
    /// <param name="speed">발사 속도</param>
    public void Launch(Vector2 direction, float speed)
    {
        _rigidbody2D.linearVelocity = direction.normalized * speed;
    }

    /// <summary>
    /// 각도를 받아 투사체를 발사하는 함수 (Degree 단위 입력, 포물선 발사 시)    삭제 예정
    /// </summary>
    /// <param name="angleDegrees">발사 각도 (0°은 오른쪽 방향)</param>
    /// <param name="speed">발사 속도</param>
    public void Launch(float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        _rigidbody2D.linearVelocity = direction * _defaultLaunchSpeed;
    }



    // 충돌 처리
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // <-- 수정: 플레이어와 충돌 시
        {
            Debug.Log($"적 화살이 플레이어 {other.name}와 충돌했습니다.");
            // TODO: 플레이어에게 데미지를 주는 로직 추가
            PlayerManager.Instance.GetPlayerData().TakeDamage(10);

            SpawnHitEffect();

            // 타격 사운드 재생
            PlayHitSound();

            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Debug.Log($"적 화살이 땅 {other.name}에 닿았습니다.");
            Destroy(gameObject);
        }
        else if (other.CompareTag("Shield")) // <-- 수정: 플레이어 방패와 충돌했을 경우
        {
            Debug.Log($"적 화살이 플레이어 방패 {other.name}와 충돌했습니다.");
            
            SpawnHitEffect();

            // 타격 사운드 재생
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
                ps.Play(); // 파티클 시스템이 자동 재생 안될 경우 수동으로 호출
            }
        }
    }

    private void PlayHitSound()
    {
        if (hitSoundClip != null)
        {
            // 임시 오디오 오브젝트 생성
            GameObject tempAudioObject = new GameObject("TempAudio");
            tempAudioObject.transform.position = transform.position;

            AudioSource tempAudioSource = tempAudioObject.AddComponent<AudioSource>();
            tempAudioSource.clip = hitSoundClip;
            tempAudioSource.volume = 1f;
            tempAudioSource.spatialBlend = 0f; // 2D 사운드로 설정

            tempAudioSource.Play();
            Destroy(tempAudioObject, hitSoundClip.length); // 사운드가 끝난 후에 오브젝트 파괴
        }
    }
}
