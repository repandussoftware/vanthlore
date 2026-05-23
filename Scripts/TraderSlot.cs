using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TraderSlot : MonoBehaviour
{
    [Header("UI Referansları")]
    public Image itemImage;
    public TextMeshProUGUI priceText;

    // Yeni Eklenen Referanslar
    [Header("Yığın (Stack) Ayarları")]
    public GameObject counterObj; // Prefab'deki 'Counter' objesi
    public TextMeshProUGUI counterText; // Counter içindeki Text (TMP)

    private ItemData _currentItem;
    private NPCInteraction _manager;
    private bool _isSelling;

    public void Setup(ItemData item, NPCInteraction manager, bool isSelling, int quantity = 1)
    {
        _currentItem = item;
        _manager = manager;
        _isSelling = isSelling;

        if (itemImage != null) itemImage.sprite = item.icon; 
        
        if (priceText != null)
            priceText.text = (isSelling ? item.sellPrice : item.buyPrice).ToString();

        // --- SAYI GÖSTERGE MANTIĞI ---
        // Eğer eşya stacklenebilir ise ve adedi 1'den büyükse sayacı göster
        if (item != null && item.isStackable && quantity > 1)
        {
            if (counterObj != null) counterObj.SetActive(true);
            if (counterText != null) counterText.text = quantity.ToString();
        }
        else
        {
            // Tek bir item varsa veya stacklenemiyorsa sayacı gizle
            if (counterObj != null) counterObj.SetActive(false);
        }
    }

    public void OnSlotClicked()
    {
        if (_currentItem != null && _manager != null)
        {
            if (_isSelling)
                _manager.SellItem(_currentItem);
            else
                _manager.BuyItem(_currentItem);
        }
    }
}