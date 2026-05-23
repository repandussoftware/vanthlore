using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // 1. GÜVENLİK DUVARI: Sürüklenen bir obje var mı?
        if (eventData.pointerDrag == null) return;

        // 2. GÜVENLİK DUVARI: Sürüklenen obje bir InventorySlot mu?
        InventorySlot draggedSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (draggedSlot == null || draggedSlot.currentItem == null) return;

        // 3. GÜVENLİK DUVARI: InventoryManager hayatta mı?
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("<color=red>Aritheon Hatası:</color> InventoryManager bulunamadı!");
            return;
        }

        // Uygun ekipman slotunu bul
        InventorySlot targetSlot = InventoryManager.Instance.GetEquipmentSlotByType(draggedSlot.currentItem.itemType);

        if (targetSlot != null)
        {
            Debug.Log($"<color=cyan>{draggedSlot.currentItem.itemName}</color> doğrudan kuşanıldı!");

            // Eşyaları takas et (Swap)
            ItemData oldItem = targetSlot.currentItem;
            targetSlot.currentItem = draggedSlot.currentItem;
            draggedSlot.currentItem = oldItem;

            // UI ve Veri Senkronizasyonu (Merkezi sistem üzerinden)
            // Not: Bu fonksiyonu senin için InventoryManager içinde kurgulamıştık canım
            InventoryManager.Instance.EquipFromStartingItems(targetSlot.currentItem, false);
            
            // Eğer elinden bir şey çıktıysa, çantaya geri döndüğünü doğrula
            targetSlot.UpdateSlotUI();
            draggedSlot.UpdateSlotUI();
        }
        else
        {
            Debug.LogWarning("Bu eşya tipi için uygun bir ekipman slotu bulunamadı!");
        }
    }
}