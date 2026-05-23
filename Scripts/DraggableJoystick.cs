using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private RectTransform parentRectTransform;

    public Image joystickImage;

    [Header("Görsel Parçalar")]
    [SerializeField] private GameObject backgroundObject; // Arka plan objesi

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        StatsManager.OnJoystickSettingsUpdated += ApplySettings;
        ApplySettings(); // İlk açılışta ayarları uygula
    }

    private void OnDisable()
    {
        StatsManager.OnJoystickSettingsUpdated -= ApplySettings;
    }

    private void ApplySettings()
    {
        var stats = StatsManager.Instance;

        // 1. Şeffaflık (Opacity)
        if (canvasGroup != null) canvasGroup.alpha = stats.joystickOpacity;

        // 2. Boyut (Size/Scale)
        rectTransform.localScale = new Vector3(stats.joyStickScale[0], stats.joyStickScale[1], stats.joyStickScale[2]);

        // --- RAYCAST TARGET KONTROLÜ ---
        if (joystickImage != null)
        {
            // Eğer kilitli (true) ise -> Raycast Target kapalı (false) olmalı (Tıklanamaz)
            // Eğer kilit açık (false) ise -> Raycast Target açık (true) olmalı (Taşınabilir)
            joystickImage.raycastTarget = !stats.isJoystickPositionLocked;
        }
      
        // 3. Arka Plan Görünürlüğü
        if (backgroundObject != null) backgroundObject.SetActive(stats.isJoystickBackgroundVisible);

        // 4. Kayıtlı Pozisyon (Eğer kilitliyse son pozisyona çek)
        if (stats.isJoystickPositionLocked)
        {
            rectTransform.anchoredPosition = new Vector2(stats.joyStickPosition[0], stats.joyStickPosition[1]);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Eğer kilitli değilse hareket ettir
        if (!StatsManager.Instance.isJoystickPositionLocked)
        {
            SetJoystickPosition(eventData.position);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!StatsManager.Instance.isJoystickPositionLocked)
        {
            SetJoystickPosition(eventData.position);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Pozisyonu StatsManager'a kaydet (Export için hazır olsun)
        StatsManager.Instance.joyStickPosition = new float[] {
            rectTransform.anchoredPosition.x,
            rectTransform.anchoredPosition.y,
            0
        };
    }

    private void SetJoystickPosition(Vector2 screenPos)
    {
        // 1. Ekran koordinatını (dokunulan yer) hızlıca lokal koordinata çevir
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPos
        );

        // 2. Sınırları (Radius) belirle
        // lossyScale kullanarak objenin ekrandaki gerçek boyutunu hesaba katıyoruz
        float halfWidth = (rectTransform.rect.width * rectTransform.localScale.x) / 2f;
        float halfHeight = (rectTransform.rect.height * rectTransform.localScale.y) / 2f;

        // 3. Parent'ın sınırlarını al (Genelde Canvas veya tam ekran panel)
        // Parent'ın Rect'ini kullanarak joystick'in dışarı çıkmasını engelliyoruz
        float minX = parentRectTransform.rect.xMin + halfWidth;
        float maxX = parentRectTransform.rect.xMax - halfWidth;
        float minY = parentRectTransform.rect.yMin + halfHeight;
        float maxY = parentRectTransform.rect.yMax - halfHeight;

        // 4. Pozisyonu sınırla (Clamp)
        localPos.x = Mathf.Clamp(localPos.x, minX, maxX);
        localPos.y = Mathf.Clamp(localPos.y, minY, maxY);

        // 5. Uygula
        rectTransform.anchoredPosition = localPos;
    }
}