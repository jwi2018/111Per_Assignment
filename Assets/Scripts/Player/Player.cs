using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float _moveSpeed = 5.0f;

    [Header("체력")]
    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;

    [Header("스킬")]
    
    [SerializeField] private int[] skillCooldowns = new int[5];     // 스킬별 쿨다운 시간
    private int[] skillCooldownTimers = new int[5];                 // 스킬별 쿨다운 타이머
    private float[] skillDeltaTimeAccumulators = new float[5];

    [Header("컴포넌트")]
    public Animator _animator;
    public PlayerInput _playerInput;

    private void Update()
    {
        for (int i = 0; i < skillCooldownTimers.Length; i++)
        {
            if (skillCooldownTimers[i] > 0)
            {
                skillDeltaTimeAccumulators[i] += Time.deltaTime; // 시간 누적
                if (skillDeltaTimeAccumulators[i] >= 1.0f) // 1초가 넘으면
                {
                    skillCooldownTimers[i]--; // 쿨다운 1초 감소
                    skillDeltaTimeAccumulators[i] -= 1.0f; // 1초를 빼고 남은 시간은 유지
                }
            }
        }
    }

    public bool UseSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillCooldowns.Length) return false;

        if (skillCooldownTimers[skillIndex] <= 0)
        {
            // 스킬 발동 처리 (프로젝트에 맞게 커스터마이징)
            _animator.SetTrigger($"Skill{skillIndex + 1}");
            skillCooldownTimers[skillIndex] = skillCooldowns[skillIndex];
            skillDeltaTimeAccumulators[skillIndex] = 0f;
            // 데미지 처리 등 추가 코드 삽입 가능
            return true;
        }
        return false; // 쿨다운 중일 경우
    }
}
