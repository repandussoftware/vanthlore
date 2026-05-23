using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Slot Ayarları")]
    public ItemData currentItem;
    public Image itemIcon;
    public Image placeholderIcon;
    public GameObject highlightOverlay;

    // Yeni Eklenen Kısım: Sayı Göstergesi
    [Header("Yığın (Stack) Ayarları")]
    public GameObject itemCounterObj; // Hiyerarşideki 'ItemCounter'
    public TextMeshProUGUI countText; // İçindeki metin bileşeni
    public int currentQuantity = 1;   // Bu slotta kaç tane var?

    [Header("Ekipman Ayarları")]
    public bool isEquipmentSlot = false;
    public ItemType slotType;

    [Header("Mobil Touch Ayarları")]
    public Vector2 dragOffset = new Vector2(0, 100f);

    private CanvasGroup canvasGroup;
    private InventoryManager manager;
    private Coroutine pulseCoroutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        manager = InventoryManager.Instance; //

        if (highlightOverlay != null && !isEquipmentSlot) highlightOverlay.SetActive(false);
        UpdateSlotUI();
    }

    void Start()
    {
        if (manager != null && manager.itemDetailsPanelObj != null)
            manager.itemDetailsPanelObj.SetActive(false);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null || manager == null || manager.itemInfoText == null)
        {
            if (manager != null && manager.itemDetailsPanelObj != null)
                manager.itemDetailsPanelObj.SetActive(false);
            return;
        }
        ShowItemDescription(true);
    }

    private void ShowItemDescription(bool isPanelActive = false)
    {
        if (currentItem != null && manager != null && manager.itemInfoText != null)
        {
            manager.itemInfoText.text = currentItem.itemName + "\n" +
                                       currentItem.levelRequirement + " Level" + "\n" +
                                       currentItem.description;

            if (manager.itemDetailsPanelObj != null)
                manager.itemDetailsPanelObj.SetActive(isPanelActive);

            if (ItemDetailsManager.Instance != null)
                ItemDetailsManager.Instance.ShowDetails(currentItem);
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // Manager referansını tazele ve hata kontrolü yap
        if (manager == null) manager = InventoryManager.Instance;

        if (manager != null)
        {
            // Manager üzerinden merkezi sürüklemeyi başlat
            manager.StartDragging(currentItem);
            manager.globalDragIcon.transform.position = eventData.position + dragOffset;

            ShowItemDescription();
            if (manager.itemDetailsPanelObj != null)
                manager.itemDetailsPanelObj.SetActive(false);
        }

        itemIcon.color = new Color(1, 1, 1, 0.5f);
        canvasGroup.blocksRaycasts = false;

        if (manager != null)
        {
            foreach (var slot in manager.allEquipmentSlots)
            {
                if (slot.isEquipmentSlot && slot.slotType == this.currentItem.itemType)
                {
                    slot.StartPotentialHighlight();
                }
            }
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null || manager == null || manager.globalDragIcon == null) return;
        manager.globalDragIcon.transform.position = eventData.position + dragOffset;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        // Merkezi sürüklemeyi durdur
        if (manager != null)
        {
            manager.StopDragging();
            foreach (var slot in manager.allEquipmentSlots)
            {
                slot.StopPotentialHighlight();
            }
        }

        itemIcon.color = Color.white;
        canvasGroup.blocksRaycasts = true;
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        InventorySlot draggedSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (draggedSlot == null || draggedSlot.currentItem == null) return;

        // Tür Kontrolleri
        if (this.isEquipmentSlot && draggedSlot.currentItem.itemType != this.slotType) return;

        // SADECE VERİYİ MANAGER'A GÖNDER, O HALLETSİN!
        if (this.isEquipmentSlot)
        {
            // Manager hem listeyi günceller, hem SyncEverything çağırır, hem RefreshUI yapar.
            manager.EquipFromStartingItems(draggedSlot.currentItem, false);
        }
        else if (draggedSlot.isEquipmentSlot)
        {
            manager.EquipFromStartingItems(draggedSlot.currentItem, true);
        }
    }
    public virtual void UpdateSlotUI()
    {
        if (currentItem != null)
        {
            itemIcon.sprite = currentItem.icon;
            itemIcon.enabled = true;

            // --- STACK MANTIĞI ---
            // Eğer eşya stacklenebilir ise sayacı aktif et, değilse direkt kapat
            if (currentItem.isStackable)
            {
                if (itemCounterObj != null) itemCounterObj.SetActive(true);
                if (countText != null) countText.text = currentQuantity.ToString();
            }
            else
            {
                if (itemCounterObj != null) itemCounterObj.SetActive(false);
            }
            // ---------------------

            if (placeholderIcon != null)
            {
                placeholderIcon.enabled = isEquipmentSlot;
                placeholderIcon.color = new Color(1, 1, 1, 0.2f);
            }
        }
        else
        {
            // Slot boşsa hem ikonu hem sayacı kapat
            itemIcon.enabled = false;
            if (itemCounterObj != null) itemCounterObj.SetActive(false);

            if (placeholderIcon != null)
            {
                placeholderIcon.enabled = true;
                placeholderIcon.color = isEquipmentSlot ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 1f);
            }
        }
    }
    // Highlight fonksiyonları (StartPotentialHighlight, StopPotentialHighlight, PulseEffect) aynen kalıyor.
    public void StartPotentialHighlight()
    {
        if (highlightOverlay != null)
        {
            highlightOverlay.SetActive(true);
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(PulseEffect());
        }
    }

    public void StopPotentialHighlight()
    {
        if (pulseCoroutine != null) { StopCoroutine(pulseCoroutine); pulseCoroutine = null; }
        if (highlightOverlay != null && !isEquipmentSlot) highlightOverlay.SetActive(false);
    }

    private System.Collections.IEnumerator PulseEffect()
    {
        Image img = highlightOverlay.GetComponent<Image>();
        while (true)
        {
            float alpha = (Mathf.Sin(Time.time * 5f) + 1f) / 4f + 0.1f;
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
            yield return null;
        }
    }

    // InventoryManager.cs içine ekle


    public virtual void OnPointerEnter(PointerEventData eventData) { if (eventData.dragging && highlightOverlay != null) highlightOverlay.SetActive(true); }
    public virtual void OnPointerExit(PointerEventData eventData) { if (highlightOverlay != null && !isEquipmentSlot) highlightOverlay.SetActive(false); }
}