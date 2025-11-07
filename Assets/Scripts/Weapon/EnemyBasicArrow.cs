using UnityEngine;

public class EnemyBasicArrow : MonoBehaviour
{
    [Header("★ 발사")]
    float launchSpeed = 10f;       // 화살 발사 속도

    [Header("★ 컴포넌트")]
    [SerializeField] private Rigidbody2D _rigidbody2D;

    /// <summary>
    /// 방향 벡터와 속도를 받아 투사체를 발사하는 함수 (직선 발사 시)
    /// </summary>
    /// <param name="direction">발사 방향 벡터</param>
    /// <param name="speed">발사 속도</param>
    public void Launch(Vector2 direction, float speed)
    {
        _rigidbody2D.linearVelocity = direction.normalized * speed;
    }

    /// <summary>
    /// 각도를 받아 투사체를 발사하는 함수 (Degree 단위 입력, 포물선 발사 시)
    /// </summary>
    /// <param name="angleDegrees">발사 각도 (0°은 오른쪽 방향)</param>
    /// <param name="speed">발사 속도</param>
    public void Launch(float angleDegrees, float speed)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        _rigidbody2D.linearVelocity = direction * speed;
    }

    void FixedUpdate()
    {
        // 투사체 이동 방향에 맞춰 회전
        Vector2 velocity = _rigidbody2D.linearVelocity;
        if (velocity.sqrMagnitude > 0.01f) // 미세한 움직임이 아닐 때만 회전
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    // 충돌 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어에게 데미지 주기
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(10); // 예시 데미지
            }
            Destroy(gameObject); // 플레이어에게 맞으면 투사체 제거
        }
        else if (collision.CompareTag("Ground"))
        {
            // 땅에 닿으면 투사체 제거
            Destroy(gameObject);
        }
        // else if (collision.CompareTag("Wall")) // 벽에 닿았을 때
        // {
        //     Destroy(gameObject);
        // }
    }
}
