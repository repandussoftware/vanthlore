using UnityEngine;
using TMPro;
using UnityEngine.UI; // Unity'nin Toggle bileşeni için canım
using System.Text.RegularExpressions;

public class RegisterFormController : MonoBehaviour
{
    [Header("--- INPUT FIELDS ---")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField passwordAgainInput;
    [SerializeField] private Toggle termsToggle; // Sahnedeki Kullanım Sözleşmesi onay kutusu

    [Header("--- TMPRO WARNING TEXTS ---")]
    [SerializeField] private TextMeshProUGUI usernameWarningText;
    [SerializeField] private TextMeshProUGUI emailWarningText;
    [SerializeField] private TextMeshProUGUI passwordWarningText;
    [SerializeField] private TextMeshProUGUI passwordAgainWarningText;
    [SerializeField] private TextMeshProUGUI termsWarningText; // Sözleşme onaylanmadı uyarısı

    [Header("--- NETWORK BRIDGE ---")]
    [SerializeField] private AuthManager authManager; // AWS'ye şutlayan ana motor

    // Düzenli ifadelerimiz (Regex)
    private readonly Regex usernameRegex = new Regex(@"^[a-zA-Z0-9]{8,}$");
    private readonly Regex emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");

    private void Start()
    {
        ClearAllWarnings();
    }

    // ⚔️ "Kayıt Ol" Butonuna doğrudan bağlı olan kutsal fonksiyon
    public void OnClick_SubmitRegisterForm()
    {
        Debug.Log("<color=magenta>Kayıt Ol Butonuna Basıldı Canım!</color>");
        ClearAllWarnings();

        bool isUsernameValid = ValidateUsername();
        bool isEmailValid = ValidateEmail();
        bool isPasswordValid = ValidatePassword();
        bool isPasswordAgainValid = ValidatePasswordAgain();
        bool isTermsValid = ValidateTerms();

        if (isUsernameValid && isEmailValid && isPasswordValid && isPasswordAgainValid && isTermsValid)
        {
            Debug.Log($"<color=green>Lokal Kontroller Başarılı!</color> Sunucuya doğrudan kayıt isteği şutlanıyor...");
            
            authManager.ExecuteRegister(
                usernameInput.text, 
                emailInput.text, 
                passwordInput.text
            );
        }
        else
        {
            Debug.LogWarning("<color=yellow>Lokal Kontrol Bariyeri:</color> Formda eksik veya onaylanmamış alanlar var canım!");
        }
    }

    // 1. Kullanıcı Adı Denetimi
    private bool ValidateUsername()
    {
        string username = usernameInput.text.Trim();
        if (string.IsNullOrEmpty(username)) 
        { 
            ShowWarning(usernameWarningText, "UI_warn_username_empty", "Username cannot be left blank!"); 
            return false; 
        }
        if (!usernameRegex.IsMatch(username)) 
        { 
            ShowWarning(usernameWarningText, "UI_warn_username_invalid", "Must be at least 8 characters, no special characters!"); 
            return false; 
        }
        return true;
    }

    // 2. E-posta Denetimi
    private bool ValidateEmail()
    {
        string email = emailInput.text.Trim();
        if (string.IsNullOrEmpty(email)) return true; // Opsiyonel şifre adımı için pas geçiş
        if (!emailRegex.IsMatch(email)) 
        { 
            ShowWarning(emailWarningText, "UI_warn_email_invalid", "Invalid email format!"); 
            return false; 
        }
        return true;
    }

    // 3. Şifre Güvenlik Denetimi
    private bool ValidatePassword()
    {
        string password = passwordInput.text;
        if (string.IsNullOrEmpty(password)) 
        { 
            ShowWarning(passwordWarningText, "UI_warn_password_empty", "Password cannot be left blank!"); 
            return false; 
        }
        if (password.Length < 6) 
        { 
            ShowWarning(passwordWarningText, "UI_warn_password_short", "Password must be at least 6 characters!"); 
            return false; 
        }
        return true;
    }

    // 4. Şifre Tekrar Eşleşme Denetimi
    private bool ValidatePasswordAgain()
    {
        string password = passwordInput.text;
        string passwordAgain = passwordAgainInput.text;
        if (string.IsNullOrEmpty(passwordAgain)) 
        { 
            ShowWarning(passwordAgainWarningText, "UI_warn_password_empty", "Password cannot be left blank!"); 
            return false; 
        }
        if (password != passwordAgain) 
        { 
            ShowWarning(passwordAgainWarningText, "UI_warn_password_match", "Passwords do not match!"); 
            return false; 
        }
        return true;
    }

    // 📜 5. Kullanım Sözleşmesi Onay Denetimi
    private bool ValidateTerms()
    {
        if (termsToggle != null)
        {
            if (!termsToggle.isOn)
            {
                ShowWarning(termsWarningText, "UI_warn_terms_not_accepted", "Please accept the Terms & Conditions to continue!");
                return false;
            }
            return true;
        }
        
        Debug.LogError("HATA: RegisterFormController üzerinde TermsToggle referansı eksik canım!");
        return false;
    }

    // 🚀 BULUT SÖZLÜĞÜNÜ DÜRTEN AKILLI UYARI METODU
    private void ShowWarning(TextMeshProUGUI tmpText, string localizationKey, string fallbackText)
    {
        if (tmpText != null)
        {
            // Eğer bulut sözlüğümüz yüklendiyse git kelimeyi o an seçili dilde sök canım canım
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.isDictionaryLoaded)
            {
                string cloudText = LocalizationManager.Instance.GetText(localizationKey);
                
                // Eğer PostgreSQL'de unutulmuşsa veya yüklenemediyse köşeli parantez döner, o zaman fallback yaz canım
                tmpText.text = (cloudText == $"[{localizationKey}]") ? fallbackText : cloudText;
            }
            else
            {
                // Bulut henüz yoldaysa geçici olarak ham İngilizce yedek metni bas canım
                tmpText.text = fallbackText;
            }

            tmpText.color = Color.red; 
            tmpText.gameObject.SetActive(true);
        }
    }

    private void ClearAllWarnings()
    {
        if (usernameWarningText != null) usernameWarningText.gameObject.SetActive(false);
        if (emailWarningText != null) emailWarningText.gameObject.SetActive(false);
        if (passwordWarningText != null) passwordWarningText.gameObject.SetActive(false);
        if (passwordAgainWarningText != null) passwordAgainWarningText.gameObject.SetActive(false);
        if (termsWarningText != null) termsWarningText.gameObject.SetActive(false);
    }
}