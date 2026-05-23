using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullandığını varsayıyorum

public class QuestUI : MonoBehaviour
{
    [Header("Sol Panel (Görev Listesi)")]
    public GameObject questTitlePrefab;
    public Transform questListParent;

    [Header("Sağ Panel (Detaylar)")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI currentObjectiveText;

    [Header("Alt Panel (Ödüller)")]
    public Image[] rewardSlots; // Görseldeki 5 kutucuk

    public void UpdateUI(QuestData selectedQuest)
    {
        // 1. Yazıları doldur
        titleText.text = selectedQuest.title;
        descriptionText.text = selectedQuest.description;
        
        // O anki adımın metnini göster
        if(selectedQuest.GetCurrentStep() != null)
            currentObjectiveText.text = "Hedef: " + selectedQuest.GetCurrentStep().dialogueText;

        // 2. Ödül İkonlarını yerleştir
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            if (i < selectedQuest.rewardIcons.Count)
            {
                rewardSlots[i].sprite = selectedQuest.rewardIcons[i];
                rewardSlots[i].enabled = true;
            }
            else
            {
                rewardSlots[i].enabled = false; // Boş kutuları gizle
            }
        }
    }
}