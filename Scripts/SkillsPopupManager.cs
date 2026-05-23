using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillsPopupManager : MonoBehaviour
{
    public static SkillsPopupManager Instance;


    [Header("Stats Text References")]
    public TMPro.TextMeshProUGUI maxHealthText;
    public TMPro.TextMeshProUGUI maxManaText;
    public TMPro.TextMeshProUGUI allAttackText;
    public TMPro.TextMeshProUGUI allDefenseText;
    public TMPro.TextMeshProUGUI allFireAttackText;
    public TMPro.TextMeshProUGUI allIceAttackText;

    [Header("Using Skill Slots")]
    public GameObject[] usingSkillSlots;

    public GameObject[] skillSlots;

    [Header("Skill Info Popup")]
    public GameObject skillInfoPopup;
    public Image skillImage;
    public TMPro.TextMeshProUGUI skillLevel;
    public TMPro.TextMeshProUGUI skillName;
    public TMPro.TextMeshProUGUI skillType;
    public TMPro.TextMeshProUGUI skillKind;
    public TMPro.TextMeshProUGUI skillDescription;
    public Image preSkillImage1;
    public Image preSkillImage2;
    public TMPro.TextMeshProUGUI manaCostText;
    public TMPro.TextMeshProUGUI costGoldText;
    public TMPro.TextMeshProUGUI DiamondCostText;
    public TMPro.TextMeshProUGUI cooldownText;
    public TMPro.TextMeshProUGUI castTimeText;
    public TMPro.TextMeshProUGUI damageText;
    public TMPro.TextMeshProUGUI defenceText;
    public TMPro.TextMeshProUGUI rangeText;
    public TMPro.TextMeshProUGUI efectDurationText;
    public TMPro.TextMeshProUGUI knockBackText;
    public TMPro.TextMeshProUGUI reinceformentBonusText;
    public TMPro.TextMeshProUGUI scaleFactorText;

    private float lastKnownLevel; // Level değişimini yakalamak için mühürlüyoruz

    public Sprite lockerIcon; // Kilitli slotlar için kullanılacak ikon (Inspector'dan atayacağız)

    public Image popupBottomSkillIcon;
    public TMPro.TextMeshProUGUI popupBottomSkillName;
    public TMPro.TextMeshProUGUI popupBottomSkillLevel;
    public TMPro.TextMeshProUGUI popupBottomSkillDescription;
    public TMPro.TextMeshProUGUI popupBottomSkillKind;

    // Değişkenlerin olduğu yere ekle asdas
    [Header("Locked Skill Warn Prefab")]
    public LockedSkillWarnManager lockedSkillWarnPopup;

    [Header("Add Skill Slot Button")]
    public Button addSkillSlotButton;
    public Image addSkillSlotButtonImage;

    public Sprite checkMarkIcon;
    public Sprite removeIcon;

    // İstediğin an çağırıp yazıları düzenleyeceğin o metod cam gibi!
    public void ShowLockedSkillWarning(LockedSkillWarnManager lockedSkillWarnPopup)
    {
        Debug.Log("<color=orange>Aritheon:</color> Kilitli slot uyarısı tetiklendi! asdas");
        if (lockedSkillWarnPopup == null) return;

        int diamondCost = 20; // Burayı sabit veya dinamik yapabilirsin asdas
        float remainingUnlocks = CalculateRemainingSlotUnlocks();

        // Prefab üzerindeki Open metodunu çağırıp verileri gönderiyoruz
        lockedSkillWarnPopup.OpenForSlot(diamondCost, remainingUnlocks);
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void initBottomPopup(SkillData skillData)
    {
        if (skillData == null) return;
        if (popupBottomSkillIcon != null) popupBottomSkillIcon.sprite = skillData.skillIcon;
        if (popupBottomSkillName != null) popupBottomSkillName.text = skillData.skillName;
        if (popupBottomSkillLevel != null) popupBottomSkillLevel.text = "Lvl: " + skillData.requiredLevel.ToString();
        if (popupBottomSkillDescription != null) popupBottomSkillDescription.text = skillData.skillDescription;
        if (popupBottomSkillKind != null) popupBottomSkillKind.text = skillData.element.ToString();
    }

    public float CalculateRemainingSlotUnlocks()
    {
        const float slotsPerLevel = 0.2f;
        const int baseSlots = 2; // Darion başlangıçta 2 slotla başlar asdas

        // Toplam hak edilen slot miktarı
        float totalEntitledSlots = Mathf.Floor(StatsManager.Instance.currentLevel * slotsPerLevel) + baseSlots;

        // UI sınırlarını kontrol et (usingSkillSlots senin dizindi)
        totalEntitledSlots = Mathf.Min(totalEntitledSlots, usingSkillSlots.Length);

        float alreadyOpenedSlots = StatsManager.Instance.openedSkillSlots;
        float remaining = totalEntitledSlots - alreadyOpenedSlots;

        return Mathf.Max(0, remaining);
    }

    public void removeUsingSkill(string skillID)
    {
        for (int i = 0; i < StatsManager.Instance.usingSkillsIDs.Length; i++)
        {
            if (StatsManager.Instance.usingSkillsIDs[i] == skillID)
            {
                StatsManager.Instance.usingSkillsIDs[i] = "";
                break;
            }
        }

        _ = StatsManager.Instance.SaveProgress("AutoSave_Slot");
        manageUsingSkillSlots();

        // 🎯 KRİTİK EKSİK: HUD'ı hemen uyandır! asdas
        StatsManager.OnSkillHUDUpdated?.Invoke();
        if (skillInfoPopup != null) skillInfoPopup.SetActive(false);
    }

    public void addUsingSkill(string skillID)
    {
        // 1. KONTROL: Data ve Kilit Durumu asdas
        SkillData data = FindSkillByID(skillID);
        if (data == null) return;

        bool isUnlocked = false;
        if (StatsManager.Instance.unlockedSkillsIDs != null)
        {
            foreach (string id in StatsManager.Instance.unlockedSkillsIDs)
            {
                if (id == skillID) { isUnlocked = true; break; }
            }
        }

        if (!isUnlocked)
        {
            // 🎯 YENİ MÜHÜR: Level yetse bile ön koşul yetmiyor olabilir!
            if (StatsManager.Instance.currentLevel >= data.requiredLevel && ArePreSkillsUnlocked(data))
            {
                ShowSkillUnlockConfirmation(data);
                return;
            }
            else
            {
                // Kullanıcıya tam olarak neyin eksik olduğunu söyleyelim cam gibi!
                if (!ArePreSkillsUnlocked(data))
                    UIManager.Instance.ShowWarning("You must unlock previous skills in the tree first!");
                else
                    UIManager.Instance.ShowWarning($"You need level {data.requiredLevel}!");

                return;
            }
        }

        // 2. KONTROL: Zaten Takılı mı?
        foreach (string id in StatsManager.Instance.usingSkillsIDs)
        {
            if (id == skillID)
            {
                UIManager.Instance.ShowWarning("Skill already equipped!");
                return;
            }
        }

        // --- SENİOR DOKUNUŞU 1: FLOAT GÜVENLİĞİ cam gibi! asdas ---
        // (int) cast yerine RoundToInt kullanarak 0.9999 gibi kaymaları önlüyoruz
        int openCount = Mathf.RoundToInt(StatsManager.Instance.openedSkillSlots);

        // Dizi boyutu kontrolü ve genişletme
        if (StatsManager.Instance.usingSkillsIDs.Length < openCount)
        {
            string[] largerArray = new string[openCount];
            System.Array.Copy(StatsManager.Instance.usingSkillsIDs, largerArray, StatsManager.Instance.usingSkillsIDs.Length);
            StatsManager.Instance.usingSkillsIDs = largerArray;
        }

        int targetIndex = -1;
        for (int i = 0; i < openCount; i++) // Sadece açık olan i < 2 için döner cam gibi!
        {
            if (string.IsNullOrEmpty(StatsManager.Instance.usingSkillsIDs[i]))
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex == -1)
        {
            UIManager.Instance.ShowWarning("No empty slots! Buy more with diamonds. asdas");
            return;
        }

        // İşlemi mühürle
        StatsManager.Instance.usingSkillsIDs[targetIndex] = skillID;

        // Otomatik kayıt (asdas)
        _ = StatsManager.Instance.SaveProgress("AutoSave_SlotEquip");

        // --- SENİOR DOKUNUŞU 2: GLOBAL SENKRONİZASYON cam gibi! ---
        // Kendi popup UI'ını tazele
        manageUsingSkillSlots();


        UIManager.Instance.ShowWarning($"{data.skillName} equipped successfully!");
        Debug.Log($"<color=green>Aritheon:</color> {skillID} rünü {targetIndex + 1}. slota yerleşti.");

        StatsManager.OnSkillHUDUpdated?.Invoke();

        if (skillInfoPopup != null) skillInfoPopup.SetActive(false);
    }

    // Artık bu metot cam gibi sadeleşti!
    public void ShowSkillUnlockConfirmation(SkillData skillData)
    {
        if (lockedSkillWarnPopup != null)
        {
            // Tüm topu LockedSkillWarnManager'a atıyoruz asdas
            lockedSkillWarnPopup.OpenForSkill(skillData);
        }
    }
    // SkillsPopupManager.cs içindeki o metod cam gibi tertemiz olmalı asdas
    public void UnlockSkillWithDiamonds(SkillData skillData)
    {
        if (StatsManager.Instance.currentDiamonds >= skillData.Cost_Diamond)
        {
            StatsManager.Instance.currentDiamonds -= (int)skillData.Cost_Diamond;

            // 1. ADIM: SADECE YETENEK MÜHÜRÜNÜ AÇIYORUZ
            List<string> tempUnlocked = new List<string>(StatsManager.Instance.unlockedSkillsIDs);
            if (!tempUnlocked.Contains(skillData.skillID))
            {
                tempUnlocked.Add(skillData.skillID);
                StatsManager.Instance.unlockedSkillsIDs = tempUnlocked.ToArray();
            }

            // 2. ADIM: UI GÜNCELLEME asdas
            manageLockedSkillSlots();
            manageUsingSkillSlots();

            // 3. ADIM: OTOMATİK KUŞANMA (SADECE BOŞ YER VARSA!)
            // Bu metod sadece açık slotlara (openedSkillSlots) bakar, yenisini AÇMAZ.
            addUsingSkill(skillData.skillID);

            StatsManager.OnSkillHUDUpdated?.Invoke();

            // Satın alma işlemi bittiğinde cam gibi kapatıyoruz asdas
            if (lockedSkillWarnPopup != null)
                lockedSkillWarnPopup.gameObject.SetActive(false);

            _ = StatsManager.Instance.SaveProgress("AutoSave_SkillUnlocked");
            Debug.Log($"<color=green>Aritheon:</color> {skillData.skillName} yeteneği açıldı. Slotlar hala aynı!");
        }
    }
    public void manageUsingSkillSlots()
    {
        string[] skillsIDs = StatsManager.Instance.usingSkillsIDs;
        int openCount = Mathf.RoundToInt(StatsManager.Instance.openedSkillSlots);

        for (int i = 0; i < usingSkillSlots.Length; i++)
        {
            if (usingSkillSlots[i] == null) continue;
            SkillSlotManager slot = usingSkillSlots[i].GetComponent<SkillSlotManager>();

            // Sadece açılmış slotlara rün yerleştirilebilir asdas
            if (i < openCount)
            {
                if (i < skillsIDs.Length && !string.IsNullOrEmpty(skillsIDs[i]))
                {
                    SkillData foundData = FindSkillByID(skillsIDs[i]);
                    UpdateSlotUI(slot, foundData, false);
                }
                else
                {
                    UpdateSlotUI(slot, null, false); // Boş ama açık slot
                }
            }
            else
            {
                UpdateSlotUI(slot, null, true); // Kilitli slot
            }
        }

        StatsManager.OnSkillHUDUpdated?.Invoke();
    }

    // UI güncelleme için yardımcı metod asdas
    private void UpdateSlotUI(SkillSlotManager slot, SkillData data, bool isLocked)
    {
        if (slot.skillIcon != null)
        {
            Image iconImage = slot.skillIcon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = isLocked ? lockerIcon : (data != null ? data.skillIcon : null);
                slot.skillIcon.SetActive(iconImage.sprite != null);
            }
        }

        if (slot.enabler != null) slot.enabler.SetActive(isLocked);

        // Şartları kaldırıyoruz, her zaman senkronize olsun cam gibi!
        slot.skillData = data;
        slot.isLocked = isLocked;
        SkillBarManager.Instance.RefreshAllSkillSlots();
    }

    public void TryUnlockNewSlot()
    {
        int diamondCost = 20; // Örn: Her slot 100 elmas asdas
        float remainingUnlocks = CalculateRemainingSlotUnlocks();

        if (remainingUnlocks > 0)
        {
            if (StatsManager.Instance.currentDiamonds >= diamondCost)
            {
                StatsManager.Instance.currentDiamonds -= diamondCost;
                StatsManager.Instance.openedSkillSlots++; // Yeni slot mühürlendi!

                manageUsingSkillSlots(); // UI tazele cam gibi!
            }
            else
            {
                Debug.Log("Yeterli elmasın yok Darion! asdas");
            }
        }

        // 🎯 TEK SATIRLIK MÜHÜR: Tüm HUD'ı ve sistemi uyar!
        StatsManager.OnSkillHUDUpdated?.Invoke();

        if (lockedSkillWarnPopup != null)
            lockedSkillWarnPopup.gameObject.SetActive(false);

        _ = StatsManager.Instance.SaveProgress("AutoSave_SlotUnlocked");


    }
    public void manageLockedSkillSlots()
    {
        float currentLevel = StatsManager.Instance.currentLevel;

        foreach (GameObject slotObj in skillSlots)
        {
            if (slotObj == null) continue;
            SkillSlotManager slot = slotObj.GetComponent<SkillSlotManager>();

            if (slot != null && slot.skillData != null)
            {
                // 1. Level Kontrolü
                bool levelMet = currentLevel >= slot.skillData.requiredLevel;

                // 2. Ön Koşul Kontrolü (Yeni!) asdas
                bool preSkillsMet = ArePreSkillsUnlocked(slot.skillData);

                // 🎯 İKİ ŞART DA SAĞLANMALI
                bool isFullyLocked = !levelMet || !preSkillsMet;

                slot.setLocked(isFullyLocked);
                slot.isLocked = isFullyLocked;

                // Slider'ları da bu yeni duruma göre güncelle cam gibi!
                slot.UpdateSliders();
            }
        }
    }

    // SkillsPopupManager.cs içine bu yardımcı metodu ekle asdas
    public bool ArePreSkillsUnlocked(SkillData skillData)
    {
        // Eğer ön koşul yoksa direkt true dön, yol açık!
        if (skillData.LevelParentIDs == null || skillData.LevelParentIDs.Count == 0) return true;

        // StatsManager'daki açılmış skilleri listeye alıyoruz
        List<string> unlockedList = new List<string>(StatsManager.Instance.unlockedSkillsIDs);

        // Her bir parent ID'yi kontrol et
        foreach (string preID in skillData.LevelParentIDs)
        {
            if (string.IsNullOrEmpty(preID)) continue;

            // Eğer listede bu ID yoksa, zincir kopuktur! asdas
            if (!unlockedList.Contains(preID)) return false;
        }

        return true; // Tüm zincir tamam!
    }

    public void openSkillSlotInfo(SkillData skillData, GameObject slotObject = null, bool isUsingSlot = false)
    {
        // 1. ADIM: Slot Manager'ı önbelleğe alalım asdas
        SkillSlotManager slot = null;
        if (slotObject != null)
        {
            slot = slotObject.GetComponent<SkillSlotManager>();
        }

        // 2. ADIM: Kilit kontrolü (Artık return yapmıyoruz!) cam gibi!
        if (slot != null && slot.isLocked)
        {
            // Eğer bu ana slotlardansa (Using Skill Slot) elmas uyarısını tetikle
            if (slot.isUsingSkillSlot)
            {
                OnClickLockedSlot(lockedSkillWarnPopup);
            }
            // Buradaki 'return'ü sildik! Artık kod aşağıya, popup açmaya devam edecek. asdas
        }

        // 3. ADIM: SkillData Belirleme (Slotun içindeki data her zaman öncelikli)
        if (slot != null && slot.skillData != null)
        {
            skillData = slot.skillData;
        }

        // Emniyet kemeri: Eğer elimizde hala data yoksa (boş slot vb.) çalışmasın asdas
        if (skillData == null)
        {
            Debug.LogWarning("<color=orange>Aritheon:</color> Gösterilecek data bulunamadı! asdas");
            return;
        }

        // 4. ADIM: Popup'ı Aktif Et ve Doldur
        skillInfoPopup.SetActive(true);

        // Yazıları ve görselleri mühürleyelim cam gibi!
        skillImage.sprite = skillData.skillIcon;
        skillLevel.text = "Lvl: " + skillData.requiredLevel.ToString();
        skillName.text = skillData.skillName;
        skillType.text = skillData.skillType.ToString();
        skillKind.text = skillData.element.ToString();
        skillDescription.text = skillData.skillDescription;
        manaCostText.text = skillData.manaCost.ToString();
        costGoldText.text = skillData.Cost_Gold.ToString();
        DiamondCostText.text = skillData.Cost_Diamond.ToString();
        cooldownText.text = skillData.cooldown.ToString();
        castTimeText.text = skillData.castTime.ToString();
        damageText.text = skillData.damage.ToString();
        defenceText.text = skillData.defence.ToString();
        rangeText.text = skillData.range.ToString();
        efectDurationText.text = skillData.effectDuration.ToString();
        knockBackText.text = skillData.knockbackForce.ToString();
        reinceformentBonusText.text = skillData.Reinforcement_Bonus.ToString();
        scaleFactorText.text = skillData.ScaleFactor.ToString();

        // Ön Koşul (Pre-Skill) İkonları asdas
        UpdatePreSkillIcons(skillData);

        initBottomPopup(skillData);

        // 5. ADIM: Buton Ayarları (Add/Remove)
        SetupActionButton(slot, skillData);
    }

    // Kodun okunabilirliği için ikon kısmını küçük bir yardımcı metoda mühürledim asdas
    private void UpdatePreSkillIcons(SkillData skillData)
    {
        if (skillData.LevelParentIDs != null && skillData.LevelParentIDs.Count > 0)
        {
            preSkillImage1.sprite = GetPreSkillIcon(skillData.LevelParentIDs[0]);
            preSkillImage1.gameObject.SetActive(true);
        }
        else preSkillImage1.gameObject.SetActive(false);

        if (skillData.LevelParentIDs != null && skillData.LevelParentIDs.Count > 1)
        {
            preSkillImage2.sprite = GetPreSkillIcon(skillData.LevelParentIDs[1]);
            preSkillImage2.gameObject.SetActive(true);
        }
        else preSkillImage2.gameObject.SetActive(false);
    }
    private void SetupActionButton(SkillSlotManager slot, SkillData skillData)
    {
        if (addSkillSlotButton == null) return;

        addSkillSlotButton.onClick.RemoveAllListeners(); // Eskileri mühürle!

        if (slot != null && slot.isUsingSkillSlot)
        {
            // Eğer bu slota rün takılmışsa 'Remove' butonu olsun cam gibi!
            addSkillSlotButtonImage.sprite = removeIcon;
            addSkillSlotButton.onClick.AddListener(() => removeUsingSkill(skillData.skillID));
        }
        else
        {
            // Beceri ağacından bir rüne tıklandıysa 'Add' butonu olsun asdas
            addSkillSlotButtonImage.sprite = checkMarkIcon;
            addSkillSlotButton.onClick.AddListener(() => addUsingSkill(skillData.skillID));
        }
    }

    public void OnClickLockedSlot(LockedSkillWarnManager lockedSkillWarnPopup)
    {
        float remaining = CalculateRemainingSlotUnlocks();
        ShowLockedSkillWarning(lockedSkillWarnPopup);

    }


    // manageUsingSkillSlots veya slota tıklama anında asdas

    // SkillsPopupManager.cs içindeki bu metodu böyle mühürle asdas
    public SkillData FindSkillByID(string id)
    {
        if (string.IsNullOrEmpty(id) || SkillBarManager.Instance == null) return null;

        // UI'dan değil, ana veritabanından çekiyoruz!
        return SkillBarManager.Instance.FindSkillByID(id);
    }

    // 2. GÜNCELLENMİŞ İKON GETİRME METODU
    private Sprite GetPreSkillIcon(string skillID)
    {
        // Önce yukarıdaki yardımcı metodla datayı buluyoruz asdas
        SkillData preSkill = FindSkillByID(skillID);

        if (preSkill != null)
        {
            return preSkill.skillIcon;
        }

        return null;
    }

    void Start()
    {
        // Başlangıçta mevcut leveli kaydedelim ki ilk karede gereksiz tetiklenmesin asdas
        lastKnownLevel = StatsManager.Instance.currentLevel;

        UpdateStats(StatsManager.Instance.maxHealth, StatsManager.Instance.maxMana,
                    StatsManager.Instance.allAttack, StatsManager.Instance.allDefense,
                    StatsManager.Instance.FireAttack, StatsManager.Instance.IceAttack);

        manageLockedSkillSlots();
        manageUsingSkillSlots();
    }

    void Update()
    {
        // Scriptin bağlı olduğu obje aktifse ve level değişmişse tetikle cam gibi!
        if (gameObject.activeInHierarchy)
        {
            float currentLevel = StatsManager.Instance.currentLevel;

            if (currentLevel != lastKnownLevel)
            {
                Debug.Log($"<color=green>Aritheon:</color> Level değişimi yakalandı! Yeni Level: {currentLevel} asdas");

                // 1. Slot kilitlerini güncelle
                manageLockedSkillSlots();
                manageUsingSkillSlots();


                // 2. UI istatistiklerini tazele
                UpdateStats(StatsManager.Instance.maxHealth, StatsManager.Instance.maxMana,
                            StatsManager.Instance.allAttack, StatsManager.Instance.allDefense,
                            StatsManager.Instance.FireAttack, StatsManager.Instance.IceAttack);

                // 3. Son leveli güncelle ki döngüye girmesin
                lastKnownLevel = currentLevel;
            }
        }
    }

    public void UpdateStats(float maxHealth, float maxMana, float allAttack, float allDefense, float allFireAttack, float allIceAttack)
    {
        if (maxHealthText != null) maxHealthText.text = maxHealth.ToString();
        if (maxManaText != null) maxManaText.text = maxMana.ToString();
        if (allAttackText != null) allAttackText.text = allAttack.ToString();
        if (allDefenseText != null) allDefenseText.text = allDefense.ToString();
        if (allFireAttackText != null) allFireAttackText.text = allFireAttack.ToString();
        if (allIceAttackText != null) allIceAttackText.text = allIceAttack.ToString();
    }
}
