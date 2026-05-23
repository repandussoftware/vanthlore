using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum ButtonAction { ToggleInventory, ToggleMap, ToggleDuties, RestAtDarionsRoom, ToggleParchement, ToggleLootPopup, ToggleWardrobePopup, OpenWindowView, CloseAll, QuitRoom, GetInRoom, PlaySwordAnimation, GetInPalaestra, PlayMeleeAnimation, GetInLiorasRoom, GetInGuestRoom, GetInTavern, GetInNehalengrad, InteractHelen, OpenSkillPopup, ToggleSkillSlot }

    [Header("Buton Ayarları")]
    public ButtonAction actionType;

    public Animator playersAnimator;

    // UIButtonHandler.cs içine ekle:
    public int skillSlotIndex; // Müfettişten (Inspector) 0, 1, 2 diye ayarla canım

    [Header("Görsel Efektler")]
    public float scaleAmount = 1.1f;    // Üzerine gelince büyüme oranı
    public float clickScaleAmount = 0.9f; // Tıklayınca küçülme oranı

    private Vector3 originalScale;

    public SkillData skillData; // SkillData referansı ekleyelim

    public GameObject skillSlotObject; // Hangi slotun bilgilerini göstermek istediğimizi 
    // belirlemek için

    public bool isUsingSkillSlot = false; // Bu butonun bir skill slotu olup olmadığını belirlemek için

    public LockedSkillWarnManager lockedSkillWarnPopup; // Kilitli slot uyarısı için referans

    void Start()
    {
        // Başlangıçtaki ölçeği kaydet
        originalScale = transform.localScale;

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Mouse Tıkladı");
        // Tıklama efektini oynat
        transform.localScale = originalScale * clickScaleAmount;
        Invoke("ResetScale", 0.1f);

        // UIManager'ın varlığını kontrol et ve ilgili aksiyonu tetikle
        if (UIManager.Instance != null)
        {

            UIManager.Instance.PlayClickSound();

            switch (actionType)
            {
                case ButtonAction.ToggleInventory:
                    UIManager.Instance.ToggleInventory();
                    break;
                case ButtonAction.ToggleMap:
                    UIManager.Instance.ToggleMap();
                    break;
                case ButtonAction.ToggleDuties:
                    UIManager.Instance.ToggleDuties();
                    break;
                case ButtonAction.ToggleParchement:
                    UIManager.Instance.ToggleParchement();
                    break;
                case ButtonAction.ToggleLootPopup:
                    UIManager.Instance.ToggleLootPopup();
                    break;
                case ButtonAction.ToggleWardrobePopup:
                    UIManager.Instance.ToggleWardrobePopup();
                    break;
                case ButtonAction.RestAtDarionsRoom:
                    Debug.Log("Dinlenme butonuna tıklandı.");
                    UIManager.Instance.RestAtDarionsRoom();
                    break;
                case ButtonAction.OpenWindowView:
                    Debug.Log("Manzara Butonu Tıklandı");
                   // UIManager.Instance.OpenWindowView();
                    break;
                case ButtonAction.CloseAll:
                    UIManager.Instance.CloseAllPanels();
                    break;
                case ButtonAction.QuitRoom:
                    //UIManager.Instance.ChangeScene("Lunara_Inn_Coridor");
                    break;
                case ButtonAction.GetInRoom:
                    //UIManager.Instance.ChangeScene("Darions_Room");
                    break;
                // 2. Yeni kılıç animasyonu kontrolünü buraya ekledik
                case ButtonAction.PlaySwordAnimation:
                    //TriggerSwordAnimation();
                    //SkillBarManager.Instance.UseSkill(skillSlotIndex);
                    break;
                case ButtonAction.GetInPalaestra:
                    //UIManager.Instance.ChangeScene("Palaestra");
                    break;
                case ButtonAction.PlayMeleeAnimation:
                    // Bu buton 1. slotu (Yumruğu/Melee'yi) tetiklesin canım
                    //SkillBarManager.Instance.UseSkill(skillSlotIndex);
                    break;
                case ButtonAction.GetInLiorasRoom:
                    //UIManager.Instance.ChangeScene("Lioras_Room");
                    break;
                case ButtonAction.GetInGuestRoom:
                    //UIManager.Instance.ChangeScene("Guest_Room");
                    break;
                case ButtonAction.GetInTavern:
                    //UIManager.Instance.ChangeScene("Tavern");
                    break;
                case ButtonAction.GetInNehalengrad:
                    //UIManager.Instance.ChangeScene("Nehalengrad");
                    break;
                case ButtonAction.InteractHelen:
                    //UIManager.Instance.interactWithHelen();
                    break;
                case ButtonAction.OpenSkillPopup:
                    UIManager.Instance.ToggleSkillPopup();
                    break;
                case ButtonAction.ToggleSkillSlot:
                    Debug.Log("Skill slotu butonuna tıklandı!");
                    if (skillData != null)
                    {
                        SkillsPopupManager.Instance.openSkillSlotInfo(skillData, skillSlotObject, isUsingSkillSlot);
                    }
                    else if (skillSlotObject != null && !skillSlotObject.GetComponent<SkillSlotManager>().isLocked)
                    {
                        SkillsPopupManager.Instance.openSkillSlotInfo(skillSlotObject.GetComponent<SkillSlotManager>().skillData, skillSlotObject, isUsingSkillSlot);
                    }
                    else
                    {
                        Debug.Log("On Clicked'a geçti");
                        SkillsPopupManager.Instance.OnClickLockedSlot(lockedSkillWarnPopup);
                    }

                    break;

            }
        }
        else
        {
            Debug.LogWarning("UIManager bulunamadı! Lütfen sahnede bir UIManager olduğundan emin ol.");
        }
    }

    private void TriggerSwordAnimation()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            if (playersAnimator != null)
            {
                // Animator içindeki tetikleyici (trigger) adının "isAttack" olduğundan emin ol
                playersAnimator.SetTrigger("isAttack");

                Debug.Log("<color=yellow>Darion:</color> Kılıç animasyonu tetiklendi!");
            }
        }
    }

    private void TriggerMeleeAnimation()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            if (playersAnimator != null)
            {
                // Animator içindeki tetikleyici (trigger) adının "isMelee" olduğundan emin ol
                playersAnimator.SetTrigger("isMelee");
                Debug.Log("<color=yellow>Darion:</color> Melee animasyonu tetiklendi!");
            }
        }
    }

    // Fare üzerine gelince büyüme
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * scaleAmount;
    }

    // Fare üzerinden çekilince eski boyuta dönme
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }

    void ResetScale()
    {
        transform.localScale = originalScale;
    }
}