using UnityEngine;

[RequireComponent(typeof(Collider2D))] 
[RequireComponent(typeof(Rigidbody2D))]
public class Shield : MonoBehaviour
{
    [Header("★ 방패 설정")]
    private int _maxHealth;     
    private float _duration; 
    private int _currentHealth;  

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
