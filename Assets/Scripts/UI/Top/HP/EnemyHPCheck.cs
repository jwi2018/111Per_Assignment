using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPCheck : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Slider sliderHP;
    [SerializeField] private TextMeshProUGUI textHP;

    void Start()
    {
        sliderHP.maxValue = enemy.MaxHealth; // <-- 중요: 슬라이더의 최대값을 적의 최대 체력으로 설정
        sliderHP.minValue = 0;              // <-- 최소값은 0으로 설정

        UpdateHPUI();
    }

    void Update()
    {
        UpdateHPUI();
    }

    private void UpdateHPUI()
    {
        if (sliderHP != null) sliderHP.value = Utils.Percent(enemy.CurrentHealth);
        if (textHP != null) textHP.text = $"{enemy.CurrentHealth}";
    }
}
