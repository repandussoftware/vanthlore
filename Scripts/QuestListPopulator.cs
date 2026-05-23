using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestListPopulator : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject questButtonPrefab; // O parşömen satırı prefabı
    public Transform listParent;        // ScrollView'daki Content objesi
    public QuestUI questDetailsUI;     // Sağ tarafı güncelleyecek olan script

    public void Populate(QuestType type)
    {
        ClearList();

        foreach (QuestData quest in QuestManager.Instance.allQuests)
        {
            // Filtreleme Mantığı
            bool shouldDisplay = false;
            
            if (type == QuestType.COMPLETED && quest.state == QuestState.FINISHED)
                shouldDisplay = true;
            else if (quest.questType == type && quest.state != QuestState.FINISHED)
                shouldDisplay = true;
            else if (type == QuestType.MAIN && quest.questType == QuestType.MAIN) // Hepsi sekmesi gibi düşünülebilir
                shouldDisplay = true;

            if (shouldDisplay)
            {
                CreateButton(quest);
            }
        }
    }

    private void CreateButton(QuestData quest)
    {
        GameObject newBtn = Instantiate(questButtonPrefab, listParent);
        
        // Butonun üzerindeki metni ayarla (Kendi scriptine göre uyarla)
      //  QuestListButton btnScript = newBtn.GetComponent<QuestListButton>();
       // btnScript.Setup(quest);

        // Butona tıklandığında SAĞ tarafın (QuestUI) güncellenmesini sağla
        newBtn.GetComponent<Button>().onClick.AddListener(() => {
            questDetailsUI.UpdateUI(quest);
        });
    }

    private void ClearList()
    {
        foreach (Transform child in listParent)
        {
            Destroy(child.gameObject);
        }
    }
}