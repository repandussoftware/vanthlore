using UnityEngine;
using System.Numerics;
using System.Collections.Generic;
using System;
using Vanthlore.Auth; 

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    public static System.Action OnDataImported;
    public static System.Action OnJoystickSettingsUpdated;
    public static System.Action OnSkillHUDUpdated; 

    [Header("Player Stats")]
    public float forcedScale = 0.81438f;
    public float walkSpeed = 8f;
    public float runSpeed = 15f;
    [Range(0.1f, 1f)] public float runThreshold = 0.9f;
    public float stopDistance = 0.2f;
    public float jumpForce = 12f;
    public bool isGrounded;
    public float gravity;
    public bool isFacingRight = true;

    [Header("Live Transform Cache")]
    public UnityEngine.Vector3 transformLocalPosition;
    public UnityEngine.Vector3 transformLocalEulerAngles;
    public UnityEngine.Vector3 transformLocalScale;

    public float baseSkillPower = 30; 

    [Header("Base Natural Stats")]
    public float nakedMaxHealth = 100f; 
    public float nakedMaxMana = 100f;   
    public float currentHealth = 0;
    public float maxHealth = 100;
    public float currentMana = 0;
    public float maxMana = 100;
    public string[] usingSkillsIDs;
    public float openedSkillSlots = 0f;
    public string[] unlockedSkillsIDs;

    [Header("Quick Bar Potions")]
    public string[] quickBarPotionIDs = new string[4];

    [Header("Attributes & Currencies")]
    public float allAttack = 0f;
    public float allDefense = 0f;
    public float FireAttack = 0f;
    public float IceAttack = 0f;
    public int totalCoins = 0;
    public int currentDiamonds = 0;
    public float exchangeRate = 15f;
    public int currentLevel = 1;
    public int currentExp = 0;
    public int maxExp = 100;
    public int baseExp = 20;
    public float currentActiveSpeed;
    
    private bool _isArmed = false;
    private bool _isHelmetEquipped = false;
    private bool _isGauntletEquipped = false;
    private bool _isBootEquipped = false;
    private bool _isPadEquipped = false;
    private bool _isPauldronEquipped = false;

    public bool isOnSpecialPath = false;

    public bool isArmed { get => _isArmed; set { _isArmed = value; } }
    public bool isHelmetEquipped { get => _isHelmetEquipped; set { _isHelmetEquipped = value; } }
    public bool isGauntletEquipped { get => _isGauntletEquipped; set { _isGauntletEquipped = value; } }
    public bool isBootEquipped { get => _isBootEquipped; set { _isBootEquipped = value; } }
    public bool isPadEquipped { get => _isPadEquipped; set { _isPadEquipped = value; } }
    public bool isPauldronEquipped { get => _isPauldronEquipped; set { _isPauldronEquipped = value; } }

    public string currentLanguage = "tr"; 

    [Header("World Stats")]
    public bool isDayTime = false; 

    [HideInInspector] public string[] startingItemsID;
    [HideInInspector] public string[] startingWearedItemsID;

    [Header("Eşya Listeleri (Runtime)")]
    public List<ItemData> startingItems = new List<ItemData>();
    public List<ItemData> startingWearedItems = new List<ItemData>();

    [Header("Audio Settings")]
    public float masterVolume = 0.1f;
    public float musicVolume = 0.1f;
    public float sfxVolume = 0.1f;

    [Header("Joystick Settings")]
    public float[] joyStickPosition = new float[] { 361.6331f, 332.7912f, 0f };
    public float[] joyStickScale = new float[] { 3.183873f, 3.1838731f, 3.1838731f };
    public float joystickOpacity = 0.5f;
    private bool _isJoystickPositionLocked = true;
    public bool isJoystickPositionLocked { get => _isJoystickPositionLocked; set { _isJoystickPositionLocked = value; } }
    public bool isJoystickBackgroundVisible = true;

    [System.Serializable]
    public class SkillSlotPosition
    {
        public string slotID; 
        public float posX;
        public float posY;
    }

    [Header("Skill HUD Settings")]
    public List<SkillSlotPosition> savedSkillPositions = new List<SkillSlotPosition>();
    public float skillHUDScale = 0.4f;
    public float skillHUDOpacity = 1.0f;
    public bool isSkillHUDLocked = true;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        if (usingSkillsIDs == null || usingSkillsIDs.Length < 8)
        {
            string[] newArray = new string[8];
            if (usingSkillsIDs != null) System.Array.Copy(usingSkillsIDs, newArray, usingSkillsIDs.Length);
            usingSkillsIDs = newArray;
        }
    }

    // Bulut save paketleyicisi canım
    public CloudSaveData ExportCurrentStatsToCloud()
    {
        foreach (var wearedItem in startingWearedItems)
        {
            if (startingItems.Contains(wearedItem)) startingItems.Remove(wearedItem);
        }

        this.startingItemsID = ConvertListToIDs(this.startingItems);
        this.startingWearedItemsID = ConvertListToIDs(this.startingWearedItems);

        CloudSaveData cloudData = new CloudSaveData();
        cloudData.saveName = "Online Journey";
        cloudData.lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            cloudData.playerPosition = new float[] { player.transform.position.x, player.transform.position.y, player.transform.position.z };
            cloudData.playerRotation = new float[] { player.transform.eulerAngles.x, player.transform.eulerAngles.y, player.transform.eulerAngles.z };
            cloudData.playerScale = new float[] { player.transform.localScale.x, player.transform.localScale.y, player.transform.localScale.z };
        }
        else
        {
            cloudData.playerPosition = new float[] { transformLocalPosition.x, transformLocalPosition.y, transformLocalPosition.z };
            cloudData.playerRotation = new float[] { transformLocalEulerAngles.x, transformLocalEulerAngles.y, transformLocalEulerAngles.z };
            cloudData.playerScale = new float[] { transformLocalScale.x, transformLocalScale.y, transformLocalScale.z };
        }

        cloudData.currentLevel = this.currentLevel;
        cloudData.totalCoins = this.totalCoins;
        cloudData.currentDiamonds = this.currentDiamonds;
        cloudData.exchangeRate = this.exchangeRate;
        cloudData.currentHealth = this.currentHealth;
        cloudData.maxHealth = this.maxHealth;
        cloudData.currentMana = this.currentMana;
        cloudData.maxMana = this.maxMana;
        cloudData.currentExp = this.currentExp;
        cloudData.maxExp = this.maxExp;
        cloudData.isDayTime = this.isDayTime;
        cloudData.isArmed = this.isArmed;
        cloudData.isHelmetEquipped = this.isHelmetEquipped;
        cloudData.isGauntletEquipped = this.isGauntletEquipped;
        cloudData.isBootEquipped = this.isBootEquipped;
        cloudData.isPadEquipped = this.isPadEquipped;
        cloudData.isPauldronEquipped = this.isPauldronEquipped;
        cloudData.startingItemsIDs = this.startingItemsID;
        cloudData.startingWearedItemsIDs = this.startingWearedItemsID;
        cloudData.currentLanguage = this.currentLanguage;
        cloudData.masterVolume = this.masterVolume;
        cloudData.musicVolume = this.musicVolume;
        cloudData.sfxVolume = this.sfxVolume;
        cloudData.usingSkillsIDs = this.usingSkillsIDs;
        cloudData.unlockedSkillsIDs = this.unlockedSkillsIDs;
        cloudData.openedSkillSlots = this.openedSkillSlots;
        cloudData.quickBarPotionIDs = this.quickBarPotionIDs;
        cloudData.joyStickPosition = this.joyStickPosition;
        cloudData.joyStickScale = this.joyStickScale;
        cloudData.joyStickRotation = new float[] { 0f, 0f, 0f };
        cloudData.joystickOpacity = this.joystickOpacity;
        cloudData.isJoystickPositionLocked = this.isJoystickPositionLocked;
        cloudData.isJoystickBackgroundVisible = this.isJoystickBackgroundVisible;
        cloudData.skillHUDScale = this.skillHUDScale;
        cloudData.skillHUDOpacity = this.skillHUDOpacity;
        cloudData.isSkillHUDLocked = this.isSkillHUDLocked;
        cloudData.savedSkillPositions = new List<SkillSlotPosition>(this.savedSkillPositions);

        return cloudData;
    }

    // 🎯 ESKİ LOKAL METOD: UIManager ve NPC etkileşimleri patlamasın diye doldurma köprüsü canım
    public void ExportToSaveData(SaveData data)
    {
        data.currentHealth = this.currentHealth;
        data.maxHealth = this.maxHealth;
        data.currentMana = this.currentMana;
        data.maxMana = this.maxMana;
        data.totalCoins = this.totalCoins;
        data.currentDiamonds = this.currentDiamonds;
        data.exchangeRate = this.exchangeRate;
        data.currentLevel = this.currentLevel;
        data.currentExp = this.currentExp;
        data.maxExp = this.maxExp;
        data.isDayTime = this.isDayTime;
        data.usingSkillsIDs = this.usingSkillsIDs;
        data.openedSkillSlots = this.openedSkillSlots;
        data.unlockedSkillsIDs = this.unlockedSkillsIDs;
        data.quickBarPotionIDs = this.quickBarPotionIDs;
        data.isArmed = this.isArmed;
        data.isHelmetEquipped = this.isHelmetEquipped;
        data.isGauntletEquipped = this.isGauntletEquipped;
        data.isBootEquipped = this.isBootEquipped;
        data.isPadEquipped = this.isPadEquipped;
        data.isPauldronEquipped = this.isPauldronEquipped;
        data.currentLanguage = this.currentLanguage;
        data.masterVolume = this.masterVolume;
        data.musicVolume = this.musicVolume;
        data.sfxVolume = this.sfxVolume;
        data.joyStickPosition = this.joyStickPosition;
        data.joyStickScale = this.joyStickScale;
        data.joystickOpacity = this.joystickOpacity;
        data.isJoystickPositionLocked = this.isJoystickPositionLocked;
        data.isJoystickBackgroundVisible = this.isJoystickBackgroundVisible;
        data.skillHUDScale = this.skillHUDScale;
        cloudSaveDataCopy(data);
    }

    private void cloudSaveDataCopy(SaveData data) {
        data.skillHUDOpacity = this.skillHUDOpacity;
        data.isSkillHUDLocked = this.isSkillHUDLocked;
        data.savedSkillPositions = new List<SkillSlotPosition>(this.savedSkillPositions);
    }

    public void ImportFromSaveData(SaveData data)
    {
        this.currentHealth = data.currentHealth;
        this.maxHealth = data.maxHealth;
        this.currentMana = data.currentMana;
        this.maxMana = data.maxMana;
        this.totalCoins = data.totalCoins;
        this.currentLevel = data.currentLevel;
        this.currentDiamonds = data.currentDiamonds;
        this.exchangeRate = data.exchangeRate;
        this.currentExp = data.currentExp;
        this.maxExp = data.maxExp;
        this.isDayTime = data.isDayTime;
        this.masterVolume = data.masterVolume;
        this.musicVolume = data.musicVolume;
        this.sfxVolume = data.sfxVolume;
        this.joyStickPosition = data.joyStickPosition;
        this.joyStickScale = data.joyStickScale;
        this.joystickOpacity = data.joystickOpacity;
        this.isJoystickPositionLocked = data.isJoystickPositionLocked;
        this.isJoystickBackgroundVisible = data.isJoystickBackgroundVisible;

        this.isArmed = data.isArmed;
        this.isHelmetEquipped = data.isHelmetEquipped;
        this.isGauntletEquipped = data.isGauntletEquipped;
        this.isBootEquipped = data.isBootEquipped;
        this.isPadEquipped = data.isPadEquipped;
        this.isPauldronEquipped = data.isPauldronEquipped;
        this.usingSkillsIDs = data.usingSkillsIDs;
        this.openedSkillSlots = data.openedSkillSlots;
        this.unlockedSkillsIDs = data.unlockedSkillsIDs;

        this.skillHUDScale = data.skillHUDScale > 0.1f ? data.skillHUDScale : 1.0f;
        this.skillHUDOpacity = data.skillHUDOpacity > 0.05f ? data.skillHUDOpacity : 1.0f;
        this.isSkillHUDLocked = data.isSkillHUDLocked;

        this.savedSkillPositions = data.savedSkillPositions != null ? new List<SkillSlotPosition>(data.savedSkillPositions) : new List<SkillSlotPosition>();
        if (data.quickBarPotionIDs != null) this.quickBarPotionIDs = data.quickBarPotionIDs;

        OnSkillHUDUpdated?.Invoke();

        if (data.startingItemsIDs != null) { this.startingItemsID = data.startingItemsIDs; this.startingItems = ConvertIDsToItems(this.startingItemsID); }
        if (data.startingWearedItemsIDs != null) { this.startingWearedItemsID = data.startingWearedItemsIDs; this.startingWearedItems = ConvertIDsToItems(this.startingWearedItemsID); }

        if (this.startingWearedItems.Count > 0)
        {
            foreach (var wearedItem in this.startingWearedItems)
            {
                if (this.startingItems.Contains(wearedItem)) this.startingItems.Remove(wearedItem);
            }
        }

        this.currentLanguage = data.currentLanguage; 
        OnDataImported?.Invoke();
    }

    // 🎯 ASENKRON UYUMLULUK KAPISI: Await edilen SaveProgress artık hata vermeyecek canım!
    public System.Threading.Tasks.Task SaveProgress(string slotName)
    {
        if (SaveManager.instance != null) SaveManager.instance.SaveGameProgressOnline();
        return System.Threading.Tasks.Task.CompletedTask;
    }

    private string[] ConvertListToIDs(List<ItemData> items)
    {
        if (items == null) return new string[0];
        string[] ids = new string[items.Count];
        for (int i = 0; i < items.Count; i++) ids[i] = items[i] != null ? items[i].itemID : "-1";
        return ids;
    }

    private List<ItemData> ConvertIDsToItems(string[] ids)
    {
        List<ItemData> items = new List<ItemData>();
        if (ids == null || InventoryManager.Instance == null) return items;
        foreach (string id in ids)
        {
            if (id == "-1" || string.IsNullOrEmpty(id)) continue;
            ItemData found = InventoryManager.Instance.GetItemByID(id);
            if (found != null) items.Add(found);
        }
        return items;
    }

    public void UpdateJoystickSettings() { OnJoystickSettingsUpdated?.Invoke(); }

    public void ResetStatsForNewGame()
    {
        usingSkillsIDs = new string[8]; unlockedSkillsIDs = new string[0]; quickBarPotionIDs = new string[4]; 
        startingItems.Clear(); startingWearedItems.Clear(); savedSkillPositions.Clear();
    }
}