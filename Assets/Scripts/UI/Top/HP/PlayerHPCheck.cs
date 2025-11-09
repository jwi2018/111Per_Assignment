using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPCheck : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Slider sliderHP;
    [SerializeField] private TextMeshProUGUI textHP;

    void Update()
    {
        if (sliderHP != null) sliderHP.value = Utils.Percent(player.CurrentHealth);
        if (textHP != null) textHP.text = $"{player.CurrentHealth}";
    }
}
