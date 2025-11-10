using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyShield : MonoBehaviour
{
    [Header("★ 방패 설정")]
    [SerializeField] private int _maxHealth = 10;
    [SerializeField] private float _duration = 6.0f;
    private int _currentHealth;

    void Awake()
    {
        _currentHealth = _maxHealth;
    }

    void Start()
    {
        Destroy(gameObject, _duration);
    }

    public void SetShieldProperties(int health, float duration)
    {
        _maxHealth = health;
        _currentHealth = health;
        _duration = duration;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // <-- 수정: 충돌한 오브젝트가 플레이어의 화살 태그인지 확인
        if (other.CompareTag("Arrow"))
        {
            Debug.Log($"적 방패가 플레이어 화살 {other.name} (태그: {other.tag})에 맞았습니다!");
            _currentHealth--;

            if (_currentHealth <= 0)
            {
                Debug.Log("적 방패 체력이 모두 소진되어 파괴됩니다.");
                Destroy(gameObject);
            }
        }
    }

    void OnDestroy()
    {
        Debug.Log("적 방패가 파괴되었습니다.");
    }
}