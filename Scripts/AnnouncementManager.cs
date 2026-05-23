using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class AnnouncementManager : MonoBehaviour
{
    public static AnnouncementManager Instance;

    [Header("--- AWS LIGHTSAIL ENDPOINT ---")]
    private const string API_URL = "https://vanthlore.repandus.com/api/game/announcements";

    // --- 📜 DUYURU VERİ MODELLERİ (DTO) ---
    [System.Serializable]
    public class ServerAnnouncementData
    {
        public int id;
        public string icon_type; // 'info', 'book', 'scroll'
        public string title;     // PostgreSQL JSONB'den dile göre havada ayıklanan taze başlık!
        public string content;   // Dile göre detay metni
        public string created_at;
    }

    [System.Serializable]
    public class ServerAnnouncementResponse
    {
        public string status;
        public List<ServerAnnouncementData> announcements;
    }

    [Header("--- RUNTIME CLOUD DATA ---")]
    // Sağ taraftaki o şanlı parşömen arayüzünün doğrudan besleneceği kutsal liste canım
    public List<ServerAnnouncementData> activeAnnouncements = new List<ServerAnnouncementData>();

    // Duyurular buluttan başarıyla indiğinde UI panellerini uyandıracak event
    public static event Action OnAnnouncementsLoaded;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // 🎯 KUTSAL KÖPRÜ: Dil oklarıyla her oynandığında duyurular otomatik olarak buluttan tazelenir canım!
        LocalizationManager.OnLanguageChanged += RefreshAnnouncementsByActiveLanguage;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= RefreshAnnouncementsByActiveLanguage;
    }

    // 🚀 DİL DEĞİŞTİĞİNDE OTOMATİK ÇALIŞAN YARDIMCI MOTOR
    private void RefreshAnnouncementsByActiveLanguage()
    {
        string activeLang = "tr";
        if (PlayerPrefs.HasKey("VANTHLORE_SELECTED_LANG"))
            activeLang = PlayerPrefs.GetString("VANTHLORE_SELECTED_LANG");
        else if (StatsManager.Instance != null)
            activeLang = StatsManager.Instance.currentLanguage;

        FetchAnnouncements(activeLang);
    }

    // 📡 DIŞARIDAN VEYA SAHNE AÇILIŞINDA ÇAĞRILACAK ANA METOD
    public void FetchAnnouncements(string langCode)
    {
        StartCoroutine(FetchAnnouncementsFromServerRoutine(langCode));
    }

    // 🌐 FRANKFURT SUNUCUSUNDAN DUYURULARI İNDİREN ASENKRON AĞ MOTORU
    private IEnumerator FetchAnnouncementsFromServerRoutine(string langCode)
    {
        string url = $"{API_URL}/{langCode.ToLower()}";
        Debug.Log($"<color=orange>Announcements [Cloud]:</color> Duyuru listesi sunucudan ({langCode}) talep ediliyor...");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // 🎯 HATA DÜZELTİLDİ: ServerLanguagesResponse yerine artık aslanlar gibi ServerAnnouncementResponse çözülüyor canım!
                    ServerAnnouncementResponse response = JsonUtility.FromJson<ServerAnnouncementResponse>(request.downloadHandler.text);
                    
                    if (response != null && response.announcements != null)
                    {
                        activeAnnouncements = response.announcements;
                        Debug.Log($"<color=green>Announcements Başarılı:</color> Sunucudan {activeAnnouncements.Count} adet taze duyuru alındı canım!");
                        
                        // 🔥 UI Görüntüleyici ajanları ayağa kalkıyor!
                        OnAnnouncementsLoaded?.Invoke();

                        // 🔔 İleride telefonun üst çubuğuna bildirim fırlatacak köprü tetikleyicisi
                        TryTriggerLocalNotification();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Duyuru JSON parse hatası canım: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Duyurular buluttan indirilemedi canım! Hata: {request.error}");
            }
        }
    }

    private void TryTriggerLocalNotification()
    {
        if (activeAnnouncements == null || activeAnnouncements.Count == 0) return;

        ServerAnnouncementData latestNews = activeAnnouncements[0];
        
        // Bir önceki adımda kurduğumuz NotificationBridge'e kutsal pası atıyoruz canım!
        if (NotificationBridge.Instance != null)
        {
            NotificationBridge.Instance.TriggerNotificationFromAnnouncement(latestNews.title, latestNews.content);
        }
    }
}