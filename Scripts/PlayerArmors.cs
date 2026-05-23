using UnityEngine;

public class PlayerArmors : MonoBehaviour
{
    public static PlayerArmors Instance;

    [Header("Darion Ekipmanları")]
    public ItemData equippedWeapon;
    public ItemData equippedHelmet;
    public ItemData equippedNeck;

    public ItemData equippedBoot; // Eğer 'Feet' türü varsa, burada tutabiliriz

    public ItemData equippedGauntlet; // Eğer 'Gauntlet' türü varsa, burada tutabiliriz

    public ItemData equippedPad;

    public ItemData equippedPauldron;

    void Awake() => Instance = this; //

    public void UpdateArmorData(ItemType type, ItemData item)
    {
        switch (type)
        {
            case ItemType.Weapon:
                equippedWeapon = item;
                break;
            case ItemType.Neck:
                equippedNeck = item;
                break;
            case ItemType.Head:
                equippedHelmet = item;
                if (item != null && StatsManager.Instance != null)
                    StatsManager.Instance.isHelmetEquipped = true;
                break;
            case ItemType.Feet:
                equippedBoot = item;
                if (item != null && StatsManager.Instance != null)
                    StatsManager.Instance.isBootEquipped = true;
                break;
            // EĞER kask verisi geldi ve boş değilse, DarionController'ı zorla true yapabiliriz
            case ItemType.Gauntlet:
                equippedGauntlet = item;
                if (item != null && StatsManager.Instance != null)
                    StatsManager.Instance.isGauntletEquipped = true;
                break;
            case ItemType.Legs:
                equippedPad = item;
                if (item != null && StatsManager.Instance != null)
                    StatsManager.Instance.isPadEquipped = true;
                break;
            case ItemType.Torso:
                equippedPauldron = item;
                if (item != null && StatsManager.Instance != null)
                    StatsManager.Instance.isPauldronEquipped = true;
                break;

            default:
                break;
                // Diğer türleri buraya ekleyebilirsin
        }

        // Debug için konsolda ne giydiğini görebilirsin
        Debug.Log(type + " güncellendi: " + (item != null ? item.itemName : "Boş"));
    }

    public int GetTotalEquipmentAttack()
    {
        int total = 0;

        // Her parçayı tek tek kontrol et, boş değilse gücünü ekle
        if (equippedWeapon != null) total += equippedWeapon.attackPower;
        if (equippedHelmet != null) total += equippedHelmet.attackPower;
        if (equippedNeck != null) total += equippedNeck.attackPower;
        if (equippedBoot != null) total += equippedBoot.attackPower;
        if (equippedGauntlet != null) total += equippedGauntlet.attackPower;
        if (equippedPad != null) total += equippedPad.attackPower;
        if (equippedPauldron != null) total += equippedPauldron.attackPower;

        return total;
    }

    public int GetTotalFireAttack()
    {
        int total = 0;

        // Her parçayı tek tek kontrol et, boş değilse gücünü ekle
        if (equippedWeapon != null) total += equippedWeapon.fireAttack;
        if (equippedHelmet != null) total += equippedHelmet.fireAttack;
        if (equippedNeck != null) total += equippedNeck.fireAttack;
        if (equippedBoot != null) total += equippedBoot.fireAttack;
        if (equippedGauntlet != null) total += equippedGauntlet.fireAttack;
        if (equippedPad != null) total += equippedPad.fireAttack;
        if (equippedPauldron != null) total += equippedPauldron.fireAttack;

        return total;
    }

    public int GetTotalIceAttack()
    {
        int total = 0;

        // Her parçayı tek tek kontrol et, boş değilse gücünü ekle
        if (equippedWeapon != null) total += equippedWeapon.iceAttack;
        if (equippedHelmet != null) total += equippedHelmet.iceAttack;
        if (equippedNeck != null) total += equippedNeck.iceAttack;
        if (equippedBoot != null) total += equippedBoot.iceAttack;
        if (equippedGauntlet != null) total += equippedGauntlet.iceAttack;
        if (equippedPad != null) total += equippedPad.iceAttack;
        if (equippedPauldron != null) total += equippedPauldron.iceAttack;

        return total;
    }

