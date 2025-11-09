using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Collider2D 컴포넌트가 필수
[RequireComponent(typeof(Rigidbody2D))] // Rigidbody2D 컴포넌트가 필수 (Is Kinematic 설정 예정)
public class Shield : MonoBehaviour
{
    [Header("★ 방패 설정")]
    [SerializeField] private int _maxHealth = 10;       // 방패의 최대 체력
    [SerializeField] private float _duration = 6.0f;    // 방패의 지속 시간
    [SerializeField] private Collider2D _collider2D;       // 방패의 최대 체력
    [SerializeField] private Rigidbody2D _rigidbody;    // 방패의 지속 시간
    private int _currentHealth;                         // 현재 체력

    void Awake()
    {
        _currentHealth = _maxHealth; // 시작 시 체력 초기화

        // Rigidbody2D 설정 (중력이 없도록, 물리 영향을 받지 않도록)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // Kinematic으로 설정 (물리 엔진이 움직이지 않음)
            rb.gravityScale = 0f;                    // 중력 없음
        }

        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null)
        {
            coll.isTrigger = false; // <-- 중요: 화살 콜라이더의 Is Trigger가 true이므로, 방패는 Trigger가 아니어야 물리 충돌 감지가 가능
        }
    }

    void Start()
    {
        // 방패 지속 시간 후 스스로 파괴
        Destroy(gameObject, _duration);
    }

    /// <summary>
    /// 방패의 체력 및 지속 시간을 설정하는 함수 (PlayerController에서 호출)
    /// </summary>
    public void SetShieldProperties(int health, float duration)
    {
        _maxHealth = health;
        _currentHealth = health;
        _duration = duration;
        // Destroy(gameObject, _duration); // Start에서 이미 호출되므로 여기서 호출하지 않아도 됨
    }

    // 화살 오브젝트와의 충돌 처리
    void OnTriggerEnter2D(Collider2D other) // OnTriggerEnter2D 사용
    {
        if (other.gameObject.CompareTag("EnemyArrow")) // 충돌한 오브젝트가 화살 태그인지 확인
        {
            Debug.Log($"방패가 {other.gameObject.name}에 맞았습니다!");
            _currentHealth--; // 방패 체력 감소

            // 화살은 방패에 닿으면 파괴 (BasicArrow에서 설정한대로 파괴)
            // Destroy(collision.gameObject); // BasicArrow의 OnTriggerEnter2D에서 파괴되므로 여기서 따로 파괴하지 않아도 됨

            if (_currentHealth <= 0)
            {
                Debug.Log("방패 체력이 모두 소진되어 파괴됩니다.");
                Destroy(gameObject); // 체력이 모두 닳으면 방패 파괴
            }
        }
    }

    // Shield가 파괴될 때 호출될 수 있는 추가 로직
    void OnDestroy()
    {
        Debug.Log("방패가 파괴되었습니다.");
        // TODO: 방패 파괴 시 이펙트나 사운드 추가
    }
}
