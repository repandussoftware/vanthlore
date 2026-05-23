using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableHUDElement : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private RectTransform parentRectTransform;

    [Header("Şeffaflık Ayarları")]
    public float normalOpacity = 0.5f;
    public float activeOpacity = 1.0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Eğer Canvas Group yoksa otomatik ekleyelim, TCL'de hata almayalım
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        canvas = GetComponentInParent<Canvas>();
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
        
        canvasGroup.alpha = normalOpacity;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        canvasGroup.alpha = activeOpacity;
        // İlk dokunulduğunda slotu en öne getir (diğer slotların altında kalmasın)
        transform.SetAsLastSibling();
        SetPosition(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetPosition(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        canvasGroup.alpha = normalOpacity;
    }

    private void SetPosition(Vector2 screenPos)
    {
        // 1. Ekran piksellerini ve slotun boyutunu al
        float radiusX = (rectTransform.rect.width * canvas.scaleFactor) / 2f;
        float radiusY = (rectTransform.rect.height * canvas.scaleFactor) / 2f;

        // 2. Kameranın/Ekranın sınırlarına hapset (Clamp)
        float clampedX = Mathf.Clamp(screenPos.x, radiusX, Screen.width - radiusX);
        float clampedY = Mathf.Clamp(screenPos.y, radiusY, Screen.height - radiusY);
        Vector2 clampedScreenPos = new Vector2(clampedX, clampedY);

        // 3. Pozisyonu güvenli bir şekilde hesapla (MacBook Air vs TCL uyumu)
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform, 
            clampedScreenPos, 
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, 
            out localPos
        );

        rectTransform.anchoredPosition = localPos;
    }
}