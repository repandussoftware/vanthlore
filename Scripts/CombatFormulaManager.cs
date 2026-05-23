using UnityEngine;
using System.Collections;

public class CombatFormulaManager : MonoBehaviour
{
    public static CombatFormulaManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- MANA YÖNETİMİ ---

    public void UseSkillMana(float amount)
    {
        // Mana'yı düşür ama 0'ın altına inmesini engelle
        StatsManager.Instance.currentMana = Mathf.Max(StatsManager.Instance.currentMana - amount, 0);
        UpdateManaUI();
    }

    public void UseWeaponMana()
    {
        if (PlayerArmors.Instance == null) return;

        int cost = PlayerArmors.Instance.GetTotalWeaponManaCost();
        StatsManager.Instance.currentMana = Mathf.Max(StatsManager.Instance.currentMana - cost, 0);
        UpdateManaUI();
    }

private void UpdatePlayerUI()
    {
        // 🛡️ SAVUNMA: Eğer sahnede karakter veya arayüz yoksa bodoslama çık, çökmesin!
        if (DarionController.Instance == null || UIManager.Instance == null) return;

        // 🎯 KUTSAL AKTARIM: Eski 'ctrl.healthSlider' bağımlılıkları tamamen bitti!
        // DarionController'ın kendi içindeki zırhlı fonksiyonu tetikleyerek UIManager'ı haberdar ediyoruz.
        // Bu fonksiyon otomatik olarak hem canı hem manayı aynı frame'de pürüzsüzce boyar canım benim.
        DarionController.Instance.UpdateNetworkUI();
    }

    private void UpdateManaUI()
    {
        // 🛡️ SAVUNMA: Aynı şekilde sistem güvenliğini elden bırakmıyoruz
        if (DarionController.Instance == null || UIManager.Instance == null) return;

        // 🎯 KUTSAL AKTARIM: Mananın değiştiği anlarda da yine tek merkezden (Single Source of Truth)
        // Karakterin kendi iç verilerini UIManager'ın slider ve text yuvalarına gürül gürül akıtıyoruz!
        DarionController.Instance.UpdateNetworkUI();
    }

    // --- HASAR HESAPLAMA (DARION -> ENEMY) ---

    public float[] CalculateDamage(SkillData skillData)
    {
        // 1. Zırh ve Ekipmanlardan gelen ham değerleri alıyoruz (int -> float convert)
        int[] intValues = TotalAttackValues();
        float[] armorValues = System.Array.ConvertAll(intValues, x => (float)x);

        // 2. Level Çarpanı (Scaling)
        // Her seviye %10 güç katar
        float levelMultiplier = 1f + (StatsManager.Instance.currentLevel * 0.1f);

        // 3. Temel Hasarları Hesapla
        float finalPhysical = (armorValues[0] + skillData.damage) * levelMultiplier;
        float finalFire     = (armorValues[1] + skillData.fireDamage) * levelMultiplier;
        float finalIce      = (armorValues[2] + skillData.iceDamage) * levelMultiplier;

        // 4. Hasar Yayılımı (Random Range %90 - %110) asdas
        float spread = Random.Range(0.9f, 1.1f);

        finalPhysical *= spread;
        finalFire     *= spread;
        finalIce      *= spread;

        // 5. Değerleri yuvarlayarak tertemiz bir paket döndür canım
        return new float[] { 
            Mathf.Round(finalPhysical), 
            Mathf.Round(finalFire), 
            Mathf.Round(finalIce) 
        };
    }

    // --- HASAR ALMA (ENEMY -> DARION) ---

    public void CalculatePlayerTokenDamage(float incomingPhys, float incomingFire = 0, float incomingIce = 0, Vector3 attackerPos = default(Vector3))
    {
        if (!DarionController.Instance || DarionController.Instance.isDead) return;

        // 1. Darion'un güncel savunma paketini alıyoruz
        int[] defense = TotalDefenceValues();

        // 2. Elemental Süzgeç
        // Fiziksel hasar en az 1, elemental hasarlar 0 olabilir
        float finalDamage = Mathf.Max(incomingPhys - defense[0], 1) +
                            Mathf.Max(incomingFire - defense[1], 0) +
                            Mathf.Max(incomingIce  - defense[2], 0);

        // 3. Canı Düşür ve Sınırla
        StatsManager.Instance.currentHealth -= finalDamage;
        if (StatsManager.Instance.currentHealth < 0) StatsManager.Instance.currentHealth = 0;

        // 4. UI ve Tepki
        UpdatePlayerUI();
        DarionController.Instance.PlayHitAnimation(attackerPos);

        // 5. Ölüm Kontrolü
        if (StatsManager.Instance.currentHealth <= 0)
        {
            DarionController.Instance.Die();
        }
    }

    // --- DEĞER TOPLAMA METODLARI ---

    public int[] TotalAttackValues()
    {
        int[] totals = new int[3] { 0, 0, 0 }; // [0]Phys, [1]Fire, [2]Ice

        // 1. Ekipmanlar
        if (PlayerArmors.Instance != null)
        {
            totals[0] = PlayerArmors.Instance.GetTotalEquipmentAttack();
            totals[1] = PlayerArmors.Instance.GetTotalFireAttack();
            totals[2] = PlayerArmors.Instance.GetTotalIceAttack();
        }

        // 2. Aktif Skiller
        if (SkillBarManager.Instance != null)
        {
            foreach (SkillData skill in SkillBarManager.Instance.currentUsingSkills)
            {
                if (skill == null) continue;

                // Yeteneğin elementine göre hasarı doğru haneye ekliyoruz
                switch (skill.element)
                {
                    case SkillElement.Physical: totals[0] += (int)skill.damage; break;
                    case SkillElement.Fire:     totals[1] += (int)skill.damage; break;
                    case SkillElement.Ice:      totals[2] += (int)skill.damage; break;
                }

                // Reinforcement bonus her zaman temel güce (fiziksel) eklenir asdas
                totals[0] += (int)skill.Reinforcement_Bonus;
            }
        }
        return totals;
    }

    public int[] TotalDefenceValues()
    {
        int[] totals = new int[3] { 0, 0, 0 };

        if (PlayerArmors.Instance != null)
        {
            totals[0] = PlayerArmors.Instance.GetTotalEquipmentDefense();
            totals[1] = PlayerArmors.Instance.GetTotalFireDefense();
            totals[2] = PlayerArmors.Instance.GetTotalIceDefense();
        }

        if (SkillBarManager.Instance != null)
        {
            foreach (SkillData skill in SkillBarManager.Instance.currentUsingSkills)
            {
                if (skill == null) continue;

                totals[0] += (int)skill.defence;
                totals[1] += (int)skill.fireDefence;
                totals[2] += (int)skill.iceDefence; // Buz savunması düzeltildi canım
            }
        }
        return totals;
    }
}