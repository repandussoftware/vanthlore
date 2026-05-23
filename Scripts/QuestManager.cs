// Dosya Adı: QuestManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    public List<QuestData> allQuests; // Projedeki tüm görevleri buraya sürükle

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Belirli bir NPC için o an aktif bir görev adımı var mı?
    public QuestData GetActiveQuestForNPC(string npcID)
    {
        foreach (var quest in allQuests)
        {
            if (quest.state == QuestState.IN_PROGRESS || quest.state == QuestState.CAN_START)
            {
                QuestStep currentStep = quest.GetCurrentStep();
                if (currentStep != null && currentStep.targetNPCID == npcID)
                    return quest;
            }
        }
        return null;
    }

    public void AdvanceQuest(string questID)
    {
        QuestData quest = allQuests.Find(q => q.questID == questID);
        if (quest == null) return;

        if (quest.state == QuestState.CAN_START)
        {
            quest.state = QuestState.IN_PROGRESS;
        }
        else if (quest.state == QuestState.IN_PROGRESS)
        {
            quest.currentStepIndex++;
            if (quest.currentStepIndex >= quest.steps.Count)
            {
                quest.state = QuestState.CAN_FINISH;
            }
        }
    }

    internal QuestData GetAvailableQuestForNPC(string npcID)
    {
        throw new NotImplementedException();
    }
}