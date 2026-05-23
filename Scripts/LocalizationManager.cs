using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;
    private JObject _localizationData;

    // 🎯 AKTİF DİLLER LİSTESI SUNUCUDAN İNDİĞİNDE UI OKLARINI CANLANDIRAN EVENT
    public static event Action OnActiveLanguagesListLoaded;

    // 🎯 DİL SÖZLÜĞÜ TAMAMEN İNDİĞİNDE TÜM TMPro NESNELERİNİ UYANDIRAN KUTSAL EVENT
    public static event Action OnLanguageChanged;

    private const string LOCAL_LANG_CACHE_KEY = "VANTHLORE_SELECTED_LANG";

    [Header("--- AWS LIGHTSAIL API ENDPOINTS ---")]
    private const string DICTIONARY_URL = "https://vanthlore.repandus.com/api/game/localization-dictionary";
    private const string LANGUAGES_URL = "https://vanthlore.repandus.com/api/game/active-languages";

    // --- 📜 SUNUCUDAN GELEN DİL LİSTESİ DTO MODELLERİ ---
    [System.Serializable]
    public class ServerLanguageData
    {
        public string lang_code; // 'tr', 'en', 'de' gibi
        public string lang_name; // 'Türkçe', 'English' gibi
    }

    [System.Serializable]
    public class ServerLanguagesResponse
    {
        public string status;
        public List<ServerLanguageData> languages;
    }

    [Header("--- RUNTIME CLOUD DATA ---")]
    public List<ServerLanguageData> activeLanguagesFromServer = new List<ServerLanguageData>();
    public bool isDictionaryLoaded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 🎯 MÜHÜR KONTROLÜ: Cihaz hafızasındaki yerel dil tercihine bakıyoruz canım
            string defaultLang = "tr";

            if (PlayerPrefs.HasKey(LOCAL_LANG_CACHE_KEY))
            {
                defaultLang = PlayerPrefs.GetString(LOCAL_LANG_CACHE_KEY);
            }
            else if (StatsManager.Instance != null)
            {
                defaultLang = StatsManager.Instance.currentLanguage;
            }

            // Sistemi tamamen saf bulut akışıyla ayağa kaldırıyoruz
            StartCoroutine(InitializeLocalizationSystem(defaultLang));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🔍 SAHNEDEKİ TÜM TMPro NESNELERİNİ OTOMATİK TARAYIP BULUT VERİSİYLE EŞLEŞTİREN SİHİRBAZ
    public void AnalyseAndBindUI()
    {
        if (!isDictionaryLoaded || _localizationData == null) return;

        // Sahnedeki (aktif veya inaktif fark etmeksizin) TÜM TextMeshProUGUI nesnelerini havada yakalıyoruz canım!
        TMPro.TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TMPro.TextMeshProUGUI>();

        int boundCount = 0;
        foreach (var tmpro in allTexts)
        {
            // 🛡️ GÜVENLİK KONTROLÜ: Nesne sahneye mi ait, yoksa proje klasöründeki (Assets) bir prefab mı?
            // Sadece şu an açık olan aktif sahnedeki nesneleri işleme alıyoruz canım.
            if (tmpro.gameObject.scene.name == null) continue;

            string objName = tmpro.gameObject.name;

            // Eğer objenin adı bizim belirlediğimiz o şanlı 'UI_' standardı ile başlıyorsa:
            if (objName.StartsWith("UI_"))
            {
                // Bulut sözlüğümüzden kelimeyi düz key olarak salisesinde cımbızlıyoruz
                var token = _localizationData[objName];
                if (token != null)
                {
                    tmpro.text = token.ToString();
                    boundCount++;
                }
                else
                {
                    // Eğer veritabanında unutulmuş bir key varsa seni konsolda tatlıca uyarsın canım
                    tmpro.text = $"[{objName}]";
                    Debug.LogWarning($"⚠️ <color=yellow>Sözlük Eksik:</color> Veritabanında '{objName}' anahtarı bulunamadı canım!");
                }
            }
        }

        Debug.Log($"<color=lime>UI Auto-Bind Başarılı:</color> Sahnedeki {boundCount} adet gotik TMPro nesnesi bulut sözlüğüyle milimetrik eşleşti canım!");
    }

    // ⚡ İLK AÇILIŞTA SİSTEMİ ÇİFT YÖNLÜ ATEŞLEYEN BULUT MOTORU
    private IEnumerator InitializeLocalizationSystem(string defaultLang)
    {
        // 1. Önce aktif diller listesini sunucudan çekip hafızaya alıyoruz canım
        yield return StartCoroutine(FetchActiveLanguagesFromServerRoutine());

        // 2. Ardından varsayılan dil sözlüğünü indirip oyunu canlandırıyoruz
        yield return StartCoroutine(LoadLanguageFromServerRoutine(defaultLang));
    }

    // 🚀 DIŞARIDAN VEYA AYARLAR SAYFALAMA OKLARINDAN ÇAĞRILACAK ANA DİL DEĞİŞTİRME METODU
    public void LoadLanguage(string langCode)
    {
        if (StatsManager.Instance != null)
            StatsManager.Instance.currentLanguage = langCode;

        PlayerPrefs.SetString(LOCAL_LANG_CACHE_KEY, langCode.ToLower());
        PlayerPrefs.Save();

        StartCoroutine(LoadLanguageFromServerRoutine(langCode));
    }

    // 🌍 SUNUCUDAKİ AKTİF DİLLERİN LİSTESİNİ ÇEKEN ASENKRON MOTOR
    public IEnumerator FetchActiveLanguagesFromServerRoutine()
    {
        Debug.Log($"<color=lime>Languages [Cloud]:</color> Aktif diller listesi Lightsail sunucusundan talep ediliyor...");

        using (UnityWebRequest request = UnityWebRequest.Get(LANGUAGES_URL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ServerLanguagesResponse response = JsonUtility.FromJson<ServerLanguagesResponse>(request.downloadHandler.text);
                if (response != null && response.languages != null)
                {
                    activeLanguagesFromServer = response.languages;
                    Debug.Log($"<color=green>Languages Başarılı:</color> Sunucudaki {activeLanguagesFromServer.Count} aktif dil seçeneği başarıyla RAM hafızaya alındı canım!");

                    OnActiveLanguagesListLoaded?.Invoke();
                }
            }
            else
            {
                Debug.LogError($"Aktif diller listesi buluttan indirilemedi canım! Hata: {request.error}");
            }
        }
    }

    // 📡 FRANKFURT BULUTUNDAN SÖZLÜK (DICTIONARY) İNDİREN ANA AĞ MOTORU
    private IEnumerator LoadLanguageFromServerRoutine(string langCode)
    {
        isDictionaryLoaded = false;
        string url = $"{DICTIONARY_URL}/{langCode.ToLower()}";
        Debug.Log($"<color=cyan>Localization [Cloud]:</color> '{langCode}' sözlük paketi PostgreSQL odalarından çağrılıyor...");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    JObject serverResponse = JObject.Parse(request.downloadHandler.text);

                    if (serverResponse["dictionary"] != null)
                    {
                        _localizationData = (JObject)serverResponse["dictionary"];
                        isDictionaryLoaded = true;
                        Debug.Log($"<color=green>Localization Başarılı:</color> {langCode} sözlüğü RAM belleğe kusursuzca mühürlendi canım!");

                        // 🔥 Sahnedeki tüm CloudTextDisplayer ajanlarını alt çizgiye göre tetikliyoruz!
                        OnLanguageChanged?.Invoke();
                        // Sözlük başarıyla RAM'e indiği an sahneyi bodoslama tara ve tüm yazıları giydir canım!
                        AnalyseAndBindUI();
                    }
                    else
                    {
                        Debug.LogError("Sunucu yanıt verdi ama 'dictionary' düğümü bulunamadı canım!");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON Parse Hatası canım: {e.Message}");
                }
            }
            else
            {
                // 🛡️ ESKİ YEREL DOSYA YEDEK SİSTEMİ TAMAMEN IPTAL EDİLDİ
                Debug.LogError($"Kritik Ağ Hatası: Dil sözlüğü buluttan çekilemedi! Bağlantınızı kontrol edin canım. Hata: {request.error}");
            }
        }
    }

    // 🎯 ALT ÇİZGİLİ SÖKME KÖPRÜMÜZ
    // Objeden gelen 'UI_menu_btn_login' ismini JSON içinde doğrudan düz bir anahtar olarak saniyeler içinde söküp atar canım!
    public string GetText(string path)
    {
        if (!isDictionaryLoaded || _localizationData == null) return "...";

        // Noktalı hiyerarşi kırılması yerine alt çizgili düz string cımbızlaması yapıyoruz
        var token = _localizationData[path];
        return token != null ? token.ToString() : $"[{path}]";
    }
}