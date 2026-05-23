using UnityEngine;
using System.Linq;

public class PotionsBarManager : MonoBehaviour
{
    public static PotionsBarManager Instance;

    [Header("Kısayol Slotları")]
    public PotionSlotUI[] potionSlots; // Hiyerarşideki tüm slotları buraya sürükle (Örn: 4 slot)

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        StartCoroutine(CheckLevelAtStart());
    }

    private System.Collections.IEnumerator CheckLevelAtStart()
    {
        while (StatsManager.Instance == null || InventoryManager.Instance == null)
        {
            yield return null;
        }

        UpdateUnlockedSlots();
        LoadPotionsFromStats(); // <--- YENİ EKLENDİ
    }

    // Bu pot barda zaten ekli mi kontrolü
    public bool IsPotionAssigned(string itemID)
    {
        if (potionSlots == null) return false;
        foreach (var slot in potionSlots)
        {
            if (slot != null && slot.assignedPotion != null && slot.assignedPotion.itemID == itemID)
            {
                return true;
            }
        }
        return false;
    }

    // Potu bardan çıkarma işlemi
    public void RemovePotion(string itemID)
    {
        if (potionSlots == null) return;
        foreach (var slot in potionSlots)
        {
            if (slot != null && slot.assignedPotion != null && slot.assignedPotion.itemID == itemID)
            {
                slot.assignedPotion = null; // Slotu boşalt
                slot.RefreshSlot();         // Görseli güncelle
                Debug.Log("<color=orange>İksir kısayol barından çıkarıldı!</color>");

                SyncWithStatsManager(); // <--- BURAYA ALDIK
                return;
            }
        }
    }

    // --- SEVİYE SİSTEMİ MANTIĞI ---
    public void UpdateUnlockedSlots()
    {
        if (StatsManager.Instance == null) return;

        // Formül: Başlangıçta 2 slot + (Seviye / 10)
        // Örn: Lv 1-9 -> 2 slot | Lv 10-19 -> 3 slot | Lv 20-29 -> 4 slot
        int currentLevel = StatsManager.Instance.currentLevel;
        int slotsToOpen = 2 + (currentLevel / 10);

        // Maksimum slot sayısını aşmaması için sınırla (Dizide kaç slot varsa)
        slotsToOpen = Mathf.Clamp(slotsToOpen, 2, potionSlots.Length);

        for (int i = 0; i < potionSlots.Length; i++)
        {
            if (i < slotsToOpen)
            {
                // Slotu aktif et
                potionSlots[i].gameObject.SetActive(true);
                potionSlots[i].RefreshSlot();
            }
            else
            {
                // Slotu tamamen gizle veya kilitli görseli koy
                potionSlots[i].gameObject.SetActive(false);
            }
        }

        Debug.Log($"<color=cyan>Aritheon:</color> {currentLevel}. seviyeye göre {slotsToOpen} slot aktif.");
    }

    public void LoadPotionsFromStats()
    {
        if (StatsManager.Instance == null || StatsManager.Instance.quickBarPotionIDs == null) return;

        for (int i = 0; i < potionSlots.Length; i++)
        {
            if (i >= StatsManager.Instance.quickBarPotionIDs.Length) break;

            string savedItemID = StatsManager.Instance.quickBarPotionIDs[i];

            // Eğer o slot için kayıtlı bir ID varsa ve boş değilse
            if (!string.IsNullOrEmpty(savedItemID) && savedItemID != "-1")
            {
                ItemData loadedPotion = InventoryManager.Instance.GetItemByID(savedItemID);
                if (loadedPotion != null)
                {
                    potionSlots[i].assignedPotion = loadedPotion;
                }
            }
        }
        RefreshAllSlots();
    }

    public void SyncWithStatsManager()
    {
        if (StatsManager.Instance == null) return;

        // StatsManager'daki diziyi baştan oluştur/boyutlandır
        if (StatsManager.Instance.quickBarPotionIDs == null || StatsManager.Instance.quickBarPotionIDs.Length != potionSlots.Length)
        {
            StatsManager.Instance.quickBarPotionIDs = new string[potionSlots.Length];
        }

        for (int i = 0; i < potionSlots.Length; i++)
        {
            if (potionSlots[i].assignedPotion != null)
            {
                StatsManager.Instance.quickBarPotionIDs[i] = potionSlots[i].assignedPotion.itemID;
            }
            else
            {
                StatsManager.Instance.quickBarPotionIDs[i] = ""; // Boş slot
            }
        }
    }

    public void AssignPotion(ItemData newPotion)
    {
        if (newPotion == null || newPotion.itemType != ItemType.Consumable) return;

        // Sadece AKTİF (açılmış) slotlar üzerinde arama yapalım
        foreach (var slot in potionSlots)
        {
            if (slot.gameObject.activeSelf && slot.assignedPotion != null && slot.assignedPotion.itemID == newPotion.itemID)
            {
                return;
            }
        }

        foreach (var slot in potionSlots)
        {
            // Sadece aktif olan boş slotu bul
            if (slot.gameObject.activeSelf && slot.assignedPotion == null)
            {
                slot.assignedPotion = newPotion;
                slot.RefreshSlot();

                SyncWithStatsManager(); // <--- BURAYA ALDIK
                return;
            }
        }
    }

    public void RefreshAllSlots()
    {
        foreach (var slot in potionSlots)
        {
            if (slot.gameObject.activeSelf)
                slot.RefreshSlot();
        }
    }
}