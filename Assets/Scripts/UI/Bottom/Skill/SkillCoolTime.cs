using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillCoolTime : MonoBehaviour
{
    [Header("★ 스킬 쿨타임 UI 설정")]
    [SerializeField] private int _skillIndex; // 이 UI가 담당할 스킬의 인덱스 (0: 스킬1, 1: 스킬2, ...)

    [SerializeField] private Image _skillIconImage; // 스킬의 기본 아이콘 Image (Optional, 색상 조절용)
    [SerializeField] private Image _cooldownFillImage; // <-- 추가: Fill Amount를 조절할 Image (Filled 타입)
    [SerializeField] private TextMeshProUGUI _coolTimeText; // 쿨타임을 표시할 TextMeshProUGUI 컴포넌트

    private Player _player; // 플레이어 컴포넌트 참조

    void Start()
    {
        _player = PlayerManager.Instance.GetPlayerData();

        if (_player == null)
        {
            this.enabled = false;
            return;
        }

        if (_cooldownFillImage == null)
        {
            this.enabled = false;
            return;
        }

        if (_coolTimeText == null)
        {
            this.enabled = false;
            return;
        }

        UpdateCoolTimeUI();
    }

    void Update()
    {
        UpdateCoolTimeUI();
    }

    private void UpdateCoolTimeUI()
    {
        if (_player == null || _skillIndex < 0 || _skillIndex >= _player.skillBaseCooldowns.Length)
        {
            _coolTimeText.gameObject.SetActive(false);
            _cooldownFillImage.gameObject.SetActive(false);

            _coolTimeText.text = "";
            _cooldownFillImage.fillAmount = 0f;
            return;
        }

        float totalCoolTime = _player.skillBaseCooldowns[_skillIndex];


        float remainingCoolTimePrecise = _player.GetSkillRemainingCoolTime(_skillIndex);

        if (remainingCoolTimePrecise > 0f) 
        {
            _coolTimeText.gameObject.SetActive(true);
            _cooldownFillImage.gameObject.SetActive(true);

            _coolTimeText.text = Mathf.CeilToInt(remainingCoolTimePrecise).ToString();

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