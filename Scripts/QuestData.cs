// Dosya Adı: QuestData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Quest", menuName = "Aritheon/Quest")]
public class QuestData : ScriptableObject
{
    [Header("Kimlik Bilgileri")]
    public string questID;
    public string title;
    public string description; // Görevin genel açıklaması

    [Header("Görev Adımları")]
    public List<QuestStep> steps = new List<QuestStep>();
    public int currentStepIndex = 0;

    [Header("Durum")]
    public QuestState state = QuestState.REQUIREMENTS_NOT_MET;

    [Header("Ödüller")]
    public int goldReward;
    public int expReward;

    // QuestData.cs içine eklenecekler:
    public Sprite questIcon; // Görev simgesi
    public Vector2 questLocation; // Haritadaki koordinatı
    public List<Sprite> rewardIcons; // Alt taraftaki 5 kutucuk için ikonlar

    // QuestData.cs içine ekle
    public QuestType questType;

    // QuestData.cs içine ekle
    [Header("Kısıtlamalar")]
    public int requiredLevel = 1; // Bu görevi almak için gereken minimum seviye
    public QuestData prerequisiteQuest; // (Opsiyonel) Bu görevden önce bitmesi gereken başka bir görev

    public bool CanPlayerAccept(int playerLevel)
    {
        // Eğer oyuncunun seviyesi yetmiyorsa veya önceki görev bitmemişse false döner
        bool levelMet = playerLevel >= requiredLevel;
        bool prereqMet = (prerequisiteQuest == null || prerequisiteQuest.state == QuestState.FINISHED);

        return levelMet && prereqMet;
    }

    public QuestStep GetCurrentStep()
    {
        if (currentStepIndex < steps.Count)
            return steps[currentStepIndex];
        return null;
    }
}