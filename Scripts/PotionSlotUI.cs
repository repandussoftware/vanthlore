using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

public class PotionSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Referansları")]
    public Image potionIcon;
    public Image potionBackground; // Slotun arka planı (isteğe bağlı, görsel düzenleme için)
    public GameObject countObj;
    public TextMeshProUGUI countText;

    [Header("Cooldown Ayarları")]
    public Image cooldownImage;
    // public float cooldownDuration = 2f; --> BUNU SİLDİK, veriyi ItemData'dan alacağız!

    private float _currentCooldown = 0f;
    private float _maxCooldown = 0f; // Yüzdelik dolum hesabı için potun orijinal süresini burada tutacağız
    private bool _isCooldown = false;

    [Header("Atanan Eşya")]
    public ItemData assignedPotion;

    private Image _backgroundImage;

    private void Awake()
    {
        _backgroundImage = GetComponent<Image>();
        if (cooldownImage != null) cooldownImage.fillAmount = 0f;
    }

    private void Update()
    {
        if (_isCooldown)
        {
            _currentCooldown -= Time.deltaTime;

            // Eğer maxCooldown 0'dan büyükse bölme işlemi yap (Sıfıra bölünme hatasını önlemek için)
            if (cooldownImage != null && _maxCooldown > 0f)
            {
                cooldownImage.fillAmount = _currentCooldown / _maxCooldown;
            }

            if (_currentCooldown <= 0f)
            {
                _isCooldown = false;
                if (cooldownImage != null) cooldownImage.fillAmount = 0f;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData) => UsePotion();

    public void UsePotion()
    {
        if (assignedPotion == null || StatsManager.Instance == null) return;

        if (_isCooldown) return;

        ItemData potToUse = StatsManager.Instance.startingItems.FirstOrDefault(x => x != null && x.itemID == assignedPotion.itemID);

        if (potToUse != null)
        {
            Debug.Log($"<color=red>{assignedPotion.itemName} KULLANILDI!</color>");
            // -> BURAYA CAN/MANA DOLDURMA KODU GELECEK <-

            // Eğer bu eşyanın can veya mana verip vermediğini belirleyen bir değişkenin varsa (örneğin potToUse.restoreHealthAmount)
            if (potToUse.healthBonus > 0)
            {
                DarionController.Instance.RestoreHealth(potToUse.healthBonus);
            }

            if (potToUse.manaBonus > 0)
            {
                DarionController.Instance.RestoreMana(potToUse.manaBonus);
            }

            StatsManager.Instance.startingItems.Remove(potToUse);

            // --- COOLDOWN BAŞLAT (Veriyi ItemData'dan çekiyoruz) ---
            _isCooldown = true;
            _maxCooldown = assignedPotion.duration; // Senin ItemData'ndaki değişken
            _currentCooldown = _maxCooldown;

            if (cooldownImage != null) cooldownImage.fillAmount = 1f;

            PotionsBarManager.Instance.RefreshAllSlots();
            if (InventoryManager.Instance != null) InventoryManager.Instance.RefreshInventoryUI();
        }
    }

public void RefreshSlot()
    {
        if (assignedPotion != null)
        {
            // EŞYA VARSA: Arka planı ve ikonu göster
            if (_backgroundImage != null) _backgroundImage.enabled = true;
            
            // 👇 İŞTE EKSİK OLAN SATIR BURASI CANIM 👇
            if (potionBackground != null) potionBackground.enabled = true; 
            
            int currentCount = 0;
            if (StatsManager.Instance != null && StatsManager.Instance.startingItems != null)
                currentCount = StatsManager.Instance.startingItems.Count(x => x != null && x.itemID == assignedPotion.itemID);

            potionIcon.sprite = assignedPotion.icon;
            potionIcon.enabled = true;

            if (currentCount > 0)
            {
                potionIcon.color = Color.white;
                if (countObj != null) countObj.SetActive(true);
                if (countText != null) countText.text = currentCount.ToString();
            }
            else
            {
                // Pot bittiyse gri/silik göster
                potionIcon.color = new Color(1, 1, 1, 0.3f);
                if (countObj != null) countObj.SetActive(false);
            }
        }
        else
        {
            // 🎯 EŞYA YOKSA: Slot arka planda "Açık" kalır ama görselleri gizlenir (Hayalet Modu)
            if (_backgroundImage != null) _backgroundImage.enabled = false;
            if (potionIcon != null) potionIcon.enabled = false;
            if (potionBackground != null) potionBackground.enabled = false; // Burada kapatmıştık!
            if (countObj != null) countObj.SetActive(false);

            // Eğer cooldown görseli de varsa onu da sıfırlayıp/gizliyoruz
            if (cooldownImage != null) cooldownImage.fillAmount = 0f;
        }
    }
}