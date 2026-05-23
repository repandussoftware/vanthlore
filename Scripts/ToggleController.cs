using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class ToggleController : MonoBehaviour
{
    public TextMeshProUGUI ToggleOnText;
    public TextMeshProUGUI ToggleOffText;
    public Slider toggleSlider; 

    // CANIM: Buradan panel açma/kapama kodlarını sildik. 
    // Sadece yazıların (ON/OFF veya AL/SAT) değişmesini sağlıyoruz.
    public void UpdateToggleVisuals(float value)
    {
        bool isOn = (value >= 0.5f);
        if (ToggleOnText != null) ToggleOnText.gameObject.SetActive(!isOn);
        if (ToggleOffText != null) ToggleOffText.gameObject.SetActive(isOn);
    }
}