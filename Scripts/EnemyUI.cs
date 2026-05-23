using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUI : MonoBehaviour
{
    [Header("Referanslar")]
    public EnemyStats stats; 
    public Slider healthSlider;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI rankText;

    void Start()
    {
        if (stats != null && stats.data != null)
        {
            // Verileri EnemyData ve Stats üzerinden başlatıyoruz
            nameText.text = stats.data.enemyName; 
            rankText.text = "Lv." + stats.level + " [" + stats.type.ToString() + "]";
            
            healthSlider.maxValue = stats.data.maxHealth;
            healthSlider.value = stats.currentHealth;
        }
    }

    void LateUpdate() 
    {
        // 1. Can Barını Doğrudan Güncelleme
        if (stats != null && healthSlider.value != stats.currentHealth)
        {
            healthSlider.value = stats.currentHealth;
        }

        // 2. Rotasyon Sabitleme (UI'ın ters dönmemesi için)
        FixRotation();
    }

    void FixRotation()
    {
        // World Space Canvas'ın her zaman kameraya düz bakmasını sağlar
        transform.rotation = Quaternion.identity;
    }
}