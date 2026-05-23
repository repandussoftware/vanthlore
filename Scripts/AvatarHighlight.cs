using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AvatarHighlight : MonoBehaviour
{
    private RawImage rawImage;
    private Color originalColor;
    private Coroutine pulseCoroutine;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        if (rawImage != null) originalColor = rawImage.color;
    }

    public void StartHighlight()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseEffect());
    }

    public void StopHighlight()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        if (rawImage != null) rawImage.color = originalColor; // Rengi eski haline döndür
    }

    private IEnumerator PulseEffect()
    {
        while (true)
        {
            // Sinüs dalgası ile yumuşak bir yanıp sönme efekti
            float pingPong = (Mathf.Sin(Time.time * 6f) + 1f) / 2f;
            // Orijinal renk ile hafif şeffaf/parlak bir renk arasında gidip gelir
            rawImage.color = Color.Lerp(originalColor, new Color(1f, 1f, 1f, 0.4f), pingPong);
            yield return null;
        }
    }
}