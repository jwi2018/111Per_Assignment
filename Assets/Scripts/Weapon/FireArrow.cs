using UnityEngine;

public class FireArrow : BasicArrow
{
    [Header("★ 불화살 고유 설정")]
    public GameObject fireEffectPrefab; // <-- 생성할 불길 이펙트 프리팹 (Inspector에서 연결)
    public int fireEffectDamage = 5;    // 불길의 초당 데미지
    public float fireEffectDuration = 2f; // 불길 지속 시간

    protected override void Start()
    {
        base.Start(); // 부모 클래스의 Start() 호출
        // FireArrow만의 시작 로직
    }

    protected override void OnTriggerEnter2D(Collider2D collision) // 부모의 OnTriggerEnter2D를 오버라이드합니다.
    {
        if (collision.CompareTag("Ground")) // 지면에 닿았을 때
        {
            if (fireEffectPrefab != null)
            {
                // 화살이 닿은 지점에 불길 이펙트 생성
                // collision.ClosestPoint(transform.position)을 사용하여 충돌 지점을 정확히 가져옴
                // 불길 이펙트의 Y축 위치를 조정하여 지면에 정확히 생성되도록 할 수 있음 (예: -0.5f)
                Vector3 spawnPosition = collision.ClosestPoint(transform.position);
                GameObject fire = Instantiate(fireEffectPrefab, spawnPosition, Quaternion.identity);

                // 불길 스크립트에 정보 전달 (damage, duration)
                FireEffect fireEffect = fire.GetComponent<FireEffect>();
                if (fireEffect != null)
                {
                    fireEffect.SetFireProperties(fireEffectDamage, fireEffectDuration);
                }
            }
            Destroy(gameObject); // 불화살은 불길을 생성하고 사라집니다.
            return; // 추가적인 충돌 처리 방지
        }

        // 지면이 아니라면, 부모 클래스(BasicArrow)의 충돌 처리 로직을 호출 (적과 충돌 시 파괴 등)
        base.OnTriggerEnter2D(collision);
    }
}
