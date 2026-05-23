using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumPadButton : MonoBehaviour
{
    private NumPadManager manager;
    private Button button;
    private TextMeshProUGUI myText;

    [Header("Özel Ayar (İsteğe Bağlı)")]
    [SerializeField] private string specialValue = ""; // Eğer "Geri" veya "Temizle" butonuysa buraya "BACK" veya "CLR" yazabilirsin

    void Start()
    {
        button = GetComponent<Button>();
        myText = GetComponentInChildren<TextMeshProUGUI>();
        
        // Üst parent objelerdeki Manager'ı otomatik bulur (Prefab dostu! Cam gibi!)
        manager = GetComponentInParent<NumPadManager>();

        if (button != null && manager != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        // Eğer özel bir komut atanmadıysa direkt üzerindeki rakamı gönderir
        if (string.IsNullOrEmpty(specialValue))
        {
            if (myText != null)
            {
                manager.ReceiveInput(myText.text);
            }
        }
        else
        {
            manager.ReceiveInput(specialValue);
        }
    }
}