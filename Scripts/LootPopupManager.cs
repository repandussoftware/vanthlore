using UnityEngine;

public class LootPopupManager : MonoBehaviour
{
    public static LootPopupManager Instance;
    public LootSlot[] allLootSlots;
    public Sprite coinIcon;

    void Awake() => Instance = this;

    public void FillLoot(int amount)
    {
        // 1. Önce her şeyi tertemiz (Şeffaf) yapıyoruz
        foreach (var slot in allLootSlots)
        {
            slot.isCoinSlot = false;
            slot.coinAmount = 0;
            slot.currentItem = null;
            slot.UpdateSlotUI();
        }

        // 2. Sadece 1. slota coini koyuyoruz
        if (allLootSlots.Length > 0 && amount > 0)
        {
            allLootSlots[0].SetCoin(amount, coinIcon);
        }
    }

    public bool IsLootEmpty()
    {
        foreach (var slot in allLootSlots)
        {
            // Eğer bir slotta hala coin varsa VEYA normal bir eşya duruyorsa boş değildir canım
            if (slot.isCoinSlot && slot.coinAmount > 0) return false;
            if (slot.currentItem != null) return false;
        }
        return true; // Hiçbir şey bulunamadıysa boştur
    }
}