using UnityEngine;
using UnityEngine.UI;
using TMPro; // Metinleri parlatmak istersen

public class LanguageSliderController : MonoBehaviour
{
    private Slider _slider;
    
    [Header("Visual Elements")]
    public TextMeshProUGUI trText;
    public TextMeshProUGUI engText;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        
        // Slider'ın 0 ve 1 arasında tık tık etmesi için:
        _slider.wholeNumbers = true; 
        _slider.minValue = 0;
        _slider.maxValue = 1;

        if (StatsManager.Instance != null)
        {
            // Mevcut dile göre slider'ı konumlandır (tr=0, en=1)
            _slider.value = StatsManager.Instance.currentLanguage == "eng" ? 1 : 0;
            UpdateVisuals(_slider.value > 0.5f);
        }
    }

    // Unity Inspector -> On Value Changed (Single) kısmına bunu bağla
    public void OnSliderChanged(float value)
    {
        bool isEnglish = value > 0.5f;
        string langCode = isEnglish ? "eng" : "tr";

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.LoadLanguage(langCode);
        }

        UpdateVisuals(isEnglish);
    }

    private void UpdateVisuals(bool isEnglish)
    {
        // Görsel olarak hangi dil seçiliyse onun parlaklığını ayarlayabilirsin
        if (trText != null) trText.alpha = isEnglish ? 0.3f : 1f;
        if (engText != null) engText.alpha = isEnglish ? 1f : 0.3f;
    }
}