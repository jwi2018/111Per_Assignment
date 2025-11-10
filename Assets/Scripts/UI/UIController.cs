using UnityEngine;
using TMPro; // TextMeshPro를 사용할 때 필요

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText; // 타이머 표시할 텍스트 연결
    [SerializeField] private int startTime = 60; // 시작 시간 (초)

    private float currentTime;

    void Start()
    {
        currentTime = startTime;
        UpdateTimerUI();
        InvokeRepeating(nameof(UpdateTimer), 1f, 1f); // 1초마다 UpdateTimer 호출
    }

    private void UpdateTimer()
    {
        if (currentTime > 0)
        {
            currentTime--;
            UpdateTimerUI();
        }
        else
        {
            CancelInvoke(nameof(UpdateTimer)); // 타이머 종료 시 반복 호출 중지
            // 여기서 필요한 동작을 추가할 수 있습니다.
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = currentTime.ToString();
        }
    }
}