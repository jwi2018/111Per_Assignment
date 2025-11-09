using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPCheck : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Slider sliderHP;
    [SerializeField] private TextMeshProUGUI textHP;

    void Update()
    {
        if (sliderHP != null) sliderHP.value = Utils.Percent(enemy.CurrentHealth);
        if (textHP != null) textHP.text = $"{enemy.CurrentHealth}";
    }
}
