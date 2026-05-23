using UnityEngine;
using TMPro;
using UnityEngine.UI; // Buton kontrolü için ekledik

public class LockedSkillWarnManager : MonoBehaviour
{
    [Header("UI Elementleri")]
    public TextMeshProUGUI diamondCostText;
    public TextMeshProUGUI descriptionText;
    public Button unlockButton; // Inspector'dan UNLOCK butonunu buraya sürükle cam gibi!

    // SLOT AÇMA MODU (Senin mevcut metodun biraz güncellendi)
    public void OpenForSlot(int cost, float remainingSlots)
    {
        gameObject.SetActive(true);

        if (diamondCostText != null) diamondCostText.text = "x" + cost.ToString();
        if (descriptionText != null) descriptionText.text = $"You can unlock {remainingSlots} more skill slots";

        // Butonun görevini mühürleyelim asdas
        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(UnlockSkillSlot);
        }
    }

    // SKILL AÇMA MODU (Yeni eklediğimiz mühür) asdas
    public void OpenForSkill(SkillData skillData)
    {
        if (skillData == null) return;
        gameObject.SetActive(true);

        if (diamondCostText != null) diamondCostText.text = "x" + skillData.Cost_Diamond.ToString();
        if (descriptionText != null) descriptionText.text = $"Do you want to unlock {skillData.skillName}?";

        // Butonun görevini bu yeteneğe özel mühürleyelim cam gibi!
        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(() => SkillsPopupManager.Instance.UnlockSkillWithDiamonds(skillData));
            unlockButton.onClick.AddListener(Close); // Satın alınca kapat asdas
        }
    }

    // SLOT AÇMA MANTIĞI (Async - Kayıt yapar)
    // LockedSkillWarnManager.cs içindeki o metod asdas
    public async void UnlockSkillSlot()
    {
        int cost = 20; // Slot mühürleme bedeli asdas
        if (StatsManager.Instance.currentDiamonds >= cost)
        {
            StatsManager.Instance.currentDiamonds -= cost;

            // KRİTİK AYRIM: SADECE YUVA SAYISINI ARTIRIYORUZ
            StatsManager.Instance.openedSkillSlots += 1;

            await StatsManager.Instance.SaveProgress("AutoSave_SlotUnlocked");

            // UI'da kilitli slotun açıldığını gösteriyoruz
            SkillsPopupManager.Instance.manageUsingSkillSlots();
            Close();
            Debug.Log("<color=cyan>Aritheon:</color> Yeni rün yuvası mühürlendi!");
        }
    }

    public void Close() => gameObject.SetActive(false);
}