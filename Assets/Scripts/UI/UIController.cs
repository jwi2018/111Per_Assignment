using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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
    [SerializeField] private Image resultPanelImage;         // 결과 패널의 Image 컴포넌트 (알파값 조절용)
    [SerializeField] private float fadeDuration = 1.5f;      // 알파 변경 애니메이션 시간

    private bool timerRunning = true; // 타이머가 현재 실행 중인지 여부

    void Start()
    {
        currentTime = startTime;
        UpdateTimerUI();

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);

            if (resultPanelImage != null)
            {
                Color tempColor = resultPanelImage.color;
                tempColor.a = 150f / 255f;
                resultPanelImage.color = tempColor;
            }
        }

        InvokeRepeating(nameof(UpdateTimer), 1f, 1f);
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
            CancelInvoke(nameof(UpdateTimer)); 
            timerRunning = false;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = currentTime.ToString();
    }

    public void SetResult(bool playerWin)
    {
        timerRunning = false; 
        CancelInvoke(nameof(UpdateTimer)); 

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultPanelImage != null)
        {
            // 알파값 페이드인 코루틴 시작
            StartCoroutine(FadeInResultPanel(resultPanelImage, fadeDuration, 230f / 255f));
        }

        if (resultText != null)
            resultText.text = playerWin ? "Victory!!!!!" : "Defeat.....";
    }

    private IEnumerator FadeInResultPanel(Image image, float duration, float targetAlpha)
    {
        Color startColor = image.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;
            image.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }
        image.color = targetColor;
        Time.timeScale = 0f;
    }
}