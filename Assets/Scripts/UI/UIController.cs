using UnityEngine;
using TMPro; // TextMeshPro를 사용할 때 필요
using UnityEngine.UI; // Image 컴포넌트를 사용할 때 필요
using System.Collections; // 코루틴을 사용할 때 필요

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
            // 초기 알파값을 0으로 설정하여, 시작 시에는 완전히 투명하게 만듭니다.
            if (resultPanelImage != null)
            {
                Color tempColor = resultPanelImage.color;
                tempColor.a = 150f / 255f;
                resultPanelImage.color = tempColor;
            }
        }

        InvokeRepeating(nameof(UpdateTimer), 1f, 1f); // 1초마다 UpdateTimer 호출
    }

    private void UpdateTimer()
    {
        if (!timerRunning) return; // 타이머가 실행 중이 아니면 아무것도 하지 않음

        if (currentTime > 0)
        {
            currentTime--;
            UpdateTimerUI();
        }
        else
        {
            CancelInvoke(nameof(UpdateTimer)); // 타이머 종료 시 반복 호출 중지
            timerRunning = false; // 타이머 실행 상태를 false로 변경
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = currentTime.ToString();
    }

    /// <summary>
    /// 게임 종료 시 승리 여부를 받아 UI에 표시하는 함수
    /// 이 함수가 호출되면 게임이 완전히 정지되며, 결과 패널이 서서히 나타납니다.
    /// </summary>
    /// <param name="playerWin">플레이어 승리 여부</param>
    public void SetResult(bool playerWin)
    {
        timerRunning = false; // 타이머 업데이트 중지
        CancelInvoke(nameof(UpdateTimer)); // 모든 InvokeRepeating 호출 중지

        if (resultPanel != null)
            resultPanel.SetActive(true); // 패널을 활성화하여 코루틴이 동작할 수 있게 합니다.

        if (resultPanelImage != null)
        {
            // 알파값 페이드인 코루틴 시작
            StartCoroutine(FadeInResultPanel(resultPanelImage, fadeDuration, 230f / 255f)); // 230은 0~255 스케일, Image.color는 0~1 스케일
        }
        else
        {
            Debug.LogWarning("resultPanelImage가 할당되지 않았습니다. 페이드인 효과 없이 패널이 표시됩니다.");
        }

        if (resultText != null)
            resultText.text = playerWin ? "Victory!!!!!" : "Defeat.....";

        // 코루틴이 완료된 후에 게임 시간을 정지시킵니다.
        // 만약 페이드 도중에도 게임이 정지되기를 원한다면 아래 Time.timeScale = 0f;을
        // StartCoroutine 위에 배치하고, 코루틴 내부에서 Time.unscaledDeltaTime을 사용해야 합니다.
        // 현재 로직은 페이드가 끝나면 게임이 정지됩니다.
        // 혹은 코루틴에서 Time.timeScale을 0으로 만드는 것이 더 좋습니다.

        // GameManager에서 이 함수를 호출할 때 Time.timeScale = 0f;을 먼저 적용해야
        // 코루틴이 Time.unscaledDeltaTime을 사용하지 않아도 게임이 정지된 상태에서 UI 애니메이션이 동작합니다.
        // 만약 게임 매니저에서 즉시 Time.timeScale = 0f;를 호출하지 않고,
        // 이 UIController가 UI 애니메이션을 담당하도록 한다면 아래 코루틴 수정이 필요합니다.
        // 여기서는 GameManager에서 바로 Time.timeScale = 0f;을 호출한다는 가정으로 코드를 유지합니다.
    }

    // 결과 패널의 알파값을 서서히 변경하는 코루틴
    private IEnumerator FadeInResultPanel(Image image, float duration, float targetAlpha)
    {
        Color startColor = image.color; // 현재 색상 (알파 0f로 초기화된 상태)
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        float timer = 0f;

        while (timer < duration)
        {
            // Time.unscaledDeltaTime을 사용하여 Time.timeScale의 영향을 받지 않고 애니메이션을 진행
            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;
            image.color = Color.Lerp(startColor, targetColor, progress);
            yield return null; // 다음 프레임까지 대기
        }
        image.color = targetColor; // 최종 알파값으로 정확히 설정

        // 페이드 인이 완료된 후 게임 시간 정지 (여기서는 UIController가 시간도 정지시키도록 함)
        Time.timeScale = 0f;
    }
}