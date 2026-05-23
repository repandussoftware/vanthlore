using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public string localizationKey;
    private TextMeshProUGUI _textMesh;

    void Awake() => _textMesh = GetComponent<TextMeshProUGUI>();

    void OnEnable()
    {
        // Event'e abone ol (Subscriber)
        LocalizationManager.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    void OnDisable()
    {
        // Obje yok edildiğinde veya kapandığında abonelikten çık (Memory Leak önlemi)
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    public void UpdateText()
    {
        if (_textMesh != null && LocalizationManager.Instance != null)
        {
            _textMesh.text = LocalizationManager.Instance.GetText(localizationKey);
        }
    }
}