using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Veritabanı")]
    public List<ItemData> allPossibleItems = new List<ItemData>();

    [Header("UI Referansları")]
    public InventorySlot[] allInventorySlots;
    public EquipmentSlot[] allEquipmentSlots;

    [Header("Sürükleme ve Bilgi Paneli")]
    public Image globalDragIcon;
    public TMPro.TextMeshProUGUI itemInfoText;
    public GameObject itemDetailsPanelObj;

    private CharacterVisualManager cachedVisualManager;

    [Header("Görsel Efektler")]
    public AvatarHighlight avatarHighlight; // Inspector'dan DarionAvatar'ı buraya sürükle

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (globalDragIcon != null) globalDragIcon.enabled = false;

        // Sahne açıldığında gözetleme sürecini başlatıyoruz
        StartCoroutine(ForceRefreshAfterSceneLoad());
    }

    private IEnumerator ForceRefreshAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();

        // Darion'u (veya popup içindeki görseli) bulana kadar arama modunda kal
        float timer = 0;
        while (cachedVisualManager == null && timer < 5f) // 5 saniye boyunca aramaya devam et
        {
            FindVisualManagerRef();
            if (cachedVisualManager == null)
            {
                timer += 0.2f;
                yield return new WaitForSeconds(0.2f);
            }
        }

        RefreshInventoryUI();
    }

    // --- YENİ: Kapalı Objeleri de Bulan Arama Fonksiyonu ---
    private void FindVisualManagerRef()
    {
        // Standart FindObjectOfType yerine, deaktif (kapalı) objeleri de kapsayan yeni Unity metodunu kullanıyoruz.
        // Bu sayede Popup kapalı olsa bile içindeki scripti "yakalayabiliyoruz".
        cachedVisualManager = FindFirstObjectByType<CharacterVisualManager>(FindObjectsInactive.Include);
    }

    public void RefreshInventoryUI()
    {
        // 1. StatsManager hayatta mı?
        if (StatsManager.Instance == null) return;

        ClearAllSlots();
        bool weaponFound = false;

        // 2. ÇANTADAKİLERİ AKILLI DİZ (Stackleme Mantığı)
        if (StatsManager.Instance.startingItems != null)
        {
            // ID'ye göre miktarları tutan bir sözlük oluşturuyoruz
            Dictionary<string, int> itemStacks = new Dictionary<string, int>();
            // Görselde görünecek tekil item listesi
            List<ItemData> displayList = new List<ItemData>();

            foreach (ItemData item in StatsManager.Instance.startingItems)
            {
                if (item == null) continue;

                if (item.isStackable)
                {
                    if (itemStacks.ContainsKey(item.itemID))
                    {
                        itemStacks[item.itemID]++;
                    }
                    else
                    {
                        itemStacks.Add(item.itemID, 1);
                        displayList.Add(item); // Listeye sadece bir kez ekliyoruz
                    }
                }
                else
                {
                    // Stacklenemeyen (Zırh, Kılıç vb.) her şeyi olduğu gibi listeye ekle
                    displayList.Add(item);
                }
            }

            // Hesaplanan listeyi slotlara yerleştir
            for (int i = 0; i < displayList.Count; i++)
            {
                if (i >= allInventorySlots.Length || allInventorySlots[i] == null) break;

                ItemData currentItem = displayList[i];
                allInventorySlots[i].currentItem = currentItem;

                // Eğer stackable ise sözlükteki sayıyı ver, değilse 1'dir
                if (currentItem.isStackable)
                {
                    allInventorySlots[i].currentQuantity = itemStacks[currentItem.itemID];
                }
                else
                {
                    allInventorySlots[i].currentQuantity = 1;
                }

                allInventorySlots[i].UpdateSlotUI();
            }
        }

        // 3. ÜZERİNDEKİLERİ DİZ (Ekipman Slotları)
        if (StatsManager.Instance.startingWearedItems != null)
        {
            foreach (ItemData item in StatsManager.Instance.startingWearedItems)
            {
                if (item == null) continue;

                EquipmentSlot targetSlot = System.Array.Find(allEquipmentSlots, x => x != null && x.slotType == item.itemType);

                if (targetSlot != null)
                {
                    targetSlot.currentItem = item;
                    targetSlot.currentQuantity = 1; // Ekipmanlar genelde stacklenmez
                    targetSlot.UpdateSlotUI();

                    if (item.itemType == ItemType.Weapon) weaponFound = true;
                }
            }
        }

        // 4. STATSMANAGER GÜNCELLEME
        StatsManager.Instance.isArmed = weaponFound;

        // 5. GÖRSEL TAZELENME
        if (cachedVisualManager == null) FindVisualManagerRef();
        if (cachedVisualManager != null)
        {
            cachedVisualManager.SyncVisualsFromInventory();
        }

        // 6. ZIRH VERİLERİNİ GÜNCELLE
        if (PlayerArmors.Instance != null && StatsManager.Instance.startingWearedItems != null)
        {
            foreach (ItemData item in StatsManager.Instance.startingWearedItems)
            {
                if (item != null)
                {
                    PlayerArmors.Instance.UpdateArmorData(item.itemType, item);
                }
            }
        }
    }
    public void LoadInventoryFromSave(SaveData data)
    {
        Debug.Log("<color=cyan>Aritheon:</color> Veriler yüklendi, UI tazeleniyor...");
        RefreshInventoryUI();
    }

    private void ClearAllSlots()
    {
        foreach (var slot in allInventorySlots) { slot.currentItem = null; slot.UpdateSlotUI(); }
        foreach (var slot in allEquipmentSlots) { slot.currentItem = null; slot.UpdateSlotUI(); }
    }

    public ItemData GetItemByID(string id)
    {
        if (string.IsNullOrEmpty(id) || id == "-1") return null;
        return allPossibleItems.Find(x => x.itemID == id);
    }

    // Parametre ekledik: isUnequipping (Çıkarmak için mi tıklandı?)
    public void EquipFromStartingItems(ItemData itemToEquip, bool isUnequipping = false)
    {
        if (StatsManager.Instance == null || itemToEquip == null) return;

        if (isUnequipping) // ÇIKARMA İŞLEMİ
        {
            if (StatsManager.Instance.startingWearedItems.Contains(itemToEquip))
            {
                StatsManager.Instance.startingItems.Add(itemToEquip);
                StatsManager.Instance.startingWearedItems.Remove(itemToEquip);

                // --- EKLEME: Görseli ve Statları Temizle ---
                SyncEverything(null, itemToEquip.itemType);
            }
        }
        else // GİYME İŞLEMİ
        {
            ItemData alreadyEquipped = StatsManager.Instance.startingWearedItems.Find(x => x.itemType == itemToEquip.itemType);

            if (alreadyEquipped != null)
            {
                StatsManager.Instance.startingWearedItems.Remove(alreadyEquipped);
                StatsManager.Instance.startingItems.Add(alreadyEquipped);
                // Eskisini çıkardığımızı görsele bildiriyoruz
                SyncEverything(null, alreadyEquipped.itemType);
            }

            if (StatsManager.Instance.startingItems.Contains(itemToEquip))
            {
                StatsManager.Instance.startingItems.Remove(itemToEquip);
                StatsManager.Instance.startingWearedItems.Add(itemToEquip);

                // --- EKLEME: Yeni eşyayı görsele ve sisteme bildir ---
                SyncEverything(itemToEquip, itemToEquip.itemType);
            }
        }

        RefreshInventoryUI();
    }

    // BU YENİ YARDIMCI FONKSİYON: Tüm sistemleri aynı anda tetikler
