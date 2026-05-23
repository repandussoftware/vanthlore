using UnityEngine;
using TMPro;

public class NumPadManager : MonoBehaviour
{
    [Header("Girdi Hedefi")]
    [SerializeField] private TextMeshProUGUI targetInputField; // Sayının yazılacağı ana ekran TMP'si

    [Header("Sınırlar")]
    [SerializeField] private int maxDigits = 6; // Maksimum kaç basamak girilebilsin (Örn: 999.999 elmas)
    [SerializeField] private int maxValueLimit = 999999; // Maksimum girilebilecek sayısal değer

    private string currentInputString = "0";

    void Start()
    {
        UpdateDisplay();
    }

    // Butonlardan gelen veriyi işleyen merkez mühür asdas
    public void ReceiveInput(string value)
    {
        if (targetInputField == null) return;

        switch (value)
        {
            case "CLR": // Her şeyi temizle
                currentInputString = "0";
                break;

            case "BACK": // Son basamağı sil
                if (currentInputString.Length > 1)
                {
                    currentInputString = currentInputString.Substring(0, currentInputString.Length - 1);
                }
                else
                {
                    currentInputString = "0";
                }
                break;

            default: // Rakam girildiyse
                // Eğer başlangıçta sadece 0 varsa ve yeni bir sayı girildiyse 0'ı ez
                if (currentInputString == "0")
                {
                    currentInputString = value;
                }
                else
                {
                    // Basamak sınırı kontrolü
                    if (currentInputString.Length < maxDigits)
                    {
                        string temporaryString = currentInputString + value;
                        
                        // Sayısal değer sınırı kontrolü (Oyuncunun sınırı aşmasını önler)
                        if (int.TryParse(temporaryString, out int parsedValue))
                        {
                            if (parsedValue <= maxValueLimit)
                            {
                                currentInputString = temporaryString;
                            }
                        }
                    }
                }
                break;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (targetInputField != null)
        {
            targetInputField.text = currentInputString;
        }
    }

    // Dışarıdan oyuncunun girdiği net sayıyı almak istersen (Örn: Onayla butonunda kullanmak için)
    public int GetCurrentValue()
    {
        if (int.TryParse(currentInputString, out int result))
        {
            return result;
        }
        return 0;
    }

    // Başka bir işlem için pad sıfırlanmak istenirse
    public void ResetPad()
    {
        currentInputString = "0";
        UpdateDisplay();
    }
}