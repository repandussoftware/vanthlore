using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterVisualManager : MonoBehaviour
{
    public static CharacterVisualManager Instance;

    [Header("Slot Referansları")]
    public Transform mainWeaponSlot;   // Player > Visuals > WeaponSlot
    

    [Header("Stüdyo Referansları")]
    public Transform studioWeaponSlot; // Envanterdeki Stüdyo Modeli > WeaponSlot
  

    [Header("Görsel Referanslar")]
    public RawImage darionRawImage;
    public Animator studioAnimator;

    void Awake() => Instance = this;

    public void SyncVisualsFromInventory()
{
    // InventoryManager.Instance.allEquipmentSlots üzerinden dönerken 
    // dünyadaki modele slot ataması yapmalı.
    foreach (var slot in InventoryManager.Instance.allEquipmentSlots)
    {
        UpdateDarionVisual(slot.currentItem, slot.slotType);
    }
}

    public void UpdateDarionVisual(ItemData item, ItemType type)
    {
        bool hasItem = (item != null);

        if (type == ItemType.Weapon)
        {
            UpdateWeaponParts(mainWeaponSlot, item);
            UpdateWeaponParts(studioWeaponSlot, item);

            if (StatsManager.Instance != null)
                StatsManager.Instance.isArmed = hasItem;

            if (studioAnimator != null)
                studioAnimator.SetBool("isArmed", hasItem);
        }
        if (type == ItemType.Head) // PSD'deki 'Head' türüyle eşleşir
        {

            // 2. DarionController'a kask durumunu bildir (Saçların gizlenmesi için)
            if (StatsManager.Instance != null)
                StatsManager.Instance.isHelmetEquipped = hasItem;

            // 3. Stüdyo animatorüne kask parametresini gönder
            if (studioAnimator != null)
                studioAnimator.SetBool("isHelmetEquipped", hasItem);
        }

        if (type == ItemType.Feet) // Eğer 'Feet' türü varsa
        {

            if (StatsManager.Instance != null)
                StatsManager.Instance.isBootEquipped = hasItem;

            if (studioAnimator != null)
                studioAnimator.SetBool("isBootEquipped", hasItem);

        }

        if (type == ItemType.Gauntlet) // Eğer 'Gauntlet' türü varsa
        {

             if (StatsManager.Instance != null)
                StatsManager.Instance.isGauntletEquipped = hasItem;

            if (studioAnimator != null)
                studioAnimator.SetBool("isGauntletEquipped", hasItem);
        }

        if (type == ItemType.Legs) // Eğer 'Gauntlet' türü varsa
        {

            if (StatsManager.Instance != null)
                StatsManager.Instance.isPadEquipped = hasItem;

            if (studioAnimator != null)
                studioAnimator.SetBool("isPadsEquipped", hasItem);
        }

        if (type == ItemType.Torso) // Eğer 'Pauldron' türü varsa
        {

             if (StatsManager.Instance != null)
                StatsManager.Instance.isPauldronEquipped = hasItem;

            if (studioAnimator != null)
                studioAnimator.SetBool("isPauldronEquipped", hasItem);
        }
    }

    private void UpdateWeaponParts(Transform slotTransform, ItemData item)
    {
        if (slotTransform == null) return;

        SpriteRenderer swordSR = null;
        SpriteRenderer hiltSR = null;

        SpriteRenderer[] renderers = slotTransform.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            if (sr.name == "Sword") swordSR = sr;
            if (sr.name == "Holder" || sr.name == "Hilt") hiltSR = sr;
        }

        if (item != null && item.weaponSprite != null)
        {
            slotTransform.gameObject.SetActive(true);
            if (swordSR != null) { swordSR.sprite = item.weaponSprite; swordSR.gameObject.SetActive(true); }
            if (hiltSR != null) { hiltSR.sprite = item.hiltSprite; hiltSR.gameObject.SetActive(true); }
        }
        else
        {
            slotTransform.gameObject.SetActive(false);
        }
    }

    private void UpdateHelmetParts(Transform slotTransform, ItemData item)
    {
        if (slotTransform == null) return;

        if (item != null && item.itemType == ItemType.Head)
        {
            slotTransform.gameObject.SetActive(true);
            // (true) parametresi kapalı olan çocukları da bulmasını sağlar
            SpriteRenderer[] renderers = slotTransform.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var sr in renderers)
            {
                if (sr.name == "Kask")
                {
                    sr.sprite = item.helmetStandard;
                    // SADECE 'Kask' ana objesini burada zorla açalım
                    sr.gameObject.SetActive(true);
                }
                else if (sr.name.StartsWith("Kask_cenesiz_"))
                {
                    string indexStr = sr.name.Replace("Kask_cenesiz_", "");
                    if (int.TryParse(indexStr, out int index) && item.helmetVariations != null)
                    {
                        if (index - 1 < item.helmetVariations.Count)
                        {
                            sr.sprite = item.helmetVariations[index - 1];
                            // Diğerlerini animasyonun yönetmesine izin veriyorsan SetActive(true) DEME.
                            // Ama her şey kapalı kalıyorsa test için buraya da ekleyebilirsin.
                        }
                    }
                }
            }
        }
        else
        {
            slotTransform.gameObject.SetActive(false);
        }
    }

    private void UpdateBootParts(Transform slotTransform, ItemData item)
    {
        if (slotTransform == null) return;

        if (item != null && item.itemType == ItemType.Feet)
        {
            slotTransform.gameObject.SetActive(true);
            SpriteRenderer[] renderers = slotTransform.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var sr in renderers)
            {
                // Hiyerarşideki isimlere göre tam eşleşme yapıyoruz canım
                switch (sr.name)
                {
                    case "boat_body_idle_front":
                        sr.sprite = item.bootBodyFront; // Ön gövde
                        break;
                    case "boat_under_idle_front":
                        sr.sprite = item.bootUnderFront; // Ön taban/alt
                        break;
                    case "boat_under_idle_back":
                        sr.sprite = item.bootUnderBack; // Arka taban/alt
                        break;
                    case "boat_body_idle_back":
                        sr.sprite = item.bootBodyBack; // Arka gövde
                        break;
                }

                // "Zaten hepsi enabled" dediğin için aktiflik kontrolü yapmıyoruz, 
                // sadece sprite atıyoruz.
            }
        }
        else
        {
            slotTransform.gameObject.SetActive(false);
        }
    }

    private void UpdateGauntletParts(Transform slotTransform, ItemData item)
    {
        Debug.Log("Updating gauntlet parts for item: " + (item != null ? item.itemName : "null"));
        if (slotTransform == null)
        {
            Debug.LogWarning("Gauntlet slot transform is not assigned!");
            return;
        }


        if (item != null && item.itemType == ItemType.Gauntlet)
        {
            slotTransform.gameObject.SetActive(true);
            SpriteRenderer[] renderers = slotTransform.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var sr in renderers)
            {
                // Hiyerarşideki isimlere göre tam eşleşme yapıyoruz canım
                switch (sr.name)
                {
                    case "Sleeve_Part_1":
                        sr.sprite = item.sleevePart1;
                        break;
                    case "Sleeve_Part_2":
                        sr.sprite = item.sleevePart2;
                        break;
                    case "Sleeve_Part_3":
                        sr.sprite = item.sleevePart3;
                        break;
                    case "Sleeve_Part_4":
                        sr.sprite = item.sleevePart4;
                        break;
                    case "Sleeve_Part_4 (1)":
                        sr.sprite = item.sleevePart5;
                        break;
                    case "Sleeve_Part_5":
                        sr.sprite = item.sleevePart6;
                        break;
                    case "Sleeve_Part_1_1":
                        sr.sprite = item.sleevePart7;
                        break;
                    case "Sleeve_Part_8":
                        sr.sprite = item.sleevePart8;
                        break;
                    case "Sleeve_Part_2_1":
                        sr.sprite = item.sleevePart9;
                        break;
                    case "Sleeve_Part_9":
                        sr.sprite = item.sleevePart10;
                        break;
                    case "Sleeve_Part_6":
                        sr.sprite = item.sleevePart11;
                        break;
                    case "Sleeve_Part_10":
                        sr.sprite = item.sleevePart12;
                        break;
                    case "Sleeve_Part_7":
                        sr.sprite = item.sleevePart13;
                        break;
                    case "Sleeve_Part_5_1":
                        sr.sprite = item.sleevePart14;
                        break;
                    case "Sleeve_Full":
                        sr.sprite = item.sleevePart15;
                        break;

                }
            }
        }
        else
        {
            Debug.Log("No gauntlet item or wrong item type. Hiding gauntlet visuals.");
            slotTransform.gameObject.SetActive(false);
        }
    }

    private void UpdatePadParts(Transform slotTransform, ItemData item)
    {
        if (slotTransform == null) return;

        if (item != null && item.itemType == ItemType.Legs)
        {
            slotTransform.gameObject.SetActive(true);
            SpriteRenderer[] renderers = slotTransform.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var sr in renderers)
            {
                // Hiyerarşideki isimlere göre tam eşleşme yapıyoruz canım
                switch (sr.name)
                {
                    case "pant_part_1":
                        sr.sprite = item.pantPart1;
                        break;
                    case "pant_part_1 (1)":
                        sr.sprite = item.pantPart2;
                        break;
                    case "pant_part_2":
                        sr.sprite = item.pantPart3;
                        break;
                    case "pant_part_3":
                        sr.sprite = item.pantPart4;
                        break;
                    case "pant_part_4":
                        sr.sprite = item.pantPart5;
                        break;
                    case "pant_part_5":
                        sr.sprite = item.pantPart6;
                        break;
                    case "pant_part_1 (2)":
                        sr.sprite = item.pantPart7;
                        break;
                    case "pant_part_1 (3)":
                        sr.sprite = item.pantPart8;
                        break;
                    case "pant_part_2 (1)":
                        sr.sprite = item.pantPart9;
                        break;
                    case "pant_part_3 (1)":
                        sr.sprite = item.pantPart10;
                        break;
                    case "pant_part_4 (1)":
                        sr.sprite = item.pantPart11;
                        break;
                    case "pant_part_5 (1)":
                        sr.sprite = item.pantPart12;
                        break;
                    case "pant_part_6":
                        sr.sprite = item.pantPart13;
                        break;
                    case "pant_part_6 (1)":
                        sr.sprite = item.pantPart14;
                        break;
                    case "pant_part_7":
                        sr.sprite = item.pantPart15;
                        break;
                    case "pant_part_8":
                        sr.sprite = item.pantPart16;
                        break;
                    case "pant_part_9":
                        sr.sprite = item.pantPart17;
                        break;
                    case "pant_part_10":
                        sr.sprite = item.pantPart18;
                        break;
                    case "pant_1 idle_hand":
                        sr.sprite = item.pantPart19;
                        break;
                    case "pant_trimmed":
                        sr.sprite = item.pantPart20;
                        break;
                    case "pant_orig":
                        sr.sprite = item.pantPart21;
                        break;

                }
            }
        }
        else
        {
            slotTransform.gameObject.SetActive(false);
        }
    }

    private void UpdatePauldronParts(Transform slotTransform, ItemData item)
    {
        if (slotTransform == null) return;

        if (item != null && item.itemType == ItemType.Torso) // Eğer 'Gauntlet' türü varsa
        {
            slotTransform.gameObject.SetActive(true);
            SpriteRenderer[] renderers = slotTransform.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var sr in renderers)
            {
                // Hiyerarşideki isimlere göre tam eşleşme yapıyoruz canım
                switch (sr.name)
                {
                    case "Pauldron_Idle_Armles":
                        sr.sprite = item.pauldronIdleArmless;
                        break;
                    case "Pauldron_Orig":
                        sr.sprite = item.pauldronOrig;
                        break;
                    case "Pauldron_Walk_31":
                        sr.sprite = item.pauldronWalk31;
                        break;
                    case "Pauldron_Walk_30":
                        sr.sprite = item.pauldronWalk30;
                        break;
                    case "Pauldron_Walk_24_25":
                        sr.sprite = item.pauldronWalk24_25;
                        break;
                    case "Pauldron_Walk_21_22_23_26_27":
                        sr.sprite = item.pauldronWalk21_22_23_26_27;
                        break;
                    case "Pauldron_Walk_20_28_29":
                        sr.sprite = item.pauldronWalk20_28_29;
                        break;
                    case "Pauldron_Walk_19":
                        sr.sprite = item.pauldronWalk19;
                        break;
                    case "Pauldron_Walk_18":
                        sr.sprite = item.pauldronWalk18;
                        break;
                    case "Pauldron_Walk_13":
                        sr.sprite = item.pauldronWalk13;
                        break;
                    case "Pauldron_Walk_12":
                        sr.sprite = item.pauldronWalk12;
                        break;
                    case "Pauldron_Walk_11":
                        sr.sprite = item.pauldronWalk11;
                        break;
                    case "Pauldron_Walk_10":
                        sr.sprite = item.pauldronWalk10;
                        break;
                    case "Pauldron_Walk_9":
                        sr.sprite = item.pauldronWalk9;
                        break;
                    case "Pauldron_Walk_8":
                        sr.sprite = item.pauldronWalk8;
                        break;
                    case "Pauldron_Walk_7":
                        sr.sprite = item.pauldronWalk7;
                        break;
                    case "Pauldron_Walk_6":
                        sr.sprite = item.pauldronWalk6;
                        break;
                    case "Pauldron_Walk_5":
                        sr.sprite = item.pauldronWalk5;
                        break;
                    case "Pauldron_Walk_4":
                        sr.sprite = item.pauldronWalk4;
                        break;
                    case "Pauldron_Walk_3_14":
                        sr.sprite = item.pauldronWalk3_14;
                        break;
                    case "Pauldron_Walk_2_15_16":
                        sr.sprite = item.pauldronWalk2_15_16;
                        break;
                    case "Pauldron_Walk_1_17":
                        sr.sprite = item.pauldronWalk1_17;
                        break;
                    case "Pauldron_Run_16_17_18_19_20_21_22":
                        sr.sprite = item.pauldronRun16_17_18_19_20_21_22;
                        break;
                    case "Pauldron_Run_15_23":
                        sr.sprite = item.pauldronRun15_23;
                        break;
                    case "Pauldron_Run_14":
                        sr.sprite = item.pauldronRun14;
                        break;
                    case "Pauldron_Run_13":
                        sr.sprite = item.pauldronRun13;
                        break;
                    case "Pauldron_Run_11_12":
                        sr.sprite = item.pauldronRun11_12;
                        break;
                    case "Pauldron_Run_10":
                        sr.sprite = item.pauldronRun10;
                        break;
                    case "Pauldron_Run_9":
                        sr.sprite = item.pauldronRun9;
                        break;
                    case "Pauldron_Run_8":
                        sr.sprite = item.pauldronRun8;
                        break;
                    case "Pauldron_Run_7":
                        sr.sprite = item.pauldronRun7;
                        break;
                    case "Pauldron_Run_6":
                        sr.sprite = item.pauldronRun6;
                        break;
                    case "Pauldron_Run_5":
                        sr.sprite = item.pauldronRun5;
                        break;
                    case "Pauldron_Run_4":
                        sr.sprite = item.pauldronRun4;
                        break;
                    case "Pauldron_Run_3":
                        sr.sprite = item.pauldronRun3;
                        break;
                    case "Pauldron_Run_2":
                        sr.sprite = item.pauldronRun2;
                        break;
                    case "Pauldron_Run_1":
                        sr.sprite = item.pauldronRun1;
                        break;
                    case "Pauldron_Sword_Attack_1":
                        sr.sprite = item.pauldronSwordAttack1;
                        break;
                    case "Pauldron_Sword_Attack_2":
                        sr.sprite = item.pauldronSwordAttack2;
                        break;
                    case "Pauldron_Sword_Attack_3":
                        sr.sprite = item.pauldronSwordAttack3;
                        break;
                    case "Pauldron_Sword_Attack_4":
                        sr.sprite = item.pauldronSwordAttack4;
                        break;
                    case "Pauldron_Sword_Attack_5":
                        sr.sprite = item.pauldronSwordAttack5;
                        break;
                    case "Pauldron_Sword_Attack_6":
                        sr.sprite = item.pauldronSwordAttack6;
                        break;
                    case "Pauldron_Sword_Attack_7":
                        sr.sprite = item.pauldronSwordAttack7;
                        break;
                    case "Pauldron_Sword_Attack_8_9_10_11_12_13":
                        sr.sprite = item.pauldronSwordAttack8_9_10_11_12_13;
                        break;
                    case "Pauldron_Sword_Attack_14_15_16":
                        sr.sprite = item.pauldronSwordAttack14_15_16;
                        break;
                    case "Pauldron_Sword_Attack_17_18":
                        sr.sprite = item.pauldronSwordAttack17_18;
                        break;
                    case "Pauldron_Sword_Attack_Base":
                        sr.sprite = item.pauldronSwordAttackBase;
                        break;
                    case "Pauldron_Melee_1_2_3_4_5_6_7_8":
                        sr.sprite = item.pauldronMelee1_2_3_4_5_6_7_8;
                        break;
                    case "Pauldron_Melee_9":
                        sr.sprite = item.pauldronMelee9;
                        break;
                    case "Pauldron_Melee_10":
                        sr.sprite = item.pauldronMelee10;
                        break;
                    case "Pauldron_Melee_11":
                        sr.sprite = item.pauldronMelee11;
                        break;
                    case "Pauldron_Melee_12":
                        sr.sprite = item.pauldronMelee12;
                        break;
                    case "Pauldron_Melee_13":
                        sr.sprite = item.pauldronMelee13;
                        break;
                    case "Pauldron_Melee_14_15":
                        sr.sprite = item.pauldronMelee14_15;
                        break;
                    case "Pauldron_Melee_16_17_18_19":
                        sr.sprite = item.pauldronMelee16_17_18_19;
                        break;
                    case "Pauldron_Melee_20":
                        sr.sprite = item.pauldronMelee20;
                        break;
                    case "Pauldron_Die_1_2":
                        sr.sprite = item.pauldronDie1_2;
                        break;
                    case "Pauldron_Die_3":
                        sr.sprite = item.pauldronDie3;
                        break;
                    case "Pauldron_Die_4_5_6":
                        sr.sprite = item.pauldronDie4_5_6;
                        break;
                    case "Pauldron_Die_7_8":
                        sr.sprite = item.pauldronDie7_8;
                        break;
                    case "Pauldron_Die_9_10":
                        sr.sprite = item.pauldronDie9_10;
                        break;
                    case "Pauldron_Die_11_12":
                        sr.sprite = item.pauldronDie11_12;
                        break;
                    case "Pauldron_Die_13_14_15_16_17_18_19_20_21":
                        sr.sprite = item.pauldronDie13_14_15_16_17_18_19_20_21;
                        break;
                    case "Pauldron_Die_22_23_24":
                        sr.sprite = item.pauldronDie22_23_24;
                        break;
                    case "Pauldron_Die_25_26_27":
                        sr.sprite = item.pauldronDie25_26_27;
                        break; 
                    case "Pauldron_Die_28_29_30_31":
                        sr.sprite = item.pauldronDie28_29_30_31;
                        break;
                    case "Pauldron_Die_32_33_34_35":
                        sr.sprite = item.pauldronDie32_33_34_35;
                        break;
                    case "Pauldron_Die_36_37_38":
                        sr.sprite = item.pauldronDie36_37_38;
                        break;
                    case "Pauldron_Die_39_40":
                        sr.sprite = item.pauldronDie39_40;
                        break;
                    case "Pauldron_Die_41_42_43_44_45_46_47":
                        sr.sprite = item.pauldronDie41_42_43_44_45_46_47;
                        break;
                    case "Pauldron_Sende_1_2_3_4_5_6":
                        sr.sprite = item.pauldronSende1_2_3_4_5_6;
                        break;
                    case "Pauldron_Sende_7_8_9":
                        sr.sprite = item.pauldronSende7_8_9;
                        break;
                    case "Pauldron_Sende_10_11_12_13":
                        sr.sprite = item.pauldronSende10_11_12_13;
                        break;
                    case "Pauldron_Sende_14_15_16_17":
                        sr.sprite = item.pauldronSende14_15_16_17;
                        break;
                    case "Pauldron_Sende_18_19_20_21":
                        sr.sprite = item.pauldronSende18_19_20_21;
                        break;
                    case "Pauldron_Sende_22_23_24":
                        sr.sprite = item.pauldronSende22_23_24;
                        break;
                    case "Pauldron_Sende_25_26_27_28":
                        sr.sprite = item.pauldronSende25_26_27_28;
                        break;
                    case "Pauldron_Jump_1_2_3_4":
                        sr.sprite = item.pauldronJump1_2_3_4;
                        break;
                    case "Pauldron_Jump_5":
                        sr.sprite = item.pauldronJump5;
                        break;
                    case "Pauldron_Jump_6":
                        sr.sprite = item.pauldronJump6;
                        break;
                    case "Pauldron_Jump_7":
                        sr.sprite = item.pauldronJump7;
                        break;
                    case "Pauldron_Jump_8_9":
                        sr.sprite = item.pauldronJump8_9;
                        break;
                    case "Pauldron_Jump_10_11_12_13_14_15_16":
                        sr.sprite = item.pauldronJump10_11_12_13_14_15_16;
                        break;
                    case "Pauldron_Jump_17_18":
                        sr.sprite = item.pauldronJump17_18;
                        break;
                    case "Pauldron_Jump_19_20_21":
                        sr.sprite = item.pauldronJump19_20_21;
                        break;
                    case "Pauldron_Jump_21_22":
                        sr.sprite = item.pauldronJump21_22;
                        break;
                    case "Pauldron_Jump_23_24_25":
                        sr.sprite = item.pauldronJump23_24_25;
                        break;
                    case "Pauldron_Jump_26_27_28_29_30_31":
                        sr.sprite = item.pauldronJump26_27_28_29_30_31;
                        break;
                    case "Pauldron_Melee_Base":
                        sr.sprite = item.pauldronMeleeBase;
                        break;
                    case "Pauldron_Die_Base":
                        sr.sprite = item.pauldronDieBase;
                        break;
                    case "Pauldron_Jump_Base":
                        sr.sprite = item.pauldronJumpBase;
                        break;
                    case "Pauldron_Walk_Base":
                        sr.sprite = item.pauldronWalkBase;
                        break;
                    case "Pauldron_Sende_Base":
                        sr.sprite = item.pauldronSendeBase;
                        break;
                    case "Pauldron_Sende_Base_2":
                        sr.sprite = item.pauldronSendeBase2;
                        break;
                    case "Pauldron_Run_Base":
                        sr.sprite = item.pauldronRunBase;
                        break;
                    case "Pauldron_Back":
                        sr.sprite = item.pauldronBack;
                        break;
                    case "Pauldron_Part_1":
                        sr.sprite = item.pauldronPart1;
                        break;
                    case "Pauldron_Part_2":
                        sr.sprite = item.pauldronPart2;
                        break;
                    case "Pauldron_Part_3":
                        sr.sprite = item.pauldronPart3;
                        break;
                    case "Kumaş_Little":
                        sr.sprite = item.pauldronKumasLittle;
                        break;
                }
            }
        }
        else
        {
            slotTransform.gameObject.SetActive(false);
        }
    }
}