using UnityEngine;
using UnityEngine.UI;

public class QuestMarker : MonoBehaviour
{
    public Image markerImage; // Minimap üzerindeki ikon
    public QuestData activeQuest;

    void Update()
    {
        if (activeQuest != null && activeQuest.state == QuestState.IN_PROGRESS)
        {
            markerImage.gameObject.SetActive(true);
            // Harita üzerindeki pozisyonu ayarla (Kendi harita sistemine göre uyarla)
            // markerImage.rectTransform.localPosition = activeQuest.questLocation;
        }
        else
        {
            markerImage.gameObject.SetActive(false);
        }
    }
}