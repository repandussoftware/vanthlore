using UnityEngine;
using UnityEngine.EventSystems;

public class LootSlot : InventorySlot
{
    public int coinAmount;
    public bool isCoinSlot = false;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (isCoinSlot && coinAmount > 0)
        {
            CollectCoins();
        }
        // LootSlot.cs içindeki OnPointerClick içindeki else if kısmına:
        else if (currentItem != null)
        {
            // Envantere ekleme kodun buraya gelecek...
            // InventoryManager.Instance.AddItem(currentItem); 

            currentItem = null; // Eşyayı lottan çıkar
            UpdateSlotUI();

            // Yine kontrol et canım
            if (LootPopupManager.Instance != null && LootPopupManager.Instance.IsLootEmpty())
            {
                UIManager.Instance.ToggleLootPopup();
            }
        }
    }

    // KRİTİK: Görseli güncellerken coin durumunu kontrol etmeliyiz
    public override void UpdateSlotUI()
    {
        if (itemIcon == null) return;

        if (isCoinSlot && coinAmount > 0)
        {
            itemIcon.enabled = true;
            // Rengi tam opak beyaz yapıyoruz ki coin pırıl pırıl görünsün
            itemIcon.color = Color.white;
            if (placeholderIcon != null) placeholderIcon.enabled = false;
        }
        else
        {
            // Eğer içinde eşya da yoksa ikonu tamamen kapat ki saydam kalsın
            if (currentItem == null)
            {
                itemIcon.enabled = false;
                if (placeholderIcon != null) placeholderIcon.enabled = true;
            }
            else
            {
                itemIcon.enabled = true;
                itemIcon.color = Color.white;
                itemIcon.sprite = currentItem.icon;
                if (placeholderIcon != null) placeholderIcon.enabled = false;
            }
        }
    }
    // LootSlot.cs içindeki CollectCoins fonksiyonunu güncelle:
    private void CollectCoins()
    {
        if (UIManager.Instance != null) UIManager.Instance.AddCoins(coinAmount);

        // Slotu temizle
        coinAmount = 0;
        isCoinSlot = false;
        currentItem = null;
        UpdateSlotUI();

        // --- YENİ: EĞER TÜM LOOT BİTTİYSE PANELİ KAPAT ---
        if (LootPopupManager.Instance != null && LootPopupManager.Instance.IsLootEmpty())
        {
            // UIManager üzerindeki kapatma fonksiyonunu çağırıyoruz canım
            UIManager.Instance.ToggleLootPopup();
            Debug.Log("<color=cyan>Tüm ganimetler toplandı, panel otomatik kapatılıyor.</color>");
        }
    }

    public void SetCoin(int amount, Sprite coinIcon)
    {
        isCoinSlot = true;
        coinAmount = amount;
        itemIcon.sprite = coinIcon;
        UpdateSlotUI(); // Görseli zorla güncelle
    }
}