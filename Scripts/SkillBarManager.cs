using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;

public class SkillBarManager : MonoBehaviour
{
    // UIManager.cs (veya StatsManager.cs) içine asdas
    [Header("Skill Database")]
    public List<SkillData> allSkillsDatabase = new List<SkillData>();
    public static SkillBarManager Instance;

    [Header("Skill Bar")]
    public GameObject skillBarPanel;

    [Header("Skill Slotları")]
    public List<SkillSlotUIController> skillSlots;

    [Header("Send Skill Data To Animation")]
    public SkillData currentUsedSkill; // Şu anda kullanılan yeteneğin bilgilerini tutar, animasyon köprüsüne gönderilir asdas

    public GameObject player;

    public Animator anim;

    [Header("Cooldown Tracking")]
    // Aktif olarak cooldown'da olan skilleri burada göreceksin
    public List<SkillData> currentUsingSkills = new List<SkillData>();

    public SkillSlotUIController lastUsedSlot; // En son hangi slota tıklandığını hatırlar


    // SkillBarManager.cs içine ekle asdas
    // SkillBarManager.cs içine bu blokları ekle asdas
    private void OnEnable()
    {
        // StatsManager'dan gelen "Bir şeyler değişti!" sinyalini dinlemeye başlıyoruz cam gibi!
        StatsManager.OnSkillHUDUpdated += RefreshAllSkillSlots;
        // 🎯 KRİTİK EKSİK: Obje aktif olduğu an, kimseyi beklemeden kendini bir kez tazelesin!
        RefreshAllSkillSlots();
    }

    private void OnDisable()
    {
        // Obje deaktif olursa veya sahne değişirse dinlemeyi bırakıyoruz ki hata vermesin asdas
        StatsManager.OnSkillHUDUpdated -= RefreshAllSkillSlots;
    }
    // SkillBarManager.cs içine asdas
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Database yükleme cam gibi!
        if (allSkillsDatabase.Count == 0)
            allSkillsDatabase.AddRange(Resources.LoadAll<SkillData>("Skills"));

