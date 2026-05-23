// Dosya Adı: QuestStep.cs
using UnityEngine;

[System.Serializable]
public class QuestStep
{
    public string targetNPCID; // Bu adımda hangi NPC ile konuşulacak? (Örn: "LYSANDRA")
    [TextArea(3, 10)]
    public string dialogueText; // NPC'nin bu adımda söyleyeceği özel metin
    public bool isCompleted = false;
}