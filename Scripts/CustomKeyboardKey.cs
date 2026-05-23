using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomKeyboardKey : MonoBehaviour
{
    [Header("Tuş Değeri")]
    [Tooltip("Harf/Sembol ise direkt kendisini yazın (örn: q, @, 1). Özel komutlar için: BACK, SHIFT, SPACE, DONE, TO_NUMS, TO_LETTERS")]
    public string keyAction; 

    private CustomKeyboardManager keyboardManager;
    private Button button;
    private TextMeshProUGUI keyText;

    void Start()
    {
        button = GetComponent<Button>();
        keyText = GetComponentInChildren<TextMeshProUGUI>();
        keyboardManager = GetComponentInParent<CustomKeyboardManager>();

        if (button != null && keyboardManager != null)
        {
            button.onClick.AddListener(OnKeyClick);
        }
    }

    private void OnKeyClick()
    {
        if (keyboardManager != null)
        {
            keyboardManager.ReceiveKeyStroke(keyAction);
        }
    }

    public void SetLetterCase(bool isUpper)
    {
        // Özel butonların (SHIFT, DONE, ABC vb.) yazısını bozmamak için uzunluk kontrolü cam gibi!
        if (keyText == null || keyAction.Length > 1) return; 

        keyText.text = isUpper ? keyAction.ToUpper() : keyAction.ToLower();
    }
}