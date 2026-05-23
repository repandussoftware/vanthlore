using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDetailsManager : MonoBehaviour
{
    public static ItemDetailsManager Instance;

    [Header("UI References")]
    public Image itemIcon;
    public TMP_Text nameText, idText, typeText, rarityText;
    public TMP_Text statsText, weightText, levelText;
    public TMP_Text descText, stackText, priceText;

    public Button checkerButton;
    public Image checkerIcon;

    public Sprite checkmarkSprite;
    public Sprite removeSprite; 

    private ItemData _currentItem; 

    void Awake() 
    {
        Instance = this;
        
        if (checkerButton != null)
        {
            checkerButton.onClick.RemoveAllListeners();
            checkerButton.onClick.AddListener(OnCheckerButtonClicked);
        }
    }

    public void ShowDetails(ItemData item)
    {
        if (item == null) return;
        _currentItem = item; 
        gameObject.SetActive(true);

        itemIcon.sprite = item.icon;
        nameText.text = item.itemName;
        idText.text = "ID: " + item.itemID;
        typeText.text = "Type: " + item.itemType.ToString();
        rarityText.text = item.rarity.ToString();
        rarityText.color = GetRarityColor(item.rarity);

        string statsStr = "";
        if (item.attackPower > 0) statsStr += "ATK: +" + item.attackPower + "  ";
        if (item.defensePower > 0) statsStr += "DEF: +" + item.defensePower;
        statsText.text = statsStr;

        weightText.text = "Weight: " + item.weight + " kg";
        levelText.text = "Required Level: " + item.levelRequirement;
        
        descText.text = item.description;
        stackText.text = item.isStackable ? "Max Stack: " + item.maxStackSize : "Single Item";
        priceText.text = $"Buy: <color=yellow>{item.buyPrice}</color> / Sell: <color=orange>{item.sellPrice}</color>";

        // --- BUTON VE İKON KONTROLÜ ---
        if (_currentItem.itemType == ItemType.Consumable) 
        {
            checkerButton.gameObject.SetActive(true);

            // Bar'da bu eşya var mı diye soruyoruz
            bool isAlreadyEquipped = PotionsBarManager.Instance != null && PotionsBarManager.Instance.IsPotionAssigned(_currentItem.itemID);

            // Varsa "Çıkar" ikonunu, yoksa "Ekle" ikonunu gösteriyoruz
            checkerIcon.sprite = isAlreadyEquipped ? removeSprite : checkmarkSprite; 
        }
        else 
        {
            checkerButton.gameObject.SetActive(false); 
        }
    }

    public void HideDetails() => gameObject.SetActive(false);

    private void OnCheckerButtonClicked()
    {
        if (_currentItem != null && PotionsBarManager.Instance != null)
        {
            if (_currentItem.itemType == ItemType.Consumable)
            {
                // Tıklanma anında tekrar kontrol ediyoruz
                bool isAlreadyEquipped = PotionsBarManager.Instance.IsPotionAssigned(_currentItem.itemID);

                if (isAlreadyEquipped)
                {
                    // Eğer bardaysa, bardan çıkar
                    PotionsBarManager.Instance.RemovePotion(_currentItem.itemID);
                }
                else
                {
                    // Barda değilse, bara ekle
                    PotionsBarManager.Instance.AssignPotion(_currentItem);
                }

                HideDetails(); 
            }
        }
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => Color.white,
            ItemRarity.Rare => Color.cyan,
            ItemRarity.Legendary => Color.yellow,
            _ => Color.white
        };
    }
}