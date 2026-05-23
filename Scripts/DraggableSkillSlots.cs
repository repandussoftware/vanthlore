using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableSkillSlot : MonoBehaviour, IDragHandler, IPointerUpHandler, IDropHandler
{
    [Header("ID Ayarı")]
    public string slotID; // Inspector'dan her butona benzersiz bir isim ver (Örn: Slot_1)

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image slotImage;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        slotImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        StatsManager.OnSkillHUDUpdated += ApplySkillSettings;
        ApplySkillSettings();
    }

    private void OnDisable() => StatsManager.OnSkillHUDUpdated -= ApplySkillSettings;

    public void ApplySkillSettings()
    {
        var stats = StatsManager.Instance;
        if (stats == null) return;

        // 1. Görünürlük ve Tıklanabilirlik
        canvasGroup.alpha = stats.skillHUDOpacity;
        
        // Kilitliyse tıklanmasın (Raycast Target kapalı), kilit açıkken taşınabilsin diye açık
        if (slotImage != null) slotImage.raycastTarget = !stats.isSkillHUDLocked;

        // 2. Boyut
        rectTransform.localScale = Vector3.one * stats.skillHUDScale;

        // 3. Pozisyonu Veritabanından Yükle
        if (!string.IsNullOrEmpty(slotID))
        {
            var myData = stats.savedSkillPositions.Find(x => x.slotID == slotID);
            if (myData != null)
            {
                rectTransform.anchoredPosition = new Vector2(myData.posX, myData.posY);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!StatsManager.Instance.isSkillHUDLocked)
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform.parent, 
                eventData.position, 
                canvas.worldCamera, 
                out localPos
            );
            rectTransform.anchoredPosition = localPos;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Boş bırakıldığında veya başka bir slotun üzerine bırakıldığında kaydet
        if (!StatsManager.Instance.isSkillHUDLocked)
        {
            SaveCurrentPosition();

            // --- KRİTİK NOKTA: Pozisyonu kalıcı olarak diske işle ---
            if (MenuController.Instance != null)
            {
                MenuController.Instance.SaveOnlySettings();
                Debug.Log($"<color=green>Aritheon HUD:</color> {slotID} yeni konumu kaydedildi.");
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Kilit açıkken sürükleme biterse kaydet
        if (!StatsManager.Instance.isSkillHUDLocked)
        {
            SaveCurrentPosition();

            // --- KRİTİK NOKTA: Pozisyonu kalıcı olarak diske işle ---
            if (MenuController.Instance != null)
            {
                MenuController.Instance.SaveOnlySettings();
                Debug.Log($"<color=green>Aritheon HUD:</color> {slotID} yeni konumu kaydedildi.");
            }
        }
    }

    private void SaveCurrentPosition()
    {
        if (string.IsNullOrEmpty(slotID))
        {
            Debug.LogWarning("<color=red>Hata:</color> Bu butonun Slot ID'si boş! Kaydedilemez.");
            return;
        }

        var stats = StatsManager.Instance;
        var myData = stats.savedSkillPositions.Find(x => x.slotID == slotID);
        
        // Eğer listede bu ID yoksa yeni bir tane oluştur
        if (myData == null)
        {
            myData = new StatsManager.SkillSlotPosition { slotID = slotID };
            stats.savedSkillPositions.Add(myData);
        }
        
        // Mevcut pozisyonu veriye işle
        myData.posX = rectTransform.anchoredPosition.x;
        myData.posY = rectTransform.anchoredPosition.y;
    }
}