    public int GetTotalEquipmentDefense()
    {
        int total = 0;

        // Her parçayı tek tek kontrol et, boş değilse gücünü ekle
        if (equippedWeapon != null) total += equippedWeapon.defensePower;
        if (equippedHelmet != null) total += equippedHelmet.defensePower;
        if (equippedNeck != null) total += equippedNeck.defensePower;
        if (equippedBoot != null) total += equippedBoot.defensePower;
        if (equippedGauntlet != null) total += equippedGauntlet.defensePower;
        if (equippedPad != null) total += equippedPad.defensePower;
        if (equippedPauldron != null) total += equippedPauldron.defensePower;

        return total;
    }

    public int GetTotalFireDefense()
    {
        int total = 0;

        // Her parçayı tek tek kontrol et, boş değilse gücünü ekle
        if (equippedWeapon != null) total += equippedWeapon.fireDefencePower;
        if (equippedHelmet != null) total += equippedHelmet.fireDefencePower;
        if (equippedNeck != null) total += equippedNeck.fireDefencePower;
        if (equippedBoot != null) total += equippedBoot.fireDefencePower;
        if (equippedGauntlet != null) total += equippedGauntlet.fireDefencePower;
        if (equippedPad != null) total += equippedPad.fireDefencePower;
        if (equippedPauldron != null) total += equippedPauldron.fireDefencePower;

        return total;
    }

    public int GetTotalIceDefense()
    {
        int total = 0;

        // Her parçayı tek tek kontrol et, boş değilse gücünü ekle
        if (equippedWeapon != null) total += equippedWeapon.iceDefencePower;
        if (equippedHelmet != null) total += equippedHelmet.iceDefencePower;
        if (equippedNeck != null) total += equippedNeck.iceDefencePower;
        if (equippedBoot != null) total += equippedBoot.iceDefencePower;
        if (equippedGauntlet != null) total += equippedGauntlet.iceDefencePower;
        if (equippedPad != null) total += equippedPad.iceDefencePower;
        if (equippedPauldron != null) total += equippedPauldron.iceDefencePower;

        return total;
    }

    public int GetTotalEquipmentManaCost()
    {
        int total = 0;

        // Her parçayı tek tek kontrol et, boş değilse mana maliyetini ekle
        if (equippedWeapon != null) total += equippedWeapon.manaCost;
        if (equippedHelmet != null) total += equippedHelmet.manaCost;
        if (equippedNeck != null) total += equippedNeck.manaCost;
        if (equippedBoot != null) total += equippedBoot.manaCost;
        if (equippedGauntlet != null) total += equippedGauntlet.manaCost;
        if (equippedPad != null) total += equippedPad.manaCost;
        if (equippedPauldron != null) total += equippedPauldron.manaCost;

        return total;
    }

    public int GetTotalEquipmentHealtBonus()
    {
        int total = 0;

        if (equippedWeapon != null) total += equippedWeapon.healthBonus;
        if (equippedHelmet != null) total += equippedHelmet.healthBonus;
        if (equippedNeck != null) total += equippedNeck.healthBonus;
        if (equippedBoot != null) total += equippedBoot.healthBonus;
        if (equippedGauntlet != null) total += equippedGauntlet.healthBonus;
        if (equippedPad != null) total += equippedPad.healthBonus;
        if (equippedPauldron != null) total += equippedPauldron.healthBonus;

        return total;
    }

    public int GetTotalEquipmentManaBonus()
    {
        int total = 0;

        if (equippedWeapon != null) total += equippedWeapon.manaBonus;
        if (equippedHelmet != null) total += equippedHelmet.manaBonus;
        if (equippedNeck != null) total += equippedNeck.manaBonus;
        if (equippedBoot != null) total += equippedBoot.manaBonus;
        if (equippedGauntlet != null) total += equippedGauntlet.manaBonus;
        if (equippedPad != null) total += equippedPad.manaBonus;
        if (equippedPauldron != null) total += equippedPauldron.manaBonus;

        return total;
    }

    public int GetTotalWeaponManaCost()
    {
        int total = 0;

        if (equippedWeapon != null) total += equippedWeapon.manaBonus;

        return total;
    }


}