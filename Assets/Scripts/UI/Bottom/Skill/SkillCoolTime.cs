using UnityEngine;
using TMPro; // TextMeshPro를 사용한다고 가정합니다. (Unity 메뉴: Window > TextMeshPro > Import TMP Essential Resources)

public class SkillCoolTime : MonoBehaviour
{
    [Header("★ 스킬 쿨타임 UI 설정")]
    [SerializeField] private int _skillIndex; // 이 UI가 담당할 스킬의 인덱스 (0: 스킬1, 1: 스킬2, ...)
    [SerializeField] private TextMeshProUGUI _coolTimeText; // 쿨타임을 표시할 TextMeshProUGUI 컴포넌트
    [SerializeField] private GameObject _skillIconPanel; // 스킬 아이콘 전체 (쿨타임이 아닐 때 활성화, 쿨타임일 때 비활성화하거나 어둡게 처리 등)

    private Player _player; // 플레이어 컴포넌트 참조

    void Start()
    {
        // PlayerManager 싱글톤을 통해 플레이어 데이터 가져오기
        _player = PlayerManager.Instance.GetPlayerData();

        if (_player == null)
        {
            Debug.LogError("SkillCoolTime: PlayerManager에서 Player 데이터를 찾을 수 없습니다.");
            this.enabled = false; // 컴포넌트 비활성화
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
            // Debug.LogWarning($"SkillCoolTime: Player 참조가 없거나 Skill Index가 유효하지 않습니다. Index: {_skillIndex}");
            _coolTimeText.text = ""; // 에러 방지
            if (_skillIconPanel != null) _skillIconPanel.SetActive(true); // 혹시 비활성화되어있다면 활성화
            return;
        }

        // 플레이어의 현재 스킬 쿨다운 타이머 값 가져오기
        int remainingCoolTime = _player.GetSkillRemainingCoolTime(_skillIndex); // 새로 추가할 함수 사용

        if (remainingCoolTime > 0)
        {
            _coolTimeText.text = remainingCoolTime.ToString(); // 쿨타임 표시
            // UI에 쿨타임 중임을 나타내는 처리 (예: 아이콘 어둡게, 쿨타임 숫자만 보이게 등)
            // if (_skillIconPanel != null) _skillIconPanel.SetActive(false); // 또는 이미지의 Fill Amount 조절
        }
        else
        {
            _coolTimeText.text = ""; // 쿨타임 없으면 텍스트 비움
            // UI에 쿨타임이 끝났음을 나타내는 처리 (예: 아이콘 밝게, 비활성된 UI 활성화 등)
            // if (_skillIconPanel != null) _skillIconPanel.SetActive(true);
        }
    }
}