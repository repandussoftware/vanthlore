using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LanguagePaginationUI : MonoBehaviour
{
    [Header("--- UI REFS (GÖRSELDEKİ ELEMENTLER) ---")]
    [SerializeField] private TextMeshProUGUI currentLanguageNameText; // Ortadaki "Türkçe" veya "English" yazısı canım
    [SerializeField] private Button leftArrowButton;                 // Sol ok butonu
    [SerializeField] private Button rightArrowButton;                // Sağ ok butonu

    private int _currentLanguageIndex = 0;

    private void OnEnable()
    {
        // 1. Buton listener'larını temizleyip tazece bağlıyoruz canım
        if (leftArrowButton != null)
        {
            leftArrowButton.onClick.RemoveAllListeners();
            leftArrowButton.onClick.AddListener(OnClick_PreviousLanguage);
        }

        if (rightArrowButton != null)
        {
            rightArrowButton.onClick.RemoveAllListeners();
            rightArrowButton.onClick.AddListener(OnClick_NextLanguage);
        }

        // 2. 🎯 DOĞRU EVENT ABONELİĞİ: Sunucudan liste yeni inecekse burası tetiklenecek
        LocalizationManager.OnActiveLanguagesListLoaded += UpdateLanguageUI;

        // 3. 🎯 ÇİFT YÖNLÜ GÜVENLİK KİLİDİ: 
        // Eğer sunucu veriyi çoktan indirdiyse ve liste RAM'de hazır duruyorsa,
        // event'in patlamasını hiç beklemeden okları hemen canlandır canım!
        UpdateLanguageUI();
    }

    private void OnDisable() // 🎯 İSMİ DÜZELTİLDİ: Unity artık burayı otomatik yakalayacak canım!
    {
        // RAM'de çöp birikmemesi ve kodun patlamaması için aboneliği güvenlice düşüyoruz
        LocalizationManager.OnActiveLanguagesListLoaded -= UpdateLanguageUI;
    }

    // 🎯 ARAYÜZÜ SUNUCU VERİLERİNE GÖRE GÜNCELLEYEN MERKEZ
    public void UpdateLanguageUI()
    {
        string activeLangCode = "tr"; // Varsayılan fallback

        // 1. Önce PlayerPrefs veya StatsManager'dan o anki aktif dil kodunu cımbızlıyoruz
        if (PlayerPrefs.HasKey("VANTHLORE_SELECTED_LANG"))
            activeLangCode = PlayerPrefs.GetString("VANTHLORE_SELECTED_LANG");
        else if (StatsManager.Instance != null)
            activeLangCode = StatsManager.Instance.currentLanguage;

        // 2. SUNUCU VERİLERİ HENÜZ İNMEDİYSE VE LİSTE BOMBOŞSA: 
        // Üç nokta basmak yerine, hafızadaki dile göre ismi yaz ve okları geçici kilitle canım
        if (LocalizationManager.Instance == null || LocalizationManager.Instance.activeLanguagesFromServer == null || LocalizationManager.Instance.activeLanguagesFromServer.Count == 0)
        {
            if (currentLanguageNameText != null)
            {
                currentLanguageNameText.text = (activeLangCode.ToLower() == "tr") ? "Türkçe" : "English";
            }
            if (leftArrowButton != null) leftArrowButton.interactable = false;
            if (rightArrowButton != null) rightArrowButton.interactable = false;
            
            Debug.Log("<color=orange>UI Senkronizasyonu:</color> Liste henüz boş veya sunucudan bekleniyor canım.");
            return;
        }

        // 3. SUNUCU VERİLERİ İNDİYSE VEYA ÇOKTAN HAZIRSA (OKLARIN CANLANDIĞI KUTSAL ALAN):
        var serverLangs = LocalizationManager.Instance.activeLanguagesFromServer;
        Debug.Log($"<color=lime>UI Senkronizasyonu:</color> Liste hazır! {serverLangs.Count} adet aktif dil üzerinden oklar hesaplanıyor canım.");

        // Aktif dilin listedeki index'ini milimetrik buluyoruz
        for (int i = 0; i < serverLangs.Count; i++)
        {
            if (serverLangs[i].lang_code.ToLower() == activeLangCode.ToLower())
            {
                _currentLanguageIndex = i;
                break;
            }
        }

        // Ortadaki metni sunucunun gıcır gıcır ismiyle tazeliyoruz
        if (currentLanguageNameText != null)
        {
            currentLanguageNameText.text = serverLangs[_currentLanguageIndex].lang_name;
        }

        // 🔥 İŞTE OKLARI HAYATA DÖNDÜREN FORMÜL:
        // Eğer index 0'dan büyükse SOL OK açılır. Eğer index listenin son elemanından küçükse SAĞ OK açılır!
        if (leftArrowButton != null) 
            leftArrowButton.interactable = (_currentLanguageIndex > 0);
            
        if (rightArrowButton != null) 
            rightArrowButton.interactable = (_currentLanguageIndex < serverLangs.Count - 1);
    }

    // ⬅️ SOL OKA BASILDIĞINDA ÇALIŞACAK MOTOR
    public void OnClick_PreviousLanguage()
    {
        if (LocalizationManager.Instance == null || _currentLanguageIndex <= 0) return;

        _currentLanguageIndex--;
        TriggerLanguageChange();
    }

    // ➡️ SAĞ OKA BASILDIĞINDA ÇALIŞACAK MOTOR
    public void OnClick_NextLanguage()
    {
        if (LocalizationManager.Instance == null) return;
        var serverLangs = LocalizationManager.Instance.activeLanguagesFromServer;

        if (_currentLanguageIndex >= serverLangs.Count - 1) return;

        _currentLanguageIndex++;
        TriggerLanguageChange();
    }

    // 📡 SEÇİLEN YENİ DİLİ BULUTA BİLDİREN VE AKIŞI TETKLEYEN SİHİRBAZ
    private void TriggerLanguageChange()
    {
        var targetLangData = LocalizationManager.Instance.activeLanguagesFromServer[_currentLanguageIndex];

        // 1. Kutsal motoru ateşliyoruz! Sunucu anında o dile ait sözlüğe koşacak canım!
        LocalizationManager.Instance.LoadLanguage(targetLangData.lang_code);

        // 2. Okların tıklanabilirliğini anlık olarak yeni index'e göre güncelliyoruz
        UpdateLanguageUI();
    }
}