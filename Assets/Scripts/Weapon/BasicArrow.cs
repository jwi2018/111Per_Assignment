using UnityEngine;

public class BasicArrow : MonoBehaviour
{
    [Header("★ 발사")]
    float launchSpeed = 10f;       // 화살 발사 속도

    [Header("★ 컴포넌트")]
    [SerializeField] private Rigidbody2D _rigidbody2D;

    /// <summary>
    /// 각도를 받아 화살을 발사하는 함수 (Degree 단위 입력)
    /// </summary>
    /// <param name="angleDegrees">발사 각도 (0°은 오른쪽 방향)</param>
    public void Launch(float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        // Rigidbody2D에 속도 부여
        _rigidbody2D.linearVelocity = direction * launchSpeed;
    }

    void FixedUpdate()
    {
        Vector2 velocity = _rigidbody2D.linearVelocity;

        // 속도의 방향을 기준으로 화살 회전
        if (velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    // 필요하다면 충돌 처리 함수 추가 (화살이 적이나 바닥 등에 닿았을 때)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Ground"))
        {
            // 데미지 처리나 이펙트 재생 코드를 추가할 수 있습니다.
            // 예: collision.GetComponent<Enemy>()?.TakeDamage(arrowDamage);

            // 화살 삭제 또는 비활성화
            Destroy(gameObject);
        }
    }
}
