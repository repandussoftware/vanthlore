using UnityEngine;
using System;

// Unity'nin yeni indirdiğimiz resmi mobil bildirim kütüphanelerini ekliyoruz canım:
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class NotificationBridge : MonoBehaviour
{
    public static NotificationBridge Instance;

    private const string CHANNEL_ID = "vanthlore_global_channel";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNotificationChannels();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🧱 1. ANDROID İÇİN BİLDİRİM KANALINI İŞLETİM SİSTEMİNE MÜHÜRLÜYORUZ
    private void InitializeNotificationChannels()
    {
        Debug.Log("<color=yellow>Notification Bridge:</color> Yerel bildirim kanalları Android işletim sistemine kaydettiriliyor...");
        
        #if UNITY_ANDROID
        // Android 8.0 ve üzeri için bu kanal tanımı zorunludur canım, yoksa telefon bildirimi basmaz!
        var channel = new AndroidNotificationChannel()
        {
            Id = CHANNEL_ID,
            Name = "VanthLore Canlı Duyurular",
            Importance = Importance.Default,
            Description = "Oyun içi enerji, demirci ve etkinlik bildirimleri canım.",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        #endif
    }

    // ⬅️ 2. YEREL ZAMANLI BİLDİRİM KURMA MOTORU (LOCAL NOTIFICATION)
    // Örn kullanım: NotificationBridge.Instance.ScheduleLocalNotification(7200, "Enerji Doldu!", "Zindana geri dön canım!");
    public void ScheduleLocalNotification(int delayInSeconds, string title, string body)
    {
        Debug.Log($"<color=cyan>Local Notification:</color> {delayInSeconds} saniye sonrasına bildirim kuruldu -> Başlık: {title}");

        #if UNITY_ANDROID
        // Önce cihazın hafızasında birikme yapmasın diye eski bildirimleri temizliyoruz canım
        AndroidNotificationCenter.CancelAllNotifications();

        var notification = new AndroidNotification();
        notification.Title = title;
        notification.Text = body;
        
        // Bildirimin telefonda tam olarak ne zaman patlayacağını cihazın saatine mühürlüyoruz:
        notification.FireTime = System.DateTime.Now.AddSeconds(delayInSeconds);

        // İkon olarak Unity'nin varsayılan uygulama ikonunu fırlatıyoruz canım:
        notification.SmallIcon = "icon_0";
        notification.LargeIcon = "icon_1";

        // Bildirimi Android işletim sisteminin şanlı kollarına bırakıyoruz!
        AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
        #endif
    }

    // 🌍 3. SUNUCUDAN GELEN DUYURULARI ANINDA TELEFONUN ÜST ÇUBUĞUNA BASAN KAPI
    public void TriggerNotificationFromAnnouncement(string title, string content)
    {
        // Oyuncu oyunu arka plana aldığında veya telefon kilitliyken sunucudan taze duyuru inerse,
        // 0 saniye rötarla (yani salisesinde) cihazın üst bildirim çubuğuna gotik bildirimi düşürüyoruz!
        ScheduleLocalNotification(0, title, content); 
    }
}