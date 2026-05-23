using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string itemName;

    public string itemNameTR;
    public string itemID;
    public ItemType itemType;
    public ItemRarity rarity;

    [Header("Görsel ve Ses")]
    public Sprite icon;                // Envanter ikonu

    [Header("Silah Görselleri (Weapon)")]
    [Tooltip("PSD: Sword")]
    public Sprite weaponSprite;
    [Tooltip("PSD: Holder/Hilt")]
    public Sprite hiltSprite;

    [Header("Kask Görselleri (Helmet Variations)")]
    [Tooltip("Standart tam kask (PSD: Kask)")]
    public Sprite helmetStandard;      //

    [Tooltip("Kırpılmış varyasyonlar listesi (PSD: Kask_cenesiz_1...6)")]
    public List<Sprite> helmetVariations; // 1'den 6'ya kadar olanları buraya dizebilirsin

    [Header("Boot Görselleri (Boot Variations)")]
    [Tooltip("Standart tam kask (PSD: Kask)")]

    public Sprite bootBodyFront;
    public Sprite bootUnderFront;
    public Sprite bootUnderBack;
    public Sprite bootBodyBack;

    [Header("Gauntlet Görselleri (Gauntlet Variations)")]
    [Tooltip("Standart tam kolluk (PSD: Kolluk)")]

    public Sprite sleevePart1;
    public Sprite sleevePart2;
    public Sprite sleevePart3;
    public Sprite sleevePart4;
    public Sprite sleevePart5;
    public Sprite sleevePart6;
    public Sprite sleevePart7;
    public Sprite sleevePart8;
    public Sprite sleevePart9;
    public Sprite sleevePart10;
    public Sprite sleevePart11;
    public Sprite sleevePart12;
    public Sprite sleevePart13;
    public Sprite sleevePart14;
    public Sprite sleevePart15;

    [Header("Pant Görselleri (Pant Variations)")]
    [Tooltip("Standart tam pant (PSD: Pantolon)")]

    public Sprite pantPart1;
    public Sprite pantPart2;
    public Sprite pantPart3;
    public Sprite pantPart4;
    public Sprite pantPart5;
    public Sprite pantPart6;
    public Sprite pantPart7;
    public Sprite pantPart8;
    public Sprite pantPart9;
    public Sprite pantPart10;
    public Sprite pantPart11;
    public Sprite pantPart12;
    public Sprite pantPart13;
    public Sprite pantPart14;
    public Sprite pantPart15;
    public Sprite pantPart16;
    public Sprite pantPart17;
    public Sprite pantPart18;
    public Sprite pantPart19;
    public Sprite pantPart20;
    public Sprite pantPart21;

    [Header("Pauldron Görselleri (Pauldron Variations)")]
    [Tooltip("Standart tam Pauldron (PSD: Pantolon)")]

    public Sprite pauldronIdleArmless;
    public Sprite pauldronOrig;
    public Sprite pauldronWalk31;
    public Sprite pauldronWalk30;
    public Sprite pauldronWalk24_25;
    public Sprite pauldronWalk21_22_23_26_27;
    public Sprite pauldronWalk20_28_29;
    public Sprite pauldronWalk19;
    public Sprite pauldronWalk18;
    public Sprite pauldronWalk13;
    public Sprite pauldronWalk12;
    public Sprite pauldronWalk11;
    public Sprite pauldronWalk10;
    public Sprite pauldronWalk9;
    public Sprite pauldronWalk8;
    public Sprite pauldronWalk7;
    public Sprite pauldronWalk6;
    public Sprite pauldronWalk5;
    public Sprite pauldronWalk4;
    public Sprite pauldronWalk3_14;
    public Sprite pauldronWalk2_15_16;
    public Sprite pauldronWalk1_17;
    public Sprite pauldronRun16_17_18_19_20_21_22;
    public Sprite pauldronRun15_23;
    public Sprite pauldronRun14;
    public Sprite pauldronRun13;
    public Sprite pauldronRun11_12;
    public Sprite pauldronRun10;
    public Sprite pauldronRun9;
    public Sprite pauldronRun8;
    public Sprite pauldronRun7;
    public Sprite pauldronRun6;
    public Sprite pauldronRun5;
    public Sprite pauldronRun4;
    public Sprite pauldronRun3;
    public Sprite pauldronRun2;
    public Sprite pauldronRun1;

    public Sprite pauldronSwordAttack1;
    public Sprite pauldronSwordAttack2;
    public Sprite pauldronSwordAttack3;
    public Sprite pauldronSwordAttack4;
    public Sprite pauldronSwordAttack5;
    public Sprite pauldronSwordAttack6;
    public Sprite pauldronSwordAttack7;
    public Sprite pauldronSwordAttack8_9_10_11_12_13;
    public Sprite pauldronSwordAttack14_15_16;
    public Sprite pauldronSwordAttack17_18;
    public Sprite pauldronSwordAttackBase;
    public Sprite pauldronMelee1_2_3_4_5_6_7_8;
    public Sprite pauldronMelee9;
    public Sprite pauldronMelee10;
    public Sprite pauldronMelee11;
    public Sprite pauldronMelee12;
    public Sprite pauldronMelee13;
    public Sprite pauldronMelee14_15;
    public Sprite pauldronMelee16_17_18_19;
    public Sprite pauldronMelee20;
    public Sprite pauldronDie1_2;
    public Sprite pauldronDie3;
    public Sprite pauldronDie4_5_6;
    public Sprite pauldronDie7_8;
    public Sprite pauldronDie9_10;
    public Sprite pauldronDie11_12;
    public Sprite pauldronDie13_14_15_16_17_18_19_20_21;
    public Sprite pauldronDie22_23_24;
    public Sprite pauldronDie25_26_27;
    public Sprite pauldronDie28_29_30_31;
    public Sprite pauldronDie32_33_34_35;
    public Sprite pauldronDie36_37_38;
    public Sprite pauldronDie39_40;
    public Sprite pauldronDie41_42_43_44_45_46_47;

    public Sprite pauldronSende1_2_3_4_5_6;
    public Sprite pauldronSende7_8_9;
    public Sprite pauldronSende10_11_12_13;
    public Sprite pauldronSende14_15_16_17;
    public Sprite pauldronSende18_19_20_21;
    public Sprite pauldronSende22_23_24;
    public Sprite pauldronSende25_26_27_28;

    public Sprite pauldronJump1_2_3_4;
    public Sprite pauldronJump5;
    public Sprite pauldronJump6;
    public Sprite pauldronJump7;
    public Sprite pauldronJump8_9;
    public Sprite pauldronJump10_11_12_13_14_15_16;
    public Sprite pauldronJump17_18;
    public Sprite pauldronJump19_20_21;
    public Sprite pauldronJump21_22;
    public Sprite pauldronJump23_24_25;
    public Sprite pauldronJump26_27_28_29_30_31;

    public Sprite pauldronMeleeBase;
    public Sprite pauldronDieBase;
    public Sprite pauldronJumpBase;
    public Sprite pauldronWalkBase;
    public Sprite pauldronSendeBase;
    public Sprite pauldronSendeBase2;
    public Sprite pauldronRunBase;
    public Sprite pauldronBack;
    public Sprite pauldronPart1;
    public Sprite pauldronPart2;
    public Sprite pauldronPart3;
    public Sprite pauldronKumasLittle;

    public GameObject worldPrefab;
    public AudioClip useSound;

    [Header("İstatistikler")]
    public int attackPower;
    public int defensePower;
    public int fireDefencePower;
    public int iceDefencePower;

    public int fireAttack;
    public int iceAttack;
    public int manaCost;
    public float weight;
    public int buyPrice;
    public int sellPrice;

    public int healthBonus;
    public int manaBonus;

    public bool isTemporary = false; 

    public int duration = 0;

    [Header("Açıklama")]
    [TextArea(3, 10)]
    public string description;

    [TextArea(3, 10)]
    public string descriptionTR;

    [Header("Kullanım Ayarları")]
    public bool isStackable;
    public int maxStackSize = 999;
    public int levelRequirement;

    [Header("Güvenlik Ayarları")]
    public bool canBeDropped = true;

    [Header("Yetenek Ayarları")]
    public List<SkillData> grantedSkills;
}