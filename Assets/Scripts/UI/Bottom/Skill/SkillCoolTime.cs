using UnityEngine;
using UnityEngine.UI; // UI Image 컴포넌트를 사용하기 위해 추가
using TMPro;

public class SkillCoolTime : MonoBehaviour
{
    [Header("★ 스킬 쿨타임 UI 설정")]
    [SerializeField] private int _skillIndex; // 이 UI가 담당할 스킬의 인덱스 (0: 스킬1, 1: 스킬2, ...)

    [SerializeField] private Image _skillIconImage; // 스킬의 기본 아이콘 Image (Optional, 색상 조절용)
    [SerializeField] private Image _cooldownFillImage; // <-- 추가: Fill Amount를 조절할 Image (Filled 타입)
    [SerializeField] private TextMeshProUGUI _coolTimeText; // 쿨타임을 표시할 TextMeshProUGUI 컴포넌트
    // [SerializeField] private GameObject _skillIconPanel; // 제거 또는 다른 용도로 활용

    private Player _player; // 플레이어 컴포넌트 참조

    void Start()
    {
        _player = PlayerManager.Instance.GetPlayerData();

        if (_player == null)
        {
            Debug.LogError("SkillCoolTime: PlayerManager에서 Player 데이터를 찾을 수 없습니다.");
            this.enabled = false;
            return;
        }

        if (_cooldownFillImage == null)
        {
            Debug.LogError("SkillCoolTime: CooldownFillImage가 할당되지 않았습니다. Filled 타입의 Image를 연결해주세요.");
            this.enabled = false;
            return;
        }
        if (_coolTimeText == null)
        {
            Debug.LogError("SkillCoolTime: CoolTimeText가 할당되지 않았습니다. TextMeshProUGUI를 연결해주세요.");
            this.enabled = false;
            return;
        }

        // 시작 시 한 번 업데이트
        UpdateCoolTimeUI();
    }

    void Update()
    {
        UpdateCoolTimeUI();
    }

    private void UpdateCoolTimeUI()
    {
        if (_player == null || _skillIndex < 0 || _skillIndex >= _player.skillCooldowns.Length)
        {
            _coolTimeText.gameObject.SetActive(false);
            _cooldownFillImage.gameObject.SetActive(false);

            _coolTimeText.text = "";
            _cooldownFillImage.fillAmount = 0f;
            return;
        }

        int totalCoolTime = _player.skillCooldowns[_skillIndex]; // 해당 스킬의 전체 쿨타임 (정수)

        // 정밀한 남은 쿨타임 (float) 가져오기
        float remainingCoolTimePrecise = _player.GetSkillRemainingCoolTime(_skillIndex);

        if (remainingCoolTimePrecise > 0f) // 0보다 크면 쿨타임 중
        {
            _coolTimeText.gameObject.SetActive(true);
            _cooldownFillImage.gameObject.SetActive(true);

            // 남은 정수 시간만 텍스트로 표시
            _coolTimeText.text = Mathf.CeilToInt(remainingCoolTimePrecise).ToString(); // 올림하여 정수로 표시

            // Fill Amount 계산
            // 남은 시간이 많을수록 Fill Amount가 1.0에 가까워지고, 시간이 줄어들면 0.0에 가까워집니다.
            float fillAmount = remainingCoolTimePrecise / totalCoolTime;
            _cooldownFillImage.fillAmount = fillAmount;
        }
        else
        {
            _coolTimeText.gameObject.SetActive(false);
            _cooldownFillImage.gameObject.SetActive(false);

            _coolTimeText.text = "";
            _cooldownFillImage.fillAmount = 0f;
        }
    }
}