using UnityEngine;
using TMPro;
using System.Collections;

public class WarningToast : MonoBehaviour
{
    [Header("Referanslar")]
    public TextMeshProUGUI toastText;
    public CanvasGroup canvasGroup; // Prefabın üzerine CanvasGroup eklemeyi unutma canım

    [Header("Ayarlar")]
    public float fadeSpeed = 1.5f;
    public float waitTime = 3.0f;

    public void Initialize(string message)
    {
        toastText.text = message;
        canvasGroup.alpha = 0; // Başlangıçta görünmez
        StartCoroutine(ToastSequence());
    }

    private IEnumerator ToastSequence()
    {
        // 1. Fade In
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // 2. Bekleme
        yield return new WaitForSeconds(waitTime);

        // 3. Fade Out
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // 4. Temizlik
        Destroy(gameObject);
    }
}