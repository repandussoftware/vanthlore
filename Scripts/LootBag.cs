using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class LootBag : MonoBehaviour
{
    [Header("Ganimet Ayarları")]
    public float interactionDistance = 2f;
    public GameObject takeButtonUI; // Kesenin üzerindeki "Take" butonu
    public Rigidbody2D rb;

    [Header("Düşman Verileri")]
    private int droppedLevel;
    private EnemyStats.EnemyType droppedRank;
    private int calculatedCoins;

    private Transform player;

    void Start()
    {
        player = DarionController.Instance.transform;

        // 1. Kesenin fırlama efekti (Physics)
        Vector2 throwDirection = new Vector2(Random.Range(-2f, 2f), 5f);
        rb.AddForce(throwDirection, ForceMode2D.Impulse);
    }

    void Update()
    {
        if (player == null) return;

        // 2. Mesafeye göre "Take" butonunu göster/gizle
        float distance = Vector2.Distance(transform.position, player.position);
        takeButtonUI.SetActive(distance <= interactionDistance);
    }

    public void InitializeLoot(int level, EnemyStats.EnemyType rank)
    {
        droppedLevel = level;
        droppedRank = rank;

        // 2. Coin Hesaplama Mantığı
        CalculateCoins();
    }

    void CalculateCoins()
    {
        // Rank çarpanı belirleyelim
        float rankMultiplier = 1f;
        switch (droppedRank)
        {
            case EnemyStats.EnemyType.Normal: rankMultiplier = 1f; break;
            case EnemyStats.EnemyType.Elite: rankMultiplier = 3.5f; break;
            case EnemyStats.EnemyType.Boss: rankMultiplier = 10f; break;
        }

        // Formül: (Level * Rastgele Değer) * Rank Çarpanı
        // Örn: Level 5 Normal -> (5 * 10) * 1 = 50 Coin
        // Örn: Level 5 Boss -> (5 * 15) * 10 = 750 Coin
        int baseAmount = Random.Range(5, 15);
        calculatedCoins = Mathf.RoundToInt((droppedLevel * baseAmount) * rankMultiplier);
    }

    // Butona basıldığında çalışacak fonksiyon
    public void OnTakeButtonClicked()
    {
        // 3. Envanter UI'ını aç ve içini doldur
        OpenLootInventory();
    }

    void OpenLootInventory()
    {
        Debug.Log($"Pocket Açıldı! İçinden {calculatedCoins} coin çıktı.");

        // UI'ya bu değeri gönderiyoruz (UIManager'da bu fonksiyonu yazacağız canım)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootPopup(calculatedCoins);
            Destroy(gameObject);
        }
    }
}