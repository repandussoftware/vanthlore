using UnityEngine;
using TMPro;

public class LoginPanelManager : MonoBehaviour
{
    private enum PanelState { Guest, Login, Register, Help }
    private PanelState currentPanelState = PanelState.Guest;

    [Header("Part Objeleri (Sağ Taraf)")]
    [SerializeField] private GameObject guestPart;
    [SerializeField] private GameObject loginPart;
    [SerializeField] private GameObject registerPart;
    [SerializeField] private GameObject helpPart;

    [Header("Buton Text Objeleri (Dinamik Takip İçin)")]
    [SerializeField] private TextMeshProUGUI guestButtonText;
    [SerializeField] private TextMeshProUGUI loginButtonText;
    [SerializeField] private TextMeshProUGUI registerButtonText;
    [SerializeField] private TextMeshProUGUI helpButtonText;

    [Header("Dinamik Başlık Ayarı (Üst Parşömen)")]
    [SerializeField] private TextMeshProUGUI headerText;

    void OnEnable()
    {
        // 🎯 1. LOKALİZASYON EVENT'İNE ABONE OLMA ZAMANI!
        // Sözlük buluttan indiğinde veya dil değiştiğinde parşömen anında tetiklensin canım!
        LocalizationManager.OnLanguageChanged += RefreshHeaderDinamically;

        // 🎯 2. İlk açılışta doğrudan Misafir modunu tetikle
        ShowGuestSection(guestButtonText);
    }

    void OnDisable()
    {
        // Hafıza sızıntısı olmaması için abonelikten jilet gibi çıkıyoruz
        LocalizationManager.OnLanguageChanged -= RefreshHeaderDinamically;
    }

    // 🌐 DİL DEĞİŞTİĞİ AN BU SİHİRLİ METOT TETİKLENECEK!
    public void RefreshHeaderDinamically()
    {
        switch (currentPanelState)
        {
            case PanelState.Guest:
                UpdateHeader(guestButtonText, "UI_menu_btn_guest", "Guest");
                break;
            case PanelState.Login:
                UpdateHeader(loginButtonText, "UI_menu_btn_login", "Login");
                break;
            case PanelState.Register:
                UpdateHeader(registerButtonText, "UI_menu_btn_register", "Register");
                break;
            case PanelState.Help:
                UpdateHeader(helpButtonText, "UI_menu_btn_help", "Help");
                break;
        }
    }

    // 📱 GUEST BUTONUNA BAĞLANACAK FONKSİYON
    public void ShowGuestSection(TextMeshProUGUI buttonText)
    {
        currentPanelState = PanelState.Guest;
        SetPanelStates(loginActive: false, registerActive: false, helpActive: false, guestActive: true);
        UpdateHeader(buttonText, "UI_menu_btn_guest", "Guest");
    }

    // 🔑 LOGIN BUTONUNA BAĞLANACAK FONKSİYON
    public void ShowLoginSection(TextMeshProUGUI buttonText)
    {
        currentPanelState = PanelState.Login;
        SetPanelStates(loginActive: true, registerActive: false, helpActive: false, guestActive: false);
        UpdateHeader(buttonText, "UI_menu_btn_login", "Login");
    }

    // 📝 REGISTER BUTONUNA BAĞLANACAK FONKSİYON
    public void ShowRegisterSection(TextMeshProUGUI buttonText)
    {
        currentPanelState = PanelState.Register;
        SetPanelStates(loginActive: false, registerActive: true, helpActive: false, guestActive: false);
        UpdateHeader(buttonText, "UI_menu_btn_register", "Register");
    }

    // ❓ HELP BUTONUNA BAĞLANACAK FONKSİYON
    public void ShowHelpSection(TextMeshProUGUI buttonText)
    {
        currentPanelState = PanelState.Help;
        SetPanelStates(loginActive: false, registerActive: false, helpActive: true, guestActive: false);
        UpdateHeader(buttonText, "UI_menu_btn_help", "Help");
    }

    // 🚀 BAŞLIĞI DOĞRUDAN BULUT SÖZLÜĞÜNDEKİ KEY İLE BESLEYEN AKILLI METOT
    private void UpdateHeader(TextMeshProUGUI buttonText, string localizationKey, string fallbackText)
    {
        if (headerText != null)
        {
            // 🎯 SENİN İSTEDİĞİN SİHİRLİ DOKUNUŞ: 
            // Eğer bulut sözlüğü yüklendiyse, butonun ne yazdığına hiç bakma; doğrudan git o şanlı key'i sözlükten sök getir canım!
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.isDictionaryLoaded)
            {
                string cloudText = LocalizationManager.Instance.GetText(localizationKey);
                
                // GetText eğer henüz indirilmediği için köşeli parantezli ham halini fırlatırsa fallback yaz canım
                headerText.text = (cloudText == $"[{localizationKey}]") ? fallbackText : cloudText;
            }
            // Bulut henüz yoldaysa, butonun o anki üstündeki mevcut yazıyı bas (İngilizce ise İngilizce kalır)
            else if (buttonText != null && !string.IsNullOrEmpty(buttonText.text))
            {
                headerText.text = buttonText.text;
            }
            // En kötü senaryoda eldeki yedek kelimeyi mühürle canım
            else
            {
                headerText.text = fallbackText;
            }
        }

        if (CustomKeyboardManager.Instance != null)
            CustomKeyboardManager.Instance.CloseKeyboard();
    }

    private void SetPanelStates(bool loginActive, bool registerActive, bool helpActive, bool guestActive)
    {
        if (loginPart != null) loginPart.SetActive(loginActive);
        if (registerPart != null) registerPart.SetActive(registerActive);
        if (helpPart != null) helpPart.SetActive(helpActive);
        if (guestPart != null) guestPart.SetActive(guestActive);
    }
}