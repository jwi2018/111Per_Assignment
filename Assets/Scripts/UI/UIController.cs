using UnityEngine;
using TMPro; // TextMeshPro를 사용할 때 필요

public class UIController : MonoBehaviour
{
    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText; // 타이머 표시할 텍스트 연결
    [SerializeField] private int startTime = 60; // 시작 시간 (초)

    private float currentTime;
    public int CurrentTime => (int)currentTime;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;         // 승패 UI 패널, 초기에는 비활성화
    [SerializeField] private TextMeshProUGUI resultText;     // 결과 메시지 텍스트

    private bool timerRunning = true;

    void Start()
    {
        currentTime = startTime;
        UpdateTimerUI();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        InvokeRepeating(nameof(UpdateTimer), 1f, 1f); // 1초마다 UpdateTimer 호출
    }

    private void UpdateTimer()
    {
        if (!timerRunning) return;

        if (currentTime > 0)
        {
            currentTime--;
            UpdateTimerUI();
        }
        else
        {
            CancelInvoke(nameof(UpdateTimer)); // 타이머 종료 시 반복 호출 중지
            timerRunning = false;
            // 타이머가 0 되었을 때 자동 호출할 동작이 필요하다면 여기에 추가
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = currentTime.ToString();
    }

    /// <summary>
    /// 게임 종료 시 승리 여부를 받아 UI에 표시하는 함수
    /// </summary>
    /// <param name="playerWin">플레이어 승리 여부</param>
    public void SetResult(bool playerWin)
    {
        timerRunning = false;
        CancelInvoke(nameof(UpdateTimer));

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
            resultText.text = playerWin ? "플레이어 승리!" : "적군 승리!";

        // 게임을 완전히 정지시킵니다. (FixedUpdate, Update 등의 콜백 함수 호출이 멈춥니다)
        Time.timeScale = 0f;
    }
}