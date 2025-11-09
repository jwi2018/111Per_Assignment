using UnityEngine;

public class BasicArrow : MonoBehaviour
{
    [Header("★ 화살 기본 설정")]
    [SerializeField] protected  float launchSpeed = 10f;       // 화살 발사 속도
    [SerializeField] protected float _arrowLifeTime = 3f;      // 화살 존재 시간

    [Header("★ 컴포넌트")]
    [SerializeField] protected Rigidbody2D _rigidbody2D;

    protected virtual void Start() // virtual로 변경
    {
        Destroy(gameObject, _arrowLifeTime);
    }

    // FixedUpdate에서 Rigidbody의 현재 속도 방향을 따라 화살 스프라이트 회전
    public virtual void FixedUpdate()
    {
        Vector2 velocity = _rigidbody2D.linearVelocity; // Rigidbody2D.linearVelocity 대신 Rigidbody2D.velocity 사용

        // 화살이 움직이는 중일 때만 회전
        if (velocity.sqrMagnitude > 0.01f) // 속도 벡터의 크기가 0.01 이상일 때 (아주 미세한 움직임 제외)
        {
            // Atan2 함수로 속도 벡터의 각도를 구하고 Rad2Deg로 라디안을 Degree로 변환
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            // transform.rotation = Quaternion.Euler(0, 0, angle);
            // 대부분의 화살 Sprite는 위쪽(Y+)을 기본 방향으로 그리기 때문에,
            // 발사 방향(오른쪽 X+가 0도 기준)에 맞추려면 -90도를 보정해야 합니다.
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    /// <summary>
    /// 특정 각도와 속도로 화살을 발사하는 함수 (각도는 Degree 단위 입력)
    /// </summary>
    /// <param name="angleDegrees">발사 각도 (0°은 오른쪽 방향)</param>
    /// <param name="speed">발사 속도</param>
    public virtual void Launch(float angleDegrees, float speed)
    {
        // 각도를 라디안으로 변환 (삼각 함수 계산용)
        float angleRad = angleDegrees * Mathf.Deg2Rad;

        // 발사 벡터 계산 (cos이 X축 성분, sin이 Y축 성분)
        Vector2 launchDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        // Rigidbody의 velocity를 사용하여 화살에 속도 적용 (linearVelocity 대신 velocity 권장)
        _rigidbody2D.linearVelocity = launchDirection.normalized * speed;

        // 화살의 방향을 발사 방향에 맞춰 회전
        // 이 코드는 화살 스프라이트가 기본적으로 오른쪽 (X+ 방향)을 바라보고 있다고 가정합니다.
        // 만약 스프라이트가 위쪽 (Y+ 방향)을 바라보고 있다면 Quaternion.Euler(0, 0, angleDegrees - 90f)를 사용해야 합니다.
        transform.rotation = Quaternion.Euler(0, 0, angleDegrees - 90f);
    }

    public virtual void LaunchSkill1(float minAngle, float maxAngle, float speed)
    {
        float randomAngle = Random.Range(minAngle, maxAngle);

        // 생성된 무작위 각도로 Launch 함수 호출
        Launch(randomAngle, speed);

    }

    // 화살이 다른 오브젝트와 충돌했을 때 처리
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 여기에 적에게 데미지를 주는 로직을 추가할 수 있습니다.
            Destroy(gameObject); // 적에게 닿으면 파괴
        }
        // BasicArrow는 Ground에 닿으면 기본적으로 파괴됩니다.
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
        else if (collision.CompareTag("EnemyShield")) // <-- 수정: 플레이어 방패와 충돌했을 경우
        {
            Debug.Log($"적 화살이 플레이어 방패 {collision.name}와 충돌했습니다.");
            Destroy(gameObject);
        }
    }
}
