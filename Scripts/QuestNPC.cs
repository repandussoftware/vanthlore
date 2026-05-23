// Dosya Adı: QuestNPC.cs
using UnityEngine;

public class QuestNPC : MonoBehaviour
{
    public string npcID; // Unity Inspector'da her NPC'ye eşsiz isim ver (Örn: ELDER_1)
    public string defaultDialogue = "Güzel bir gün, değil mi Darion?";

    // QuestNPC.cs içindeki Interact metodunu güncelle
    public void Interact()
    {
        int currentLevel = StatsManager.Instance.currentLevel; // Oyuncunun levelini aldığımızı varsayalım
        QuestData potentialQuest = QuestManager.Instance.GetAvailableQuestForNPC(npcID);

        if (potentialQuest != null)
        {
            if (potentialQuest.CanPlayerAccept(currentLevel))
            {
                // Seviye yetiyor, görevi ver veya ilerlet
                QuestManager.Instance.AdvanceQuest(potentialQuest.questID);
                //UIManager.Instance.ShowDialogue(npcID, potentialQuest.GetCurrentStep().dialogueText);
            }
            else
            {
                // SEVİYE YETMİYOR: Burada oyuncuyu ana göreve yönlendirebiliriz
                string warning = $"Henüz hazır değilsin Darion. En az {potentialQuest.requiredLevel}. seviye olmalısın. Belki ana hikayeyi takip ederek güçlenebilirsin...";
                //UIManager.Instance.ShowDialogue(npcID, warning);
            }
        }
        else
        {
            //UIManager.Instance.ShowDialogue(npcID, defaultDialogue);
        }
    }
}