using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// IPointerClickHandler arabirimini de ekleyerek çift dikiş yapıyoruz cam gibi!
public class GlobalKeyboardTrigger : MonoBehaviour, ISelectHandler, IPointerClickHandler
{
    private TMP_InputField myInputField;

    void Awake()
    {
        myInputField = GetComponent<TMP_InputField>();
    }

    // Odaklanma değiştiğinde tetiklenir
    public void OnSelect(BaseEventData eventData)
    {
        TriggerKeyboard();
    }

    // 🎯 ÇİFT GÜVENCE MÜHÜRÜ: EventSystem odak geçişini kaçırsa bile, 
    // oyuncu kutuya fiziksel olarak tıkladığı an burası kesinlikle çalışır!
    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerKeyboard();
    }

    private void TriggerKeyboard()
    {
        if (CustomKeyboardManager.Instance != null && myInputField != null)
        {
            CustomKeyboardManager.Instance.ShowKeyboard(myInputField);
        }
    }
}