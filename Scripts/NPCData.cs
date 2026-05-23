using System.Collections.Generic;
using UnityEngine;

// Ruh hallerini tanımlıyoruz
public enum NPCOffset
{
    Default,
    Happy,
    Sad,
    Angry,
    Surprised,
    Thinking,
    Tired,
    Feared,
    Begged,
    Crying
}

[System.Serializable]
public struct NPCPortrait
{
    public NPCOffset mood;
    public Sprite portraitSprite;
}

[CreateAssetMenu(fileName = "New NPC Data", menuName = "Aritheon/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Kimlik Bilgileri")]
    public string npcName;


    [Header("Portreler (Ruh Halleri)")]
    public List<NPCPortrait> portraits; // Buradan istediğin kadar ruh hali ekleyebilirsin

    [Header("Ticaret Ayarları")]
    public Sprite tradeWindowBanner; // Trade modu açıldığında üstte görünecek sabit görsel
    public List<ItemData> shopItems; 

    // Yardımcı fonksiyon: Mood'a göre doğru sprite'ı getirir
    public Sprite GetPortrait(NPCOffset mood)
    {
        foreach (var p in portraits)
        {
            if (p.mood == mood) return p.portraitSprite;
        }
        return portraits[0].portraitSprite; // Bulamazsa ilkini döndür
    }
}