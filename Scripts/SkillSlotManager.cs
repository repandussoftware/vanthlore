using UnityEngine;
using UnityEngine.UI;

public class SkillSlotManager : MonoBehaviour
{
    public SkillData skillData;
    public GameObject skillIcon;
    public GameObject enabler;

    [Header("Progress Sliders")]
    public Slider[] previousSliders; // Bu yeteneğe gelen yollar asdas
    public Slider[] nextSliders;     // Bu yetenekten giden yollar cam gibi!

    public bool isUsingSkillSlot = false;
    public bool isLocked = true;

    public void setLocked(bool isLocked)
    {
        this.isLocked = isLocked;

        if (enabler != null)
        {
            enabler.SetActive(isLocked);
        }
        else
        {
            Debug.LogError($"<color=red>Aritheon Hatası:</color> {gameObject.name} üzerindeki 'enabler' atanmamış! asdas");
        }

        // 🎯 KRİTİK MÜHÜR: Kilit durumu güncellendiğinde slider'ları da tazeliyoruz
        UpdateSliders();
    }

    // Darion'un seviyesini slider'lara mühürleyen o meşhur metod asdas
    public void UpdateSliders()
    {
        // 🛡️ Emniyet Kemerleri asdas
        if (StatsManager.Instance == null || skillData == null || SkillsPopupManager.Instance == null) return;

        float currentLvl = (float)StatsManager.Instance.currentLevel;

        // 1. ADIM: Kontrolleri mühürleyelim cam gibi!
        // Bir önceki rünlerin açılıp açılmadığını SkillsPopupManager'dan öğreniyoruz asdas
        bool preSkillsUnlocked = SkillsPopupManager.Instance.ArePreSkillsUnlocked(skillData);

        // Bu rünün kendisinin açılıp açılmadığını StatsManager'dan kontrol ediyoruz
        bool thisSkillUnlocked = false;
        foreach (string id in StatsManager.Instance.unlockedSkillsIDs)
        {
            if (id == skillData.skillID) { thisSkillUnlocked = true; break; }
        }

        // 2. ADIM: GİRİŞ YOLLARI (Previous Sliders) asdas
        // Eğer önceki skiller açılmamışsa, level yetse bile 0 kalır!
        if (previousSliders != null)
        {
            foreach (Slider s in previousSliders)
            {
                if (s != null)
                {
                    s.value = preSkillsUnlocked ? currentLvl : 0;
                }
            }
        }

        // 3. ADIM: ÇIKIŞ YOLLARI (Next Sliders) cam gibi!
        // Eğer Darion bu skilli henüz açmadıysa, sonraki yollar asla dolmaz!
        if (nextSliders != null)
        {
            foreach (Slider s in nextSliders)
            {
                if (s != null)
                {
                    s.value = thisSkillUnlocked ? currentLvl : 0;
                }
            }
        }
    }
}