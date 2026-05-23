using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CloudSaveData
{
    [Header("--- BASIC JOURNEY DETAILS ---")]
    public string saveName;
    public string lastScene;

    [Header("--- TRANSFORMATION & PHYSICS ---")]
    public float[] playerPosition; // float[3]
    public float[] playerRotation; // float[3]
    public float[] playerScale;    // float[3]

    [Header("--- CHARACTER BASIC STATS ---")]
    public int currentLevel;
    public int totalCoins;
    public int currentDiamonds;
    public float exchangeRate;
    public float currentHealth;
    public float maxHealth;
    public float currentMana;
    public float maxMana;
    public int currentExp;
    public int maxExp;

    [Header("--- EQUIPMENT BAYRAKLARI ---")]
    public bool isArmed;
    public bool isHelmetEquipped;
    public bool isGauntletEquipped;
    public bool isBootEquipped;
    public bool isPadEquipped;
    public bool isPauldronEquipped;

    [Header("--- INVENTORY & ITEMS ---")]
    public string[] startingItemsIDs;
    public string[] startingWearedItemsIDs;

    [Header("--- ENVIRONMENT CONTROL ---")]
    public bool isDayTime;
    public string currentLanguage;

    [Header("--- AUDIO MANAGEMENT ---")]
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;

    [Header("--- SKILLS & ACTION BARS ---")]
    public string[] usingSkillsIDs;    // string[8]
    public string[] unlockedSkillsIDs;
    public float openedSkillSlots;
    public string[] quickBarPotionIDs;  // string[4]

    [Header("--- MOBILE JOYSTICK HUD CONFIGS ---")]
    public float[] joyStickPosition; // float[3]
    public float[] joyStickScale;    // float[3]
    public float[] joyStickRotation; // float[3]
    public float joystickOpacity;
    public bool isJoystickPositionLocked;
    public bool isJoystickBackgroundVisible;

    [Header("--- SKILL HUD CONFIGS ---")]
    public float skillHUDScale;
    public float skillHUDOpacity;
    public bool isSkillHUDLocked;

    [Header("--- ADVANCED SKILL OBJECT LISTS ---")]
    public List<StatsManager.SkillSlotPosition> savedSkillPositions;
}