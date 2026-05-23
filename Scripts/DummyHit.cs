using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Slider için gerekli
using TMPro; // İsim yazısı (TextMeshPro) için gerekli

public class DummyHit : MonoBehaviour
{
    [Header("Görsel Efektler")]
    [SerializeField] private ParticleSystem hitEffect;
    
    [Header("Sarsılma Ayarları")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.1f;
    
    [Header("UI Ayarları")]
    [SerializeField] private GameObject uiRoot; // Canvas'ın tamamını içeren obje
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI nameText; // Eğer isim değiştirmek istersen
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Start()
    {
        // Başlangıç değerlerini ata
        originalPosition = transform.localPosition;
        currentHealth = maxHealth;

        // Slider ayarlarını yap
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        // Başlangıçta UI kapalı olsun
        if (uiRoot != null) uiRoot.SetActive(false);
    }

    // Darion menzile girdiğinde UI açılır
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (uiRoot != null) uiRoot.SetActive(true);
        }
    }

    // Darion menzilden çıktığında UI kapanır
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (uiRoot != null) uiRoot.SetActive(false);
        }
    }

    public void PlayHitEffect()
    {
        // 1. Can Azaltma ve UI Güncelleme
        currentHealth -= 10f; // Her vuruşta 10 can gider
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            currentHealth = maxHealth; // Öldüğünde canı fullüyoruz (Eğitim mankeni olduğu için)
            if (healthSlider != null) healthSlider.value = maxHealth;
        }

        // 2. Partikül Efektini Çalıştır
        if (hitEffect != null)
        {
            hitEffect.Stop();
            hitEffect.Play();
        }

        // 3. Sarsılma Efektini Başlat
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition;
    }
}