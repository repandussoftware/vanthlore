using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Oyuncu Seviye Bilgileri")]

    public const int maxExp = 100; // Senin isteğin üzerine 100'e sabitledik canım

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GiveExperience(int enemyLevel, EnemyStats.EnemyType rank)
    {
        // 1. Seviye farkını bulalım
        int levelDiff = enemyLevel - StatsManager.Instance.currentLevel;

        // 2. Rank çarpanını belirleyelim
        float rankMultiplier = 1f;
        switch (rank)
        {
            case EnemyStats.EnemyType.Normal: rankMultiplier = 1f; break;
            case EnemyStats.EnemyType.Elite: rankMultiplier = 3f; break;
            case EnemyStats.EnemyType.Boss: rankMultiplier = 10f; break;
        }

        // 3. EXP Hesaplama Formülü:
        // Seviye farkına göre EXP artar veya azalır. 
        // Eğer fark -5 ise (düşman çok zayıfsa) neredeyse hiç EXP gelmez.
        float rawExp = StatsManager.Instance.baseExp + (levelDiff * 5); 
        
        // Negatif EXP gelmesini engelleyelim (En az 1 EXP versin canım)
        int finalExp = Mathf.RoundToInt(Mathf.Max(rawExp * rankMultiplier, 1));

        // Eğer oyuncu düşmandan çok üstünse (Örn: 10 level fark) cezayı keselim
        if (levelDiff <= -5) finalExp = Mathf.Max(finalExp / 4, 1);

        AddExp(finalExp);
    }

    private void AddExp(int amount)
    {
        StatsManager.Instance.currentExp += amount;
        Debug.Log($"<color=cyan>{amount} EXP kazanıldı!</color>");

        while (StatsManager.Instance.currentExp >= maxExp)
        {
            LevelUp();
        }

        // UIManager'ı güncelle
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateExperienceUI(StatsManager.Instance.currentLevel, StatsManager.Instance.currentExp, maxExp);
    }

    void LevelUp()
    {
        StatsManager.Instance.currentExp -= maxExp;
        StatsManager.Instance.currentLevel++;
        PotionsBarManager.Instance.UpdateUnlockedSlots();
        Debug.Log("<color=yellow>TEBRİKLER! Seviye Atladın!</color>");
        // Buraya Darion'un gücünü artıran kodları ekleyebiliriz hocam
    }
}