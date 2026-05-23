using UnityEngine;
using System.Collections;

// Bu scripti World Space Canvas içindeki, CanvasGroup eklediğin ana objeye at.
[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
public class WorldSpaceUIAnimator : MonoBehaviour
{
    [Header("Animasyon Ayarları")]
    public float fadeDuration = 0.2f;   // Açılma/Kapanma süresi
    public float bounceScale = 1.2f;    // İlk çıkışta ne kadar büyüsün? (1.2 = %20 daha büyük)
    public float settleDuration = 0.15f; // Zıpladıktan sonra yerine oturma süresi

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Coroutine currentRoutine;

    void Start()
    {
        // Canvas bileşenini bul ve sahnedeki ana kamerayı otomatik ata
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main; // Kamerayı kodla sabitler
        }
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        // Orijinal boyutunu (World Space'de çok küçük ayarlamıştık, onu) hatırla
        originalScale = rectTransform.localScale;

        // Başlangıçta gizle ve boyutunu sıfırla
        canvasGroup.alpha = 0;
        rectTransform.localScale = Vector3.zero;
    }

    // Dışarıdan çağrılacak "Görün" komutu
    public void Show()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShowRoutine());
    }

    // Dışarıdan çağrılacak "Kaybol" komutu
    public void Hide()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        float timer = 0f;
        // AŞAMA 1: Görünür ol ve hedeften biraz daha büyük ol (Bounce - Pop Up)
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            // SmoothStep harekete yumuşaklık katar
            canvasGroup.alpha = Mathf.SmoothStep(0, 1, t);
            // Hedef boyuttan biraz daha büyüğe (bounceScale) git
            rectTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale * bounceScale, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // AŞAMA 2: Orijinal boyuta geri dön (Settle - Yerleşme)
        timer = 0f;
        Vector3 currentScale = rectTransform.localScale;
        while (timer < settleDuration)
        {
            timer += Time.deltaTime;
            float t = timer / settleDuration;
            rectTransform.localScale = Vector3.Lerp(currentScale, originalScale, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // Değerleri netleştir
        rectTransform.localScale = originalScale;
        canvasGroup.alpha = 1f;
    }

    private IEnumerator HideRoutine()
    {
        float timer = 0f;
        Vector3 startScale = rectTransform.localScale;
        // Küçülerek ve sönerek kaybol
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            canvasGroup.alpha = Mathf.SmoothStep(1, 0, t);
            rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        // Tamamen gizle
        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.zero;
    }
}