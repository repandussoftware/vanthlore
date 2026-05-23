using System;
using UnityEngine;

public class PlayerAnimationBridge : MonoBehaviour
{
    [Header("Saldırı Ayarları")]
    [SerializeField] private Transform attackPoint; // Darion'un önünde, kılıcın ulaştığı yere koyacağın boş bir obje
    [SerializeField] private float attackRange = 1.2f; // Vuruşun algılanacağı mesafe
    [SerializeField] private LayerMask dummyLayer; // Sadece Dummy'leri algılamak için (Performans için önemli)

    public void Step()
    {
        if (UIManager.Instance != null)
        {
           // UIManager.Instance.PlayFootstepSound();
        }
    }

    // Animasyon penceresinde (Animation Window) kılıcın dummy'ye değdiği kareye bu eventi ekle
    public void AttackHit()
    {
        // 🛡️ 1. GÜVENLİK: Temel Manager'lar sahnede var mı? asdas
        if (SkillBarManager.Instance == null || StatsManager.Instance == null)
        {
            Debug.LogError("<color=red>Hata:</color> Sahnede SkillBarManager veya StatsManager eksik!");
            return;
        }

        // 🛡️ 2. VERİ YALITIMI (En Kritik Dokunuş!) cam gibi!
        // Manager'daki veriyi yerel bir değişkene 'kopyalıyoruz'. 
        // Böylece Manager birazdan bu veriyi silse bile biz işimize bakabileceğiz.
        SkillData skillData = SkillBarManager.Instance.currentUsedSkill;

        // Eğer bir rün tetiklenmemişse (Normal vuruşsa veya veri kaybolmuşsa) çık asdas
        if (skillData == null)
        {
            Debug.LogWarning("<color=yellow>Aritheon:</color> AttackHit tetiklendi ama aktif rün verisi yok. (Mekanik durduruldu)");
            return;
        }

        // 🎯 3. SİSTEMSEL SENKRONİZASYON asdas
        // Manager'daki metodu şimdi çağırıyoruz. 
        // Mana düşecek ve cooldown başlayacak. Manager kendi içindeki referansı silebilir, sorun değil!
        SkillBarManager.Instance.AttackHit();

        // 🎯 4. COMBAT VE GÖRSEL MANTIĞI cam gibi!
        Vector3 spawnPos = transform.position;
        bool isRight = StatsManager.Instance.isFacingRight;
        string vfxFunc = skillData.vfxFunctionName;

        // Hasar paketini hesapla
        float[] finalDamage = CombatFormulaManager.Instance.CalculateDamage(skillData);

        Debug.Log($"<color=cyan>Aritheon Combat:</color> {skillData.skillName} icra ediliyor. Hasar: {finalDamage[0]} Phys / {finalDamage[1]} Elem");

        // 5. VFX VE MERMİ SİSTEMİ
        if (!string.IsNullOrEmpty(vfxFunc))
        {
            string cleanVFXName = skillData.skillID.Replace("_", "");

            if (vfxFunc == "PlayVFXAttached")
            {
                VFXManager.Instance.PlayVFXAttached(cleanVFXName, this.transform, finalDamage);
            }
            else if (vfxFunc == "PlayVFXWorld")
            {
                VFXManager.Instance.PlayVFXWorld(cleanVFXName, spawnPos, isRight, finalDamage);

                // Özel yetenek kontrolü (Meteor Yağmuru vb.) asdas
                if (BurningSkyManager.Instance != null)
                {
                    BurningSkyManager.Instance.StartMeteorRain(finalDamage);
                }
            }
        }

        // 6. YAKIN DÖVÜŞ (MELEE) KONTROLÜ
        // Eğer mermi fırlatılmıyorsa alanı tara!
        if (vfxFunc != "PlayVFXWorld")
        {
            ApplyMeleeDamage(finalDamage, isRight);
        }
    }

    // 🎯 YARDIMCI METOD: Yakın dövüş hasarını veren kısım cam gibi! asdas
    private void ApplyMeleeDamage(float[] damage, bool isRight)
    {
        Vector3 hitPoint = (attackPoint != null) ? attackPoint.position :
                           transform.position + (isRight ? Vector3.right : Vector3.left);

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(hitPoint, attackRange);

        foreach (Collider2D obj in hitObjects)
        {
            if (obj.CompareTag("Enemy"))
            {
                EnemyStats enemy = obj.GetComponent<EnemyStats>();
                if (enemy != null)
                {
                    enemy.TakeHit(damage[0], damage[1], damage[2]);
                    Debug.Log($"<color=cyan>Yakın Dövüş:</color> {obj.name} darbe aldı!");
                }
            }
        }
    }
    public void JumpedTriggered()
    {
        // Konsolda bu kırmızıyı görmüyorsan event tetiklenmiyordur canım.
        Debug.Log("<color=red><b>EVENT ÇALIŞTI!</b></color>");

        if (DarionController.Instance != null)
        {
            DarionController.Instance.ApplyPhysicalJump();
        }
    }

    public void MeleeHit()
    {

    }

    // Editörde vuruş alanını görebilmek için (Gizmos)
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}