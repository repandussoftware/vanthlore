using UnityEngine;
using TMPro;

public class CustomKeyboardManager : MonoBehaviour
{
    public static CustomKeyboardManager Instance; // Global erişim mühürü

    [Header("Gboard Sayfa Panelleri")]
    [SerializeField] private GameObject lettersPanel; // Hiyerarşideki 'letters' objesi
    [SerializeField] private GameObject numsPanel;    // Hiyerarşideki 'nums' objesi

    private TMP_InputField activeInputField;
    private bool isShiftActive = false;
    private CustomKeyboardKey[] allKeys;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Altındaki tüm tuşları otomatik hafızaya alır mühürler!
        allKeys = GetComponentsInChildren<CustomKeyboardKey>(true);
    }

    void Start()
    {
        // 🎯 2. TUZAĞIN ÇÖZÜMÜ: Klavye sahnede AKTİF BAŞLAYACAK (Awake çalışsın diye).
        // Ama oyun başlar başlamaz ilk karede kendisini kodla kapatacak. Cam gibi!
        gameObject.SetActive(false);
    }

    public void ShowKeyboard(TMP_InputField targetField)
    {
        // 🎯 EL DEĞİŞTİRME MÜHÜRÜ: Eğer zaten aktif bir kutu varsa ve gelen kutu eskisinden farklıysa,
        // eski kutunun iç süreçlerini (imleç, caret, focus) temizce sonlandırıp devrediyoruz!
        if (activeInputField != null && activeInputField != targetField)
        {
            activeInputField.DeactivateInputField();
        }

        activeInputField = targetField;
        gameObject.SetActive(true);

        // İlk açılışta büyük harf kuralımız aynen devam ediyor
        isShiftActive = true;
        UpdateKeyboardLayout();

        SwitchToLetters();

        if (activeInputField != null)
        {
            activeInputField.shouldHideMobileInput = true;
        }
    }

    public void CloseKeyboard()
    {
        if (activeInputField != null) activeInputField.DeactivateInputField();
        activeInputField = null;
        gameObject.SetActive(false);
    }

    public void ReceiveKeyStroke(string key)
    {
        if (activeInputField == null) return;

        switch (key)
        {
            case "BACK":
                if (activeInputField.text.Length > 0)
                {
                    activeInputField.text = activeInputField.text.Substring(0, activeInputField.text.Length - 1);
                }
                break;

            case "SHIFT":
                isShiftActive = !isShiftActive;
                UpdateKeyboardLayout();
                break;

            case "SPACE":
                activeInputField.text += " ";
                break;

            case "DONE":
                CloseKeyboard();
                break;

            case "TO_NUMS":
                SwitchToNums();
                break;

            case "TO_LETTERS":
                SwitchToLetters();
                break;

            default:
                string characterToAdd = isShiftActive ? key.ToUpper() : key.ToLower();
                activeInputField.text += characterToAdd;
                break;
        }

        // 🎯 1. TUZAĞIN ÇÖZÜMÜ: Eğer DONE tuşuyla alan kapatılmadıysa tazelemeye izin ver!
        if (activeInputField != null)
        {
            activeInputField.ForceLabelUpdate();
        }
    }

    private void SwitchToLetters()
    {
        if (lettersPanel != null) lettersPanel.SetActive(true);
        if (numsPanel != null) numsPanel.SetActive(false);
    }

    private void SwitchToNums()
    {
        if (lettersPanel != null) lettersPanel.SetActive(false);
        if (numsPanel != null) numsPanel.SetActive(true);
    }

    private void UpdateKeyboardLayout()
    {
        foreach (var key in allKeys)
        {
            if (key != null) key.SetLetterCase(isShiftActive);
        }
    }
}