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
        sliderHP.maxValue = enemy.MaxHealth; 
        sliderHP.minValue = 0;

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