// 👑 VANTHLORE LIVE-OPS ENTEGRASYONU: Tüm sistemleri aynı anda çakışmasız tetikler
    private void SyncEverything(ItemData item, ItemType type)
    {
        // 1. Görsel Model Senkronizasyonu (Darion'un üstündeki pikseller giydiriliyor)
        if (cachedVisualManager != null) 
            cachedVisualManager.UpdateDarionVisual(item, type);

        // 2. Statlar (Zırh koruması ve bonus verileri)
        if (PlayerArmors.Instance != null) 
            PlayerArmors.Instance.UpdateArmorData(type, item);

        // 3. Silah ise Yetenek Barı (SkillBar) Güncellemesi
        /*
        if (type == ItemType.Weapon && SkillBarManager.Instance != null)
            SkillBarManager.Instance.UpdateSkillBar(item, 1);
        */

        // 4. 🎯 MANTIK GÜVENCESİ: DarionController'ın eski yerel metodunu çağırmak yerine,
        // yeni zırhlı sistemimizin HUD tazeleyicisini tetikliyoruz canım benim!
        if (DarionController.Instance != null)
        {
            // Eğer istersen ileride sunucuya "Ben bu zırhı giydim, onay ver" paketi (POST Request) de buradan fırlatılacak!
            // Şimdilik barları ve iç statları anlık pürüzsüz eşitlemesi için tetiğe basıyoruz:
            DarionController.Instance.UpdateNetworkUI();
        }
    }

    public void StartDragging(ItemData item)
    {
        if (item != null && globalDragIcon != null)
        {
            globalDragIcon.sprite = item.icon;
            globalDragIcon.enabled = true;
            if (avatarHighlight != null) avatarHighlight.StartHighlight();
        }
    }

    public void StopDragging()
    {
        if (globalDragIcon != null)
            globalDragIcon.enabled = false;

        if (avatarHighlight != null)
            avatarHighlight.StopHighlight();
    }
    public InventorySlot GetEquipmentSlotByType(ItemType type)
    {
        foreach (var slot in allEquipmentSlots) // allEquipmentSlots listesini zaten InventorySlot.cs'te kullanıyorsun
        {
            if (slot.slotType == type)
            {
                return slot;
            }
        }
        return null;
    }
}