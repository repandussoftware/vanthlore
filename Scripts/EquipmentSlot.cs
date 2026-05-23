using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlot : InventorySlot 
{
    // OnBeginDrag, OnDrag ve OnPointerEnter/Exit kısımları 
    // base (InventorySlot) üzerinden geldiği için burada tekrar yazmana gerek yok canım,
    // eğer özel bir ses efekti falan eklemeyeceksen silebilirsin.

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        // Sürükleme bittiğinde ikonu yerine oturtmak yeterlidir.
        UpdateSlotUI();
    }

    public override void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        
        InventorySlot draggedSlot = eventData.pointerDrag.GetComponent<InventorySlot>();

        if (draggedSlot != null && draggedSlot.currentItem != null)
        {
            // 1. TÜR KONTROLÜ: Ekipman slotu sadece kendi tipini kabul eder.
            if (draggedSlot.currentItem.itemType != this.slotType) 
            {
                Debug.Log($"<color=red>Hata:</color> {draggedSlot.currentItem.itemType} bu slota ({this.slotType}) takılamaz!");
                return;
            }

            // 2. MERKEZİ SİSTEMİ ÇAĞIR: 
            // Bu fonksiyon listeleri günceller, görselleri tazeler ve RefreshUI çağırır.
            // (Manager içindeki yeni Sync her şeyi kapsadığı için burası tek satıra düşer)
            InventoryManager.Instance.EquipFromStartingItems(draggedSlot.currentItem, false);

            Debug.Log($"<color=green>{draggedSlot.currentItem.itemName}</color> başarıyla kuşanıldı.");
        }
    }
}