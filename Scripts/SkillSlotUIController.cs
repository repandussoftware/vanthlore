using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUIController : MonoBehaviour
{
    [Header("UI Elementleri")]
    public Button skillSlotButton;
    public Image skillSlotIcon;

    [Header("Slot Bilgisi")]
    public int slotIndex;
    public SkillData assignedSkill;

    [Header("VFX & Feedback")]
    public Image cooldownOverlay;

    [Header("Pure Code Juice")]
    public Image skillSlotFrame;
    private bool isReadyToFlash = false;
    private float effectTimer = 0f;
    private Vector3 initialScale;

    private float currentCooldownTimer = 0f;
    private bool isCooldownActive = false;

    // 🎯 MÜHÜRLER
    private bool isWaitingForHit = false;
    private float hitWaitTimer = 0f;
    private float maxHitWaitTime = 1.5f; // 1.5 saniye içinde hit gelmezse iptal et

    void Start()
    {
        initialScale = transform.localScale;
    }

    public void RefreshSlot()
    {
        if (StatsManager.Instance == null || SkillBarManager.Instance == null) return;

        string skillID = "";
        // 🛡️ Dizi boyutu kontrolü ve null güvenliği asdas
        if (slotIndex < StatsManager.Instance.usingSkillsIDs.Length)
        {
            string rawID = StatsManager.Instance.usingSkillsIDs[slotIndex];
            skillID = !string.IsNullOrEmpty(rawID) ? rawID.Trim() : "";
        }

        assignedSkill = !string.IsNullOrEmpty(skillID) ? SkillBarManager.Instance.FindSkillByID(skillID) : null;
        UpdateUI(assignedSkill);
        ResetJuice();
    }

    private void ResetJuice()
    {
        isReadyToFlash = false;
        isWaitingForHit = false;
        hitWaitTimer = 0f; // Zamanlayıcıyı sıfırla
        effectTimer = 0f;
        transform.localScale = initialScale;
        if (skillSlotFrame != null)
        {
            Color c = skillSlotFrame.color;
            c.a = 1f;
            skillSlotFrame.color = c;
        }
    }

    private void UpdateUI(SkillData skill)
    {

        gameObject.SetActive(true);

        if (skill == null)
        {
            if (skillSlotIcon != null) skillSlotIcon.enabled = false;
            if (skillSlotButton != null) skillSlotButton.interactable = false; // Boş slota basılmasın cam gibi!
            return;
        }

        if (skillSlotIcon != null)
        {
            skillSlotIcon.sprite = skill.skillIcon;
            skillSlotIcon.enabled = true;
        }

        isCooldownActive = false;
        currentCooldownTimer = 0f;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.gameObject.SetActive(false);
        }

        if (skillSlotButton != null)
        {
            skillSlotButton.interactable = true;
        }
    }

    public void SkillClicked()
    {
        if (assignedSkill == null || isCooldownActive || isWaitingForHit) return;

        bool skillStarted = SkillBarManager.Instance.UseSkill(assignedSkill, this);
        if (skillStarted)
        {
            ResetJuice();
            isWaitingForHit = true;
            hitWaitTimer = 0f; // Zamanlayıcı başladı

            if (skillSlotButton != null) skillSlotButton.interactable = false;

            Debug.Log($"<color=cyan>Aritheon:</color> {assignedSkill.skillName} animasyonu başladı...");
        }
    }

    public void StartActualCooldown()
    {
        if (!isWaitingForHit) return;

        isWaitingForHit = false;
        isCooldownActive = true;
        currentCooldownTimer = assignedSkill.cooldown;

        SkillBarManager.Instance.StartSkillCooldownTracking(assignedSkill);

        if (skillSlotButton != null) skillSlotButton.interactable = false;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1f;
        }
    }

    public void InterruptSkill()
    {
        isWaitingForHit = false;
        hitWaitTimer = 0f;
        if (skillSlotButton != null) skillSlotButton.interactable = true;
        Debug.Log("<color=orange>Aritheon:</color> Slot kurtarıldı, buton tekrar aktif! asdas");
    }

    void Update()
    {
        // 🎯 YENİ: Zaman Aşımı Kontrolü (Timeout)
        if (isWaitingForHit)
        {
            hitWaitTimer += Time.deltaTime;
            if (hitWaitTimer >= maxHitWaitTime)
            {
                // Çok bekledik, kesin animasyon bug'a girdi veya kesildi!
                if (SkillBarManager.Instance != null)
                {
                    SkillBarManager.Instance.CancelPendingSkill();
                }
            }
        }

        if (isCooldownActive)
        {
            currentCooldownTimer -= Time.deltaTime;

            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = currentCooldownTimer / assignedSkill.cooldown;

            if (currentCooldownTimer <= 0)
            {
                isCooldownActive = false;
                isReadyToFlash = true;
                effectTimer = 0f;

                if (cooldownOverlay != null) cooldownOverlay.gameObject.SetActive(false);
                if (skillSlotButton != null) skillSlotButton.interactable = true;

                transform.localScale = initialScale * 1.3f;
            }
        }

        if (isReadyToFlash) HandleReadyEffects();
    }

    private void HandleReadyEffects()
    {
        effectTimer += Time.deltaTime;
        transform.localScale = Vector3.Lerp(transform.localScale, initialScale, Time.deltaTime * 10f);

        if (skillSlotFrame != null)
        {
            float pulse = (Mathf.Sin(effectTimer * 8f) + 1f) / 2f;
            float finalAlpha = Mathf.Lerp(0.4f, 1f, pulse);
            Color c = skillSlotFrame.color;
            c.a = finalAlpha;
            skillSlotFrame.color = c;
        }
    }
}