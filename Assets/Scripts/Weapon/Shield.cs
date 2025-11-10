using UnityEngine;

[RequireComponent(typeof(Collider2D))] 
[RequireComponent(typeof(Rigidbody2D))]
public class Shield : MonoBehaviour
{
    [Header("★ 방패 설정")]
    private int _maxHealth;       // 방패의 최대 체력
    private float _duration;    // 방패의 지속 시간
    private int _currentHealth;                         // 현재 체력

    public void SetShieldProperties(int health, float duration)
    {
        _maxHealth = health;
        _currentHealth = health;
        _duration = duration;

        Destroy(gameObject, _duration);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("EnemyArrow")) 
        {
            _currentHealth--; 
            
            if (_currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