        InitializePlayerReferences();
    }
    private void Start()
    {
        // EĞER Inspector'dan sürüklemediysen, kodla bulalım cam gibi!
        // Eğer Awake'de bir şeyler ters gittiyse Start'ta tekrar mühürlüyoruz
        if (player == null || anim == null) InitializePlayerReferences();
        RefreshAllSkillSlots();

    }

    // Artık her yerden UIManager.Instance.FindSkillByID(id) diyerek çağırabilirsin cam gibi!
    public SkillData FindSkillByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        // Harf duyarlılığını devre dışı bırakıyoruz (ToLower) cam gibi!
        return allSkillsDatabase.Find(skill =>
            skill.skillID.ToLower().Trim() == id.ToLower().Trim());
    }

    private void InitializePlayerReferences()
    {
        // 🎯 KRİTİK DÜZELTME: Başına 'GameObject' koymuyoruz ki global değişkeni doldursun asdas
        player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            anim = player.GetComponentInChildren<Animator>();
            if (anim == null) Debug.LogError("<color=red>Aritheon:</color> Player child'larında Animator YOK!");
        }
        else
        {
            // Debug.LogError("<color=red>Aritheon:</color> Sahnede 'Player' taglı obje bulunamadı! Tag'i kontrol et canım.");
        }
    }

    void Update()
    {
        // Eğer player null ise (sahne yeni yüklendiğinde veya player henüz doğmadıysa) ara
        if (player == null)
        {
            InitializePlayerReferences();

            // Bulduğumuz an log atalım ki çalıştığını görelim asdas
            if (player != null)
                Debug.Log("<color=green>Aritheon:</color> Player bulundu ve mühürlendi!");
        }
    }

    public bool UseSkill(SkillData skillData, SkillSlotUIController callingSlot)
    {
        // 1. TEMEL KONTROLLER (Referans güvenliği)
        if (skillData == null || anim == null || StatsManager.Instance == null)
        {
            Debug.LogWarning("SkillData, Animator veya StatsManager eksik! asdas");
            return false;
        }

        // 2. SİLAH/MELEE KONTROLÜ
        bool isMeleeAttack = skillData.animationTriggerName == "isMelee";
        if (!anim.GetBool("isArmed") && !isMeleeAttack)
        {
            Debug.Log("<color=yellow>Darion:</color> Silahsızken bu yeteneği kullanamazsın!");
            return false;
        }

        // 3. MANA KONTROLÜ (Sadece kontrol ediyoruz, henüz düşmüyoruz! cam gibi!)
        if (StatsManager.Instance.currentMana < skillData.manaCost)
        {
            Debug.Log($"<color=orange>Aritheon:</color> Mana yetersiz! Gereken: {skillData.manaCost}");
            return false;
        }

        // 4. BAŞARI: YETENEK BAŞLATILIYOR asdas
        // Vuruş anında kullanılacak verileri mühürlüyoruz
        currentUsedSkill = skillData;
        lastUsedSlot = callingSlot;

        // Animasyonu tetikle
        if (!string.IsNullOrEmpty(skillData.animationTriggerName))
        {
            // Önceki trigger kalıntılarını temizle (Double-trigger bugını önler cam gibi!)
            anim.ResetTrigger(skillData.animationTriggerName);
            anim.SetTrigger(skillData.animationTriggerName);

            Debug.Log($"<color=cyan>Aritheon:</color> {skillData.skillName} animasyonu başladı, vuruş bekleniyor...");
        }

        return true; // Yetenek 'niyeti' onaylandı
    }

    // 🎯 YENİ: Animasyon kesilirse veya zaman aşımına uğrarsa sistemi sıfırlar
    public void CancelPendingSkill()
    {
        if (lastUsedSlot != null)
        {
            lastUsedSlot.InterruptSkill(); // Slotu tekrar etkileşime açar
            lastUsedSlot = null;
            currentUsedSkill = null;
            Debug.Log("<color=red>SkillBarManager:</color> Animasyon kesildi veya hedefe ulaşamadı, buton kurtarıldı!");
        }
    }

    // SkillBarManager.cs içine asdas
    public void AttackHit()
    {
        // 🛡️ 1. GÜVENLİK: Referanslar boşsa hemen çık, hata verme! asdas
        if (currentUsedSkill == null || lastUsedSlot == null)
        {
            Debug.LogWarning("<color=yellow>Aritheon:</color> AttackHit tetiklendi ama veri yok (Yetenek kesilmiş olabilir).");
            return;
        }

        // 🛡️ 2. SİSTEMSEL GÜVENLİK: StatsManager var mı?
        if (StatsManager.Instance == null)
        {
            Debug.LogError("<color=red>Hata:</color> Sahnede StatsManager bulunamadı!");
            return;
        }

        // 🎯 3. İŞLEMLERİ YAP cam gibi!
        // Manayı düşür
        CombatFormulaManager.Instance.UseSkillMana(currentUsedSkill.manaCost);

        // Slotun cooldown'ını başlat
        lastUsedSlot.StartActualCooldown();

        // 🎯 KRİTİK NOKTA: Logu veriyi SİLMEDEN ÖNCE yazdırıyoruz! asdas
        Debug.Log($"<color=green>SkillBarManager:</color> {currentUsedSkill.skillName} için işlemler tamamlandı.");

        // 🎯 4. TEMİZLİK (Şimdi silebiliriz, çünkü artık ihtiyacımız kalmadı)
        lastUsedSlot = null;
        currentUsedSkill = null;

        Debug.Log("<color=green>SkillBarManager:</color> Sistem sıfırlandı, yeni komut bekleniyor.");
    }
    private void TriggerAnimation(string triggerName, SkillData skillData = null)
    {
        // 🎯 Burası artık 'player' Awake'de dolduğu için null gelmeyecek! asdas
        if (player != null && anim != null)
        {
            currentUsedSkill = skillData;
            anim.SetTrigger(triggerName);
            Debug.Log($"<color=cyan>Aritheon HUD:</color> {skillData.skillName} animasyonu tetiklendi!");
        }
    }
    // Metodları bu merkezi yapıya bağlayalım asdas
    public void TriggerSwordAnimation() => TriggerAnimation("isAttack");
    public void TriggerMeleeAnimation() => TriggerAnimation("isMelee");
    public void RefreshAllSkillSlots()
    {
        if (StatsManager.Instance == null) return;

        float openedValue = StatsManager.Instance.openedSkillSlots;
        if (openedValue <= 0)
        {
            Invoke(nameof(RefreshAllSkillSlots), 0.1f);
            return;
        }

        int openCount = Mathf.FloorToInt(openedValue);

        // StatsManager içindeki mevcut kullanılan skilleri alıyoruz
        // Örn: public string[] usingSkillsIDs;
        string[] equippedSkills = StatsManager.Instance.usingSkillsIDs;

        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (skillSlots[i] == null) continue;

            bool isUnlocked = i < openCount;

            if (isUnlocked)
            {
                // Önce objeyi açıyoruz ki RefreshSlot içindeki mantık çalışabilsin asdas
                skillSlots[i].gameObject.SetActive(true);
                skillSlots[i].RefreshSlot();
                // RefreshSlot -> UpdateUI -> Eğer skill yoksa zaten kendi kendine tekrar SetActive(false) yapacak!
            }
            else
            {
                // Kilidi hiç açılmamışsa direkt kapatıyoruz cam gibi!
                skillSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void StartSkillCooldownTracking(SkillData skill)
    {
        if (skill == null) return;

        // Eğer aynı skill zaten listede değilse ekle (Çift eklemeyi önleriz)
        if (!currentUsingSkills.Contains(skill))
        {
            currentUsingSkills.Add(skill);
            // Zamanlayıcıyı başlatıyoruz cam gibi!
            StartCoroutine(RemoveSkillAfterCooldown(skill));

            Debug.Log($"<color=orange>Aritheon Tracking:</color> {skill.skillName} listeye eklendi. Kalan aktif skill sayısı: {currentUsingSkills.Count}");
        }
    }

    private IEnumerator RemoveSkillAfterCooldown(SkillData skill)
    {
        // SkillData içindeki cooldown süresi kadar bekle asdas
        yield return new WaitForSeconds(skill.cooldown);

        if (currentUsingSkills.Contains(skill))
        {
            currentUsingSkills.Remove(skill);
            Debug.Log($"<color=green>Aritheon Tracking:</color> {skill.skillName} cooldown bitti ve listeden çıkarıldı!");
        }
    }


}