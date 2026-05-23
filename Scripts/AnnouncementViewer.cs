using UnityEngine;
using System.Collections.Generic;

public class AnnouncementViewer : MonoBehaviour
{
    [Header("--- CONTAINER & PREFAB REFS ---")]
    [SerializeField] private Transform contentContainer;      // Hiyerarşideki o şanlı 'Content' objesi canım
    [SerializeField] private GameObject announcementItemPrefab; // Prefab yaptığımız 'Anouncitem' nesnesi

    private void OnEnable()
    {
        // Kutsal sinyali dinlemeye başlıyoruz canım
        AnnouncementManager.OnAnnouncementsLoaded += RefreshAnnouncementUI;

        // Panel her açıldığında eğer listede çoktan veri varsa arayüzü anında çizdir canım
        if (AnnouncementManager.Instance != null && AnnouncementManager.Instance.activeAnnouncements.Count > 0)
        {
            RefreshAnnouncementUI();
        }
    }

    private void OnDisable()
    {
        AnnouncementManager.OnAnnouncementsLoaded -= RefreshAnnouncementUI;
    }

    // 🌍 PARŞÖMENİ BAŞTAN AŞAĞI DİZEN ANA SİHİRBAZ
    private void RefreshAnnouncementUI()
    {
        if (AnnouncementManager.Instance == null || contentContainer == null || announcementItemPrefab == null) return;

        // 1. Önce eski klonlanmış kalıntı satırları temizleyip yer açıyoruz canım
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Sunucudan inen taze listeyi alıyoruz
        List<AnnouncementManager.ServerAnnouncementData> newsList = AnnouncementManager.Instance.activeAnnouncements;
        Debug.Log($"<color=lime>AnnouncementViewer:</color> {newsList.Count} adet güncel duyuru parşömene dökülüyor canım...");

        foreach (var newsData in newsList)
        {
            // 3. Sıradaki duyuru için Content altına yeni bir klon satır fırlatıyoruz
            GameObject rowInstance = Instantiate(announcementItemPrefab, contentContainer);
            
            // 4. Bulut verilerini satıra mühürlüyoruz canım
            AnnouncementItemUI rowScript = rowInstance.GetComponent<AnnouncementItemUI>();
            if (rowScript != null)
            {
                rowScript.InitializeRow(newsData);
            }
        }
    }
}