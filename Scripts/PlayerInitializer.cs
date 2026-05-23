using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [Header("Hedef Obje Ayarları")]
    public string playerTag = "Player";
    public string visualsChildName = "Visuals"; // Hiyerarşideki ismi

    [Header("Başlangıç Yerel Değerleri (Local)")]
    public Vector3 startLocalPosition = new Vector3(-9.2f, 3.51f, 0f);
    public Vector3 startLocalRotation = Vector3.zero;
    public Vector3 startLocalScale = new Vector3(0.65f, 0.65f, 0.65f);

    [Header("Hız Ayarları")]
    public float walkSpeed = 8f; 
    public float runSpeed = 15f;

    [Header("Fizik Ayarları")]
    public float gravityScale = 5f; 
    public bool shouldResetVelocity = true;

    [Header("Zorunlu Görsel Değerleri")]
    public float forcedVisualScale = 0.81438f; // Senin istediğin o özel rakam

    private Transform _visualsContainer;

    void Start()
    {
        GameObject player = GameObject.FindWithTag(playerTag);

        if (player != null && DarionController.Instance != null)
        {
            // Visuals objesini otomatik bul
            _visualsContainer = player.transform.Find(visualsChildName);

            // Ana Player Transform değerlerini uygula
            player.transform.localPosition = startLocalPosition;
            player.transform.localEulerAngles = startLocalRotation;
            player.transform.localScale = startLocalScale;

            // Fizik ve Hızları set et
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) 
            {
                rb.gravityScale = gravityScale;
                if (shouldResetVelocity) rb.linearVelocity = Vector2.zero; // Unity 6 standardı
            }

            StatsManager.Instance.walkSpeed = walkSpeed;
            StatsManager.Instance.runSpeed = runSpeed;
        }
    }

    // --- KRİTİK: HER KAREDE DEĞERLERİ SABİTLEYEN BÖLÜM ---
}