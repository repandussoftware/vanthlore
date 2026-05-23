using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.EventSystems;

public class InteractionTrigger : MonoBehaviour
{
    [Header("UI Animasyon Bağlantısı")]
    public WorldSpaceUIAnimator uiAnimator;

    [Header("Etkileşim Ayarları")]
    public string playerTag = "Player";
    
    [Header("Giriş Aksiyonu")]
    public InputActionReference interactAction; 

    private bool isPlayerInRange = false;

    [Header("Olay Kimliği")]
    public string interactionEvent = "RestAtDarionsRoom";

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // Sadece oyuncu menzildeyse ve aksiyon 'performed' aşamasındaysa çalıştır
        if (isPlayerInRange && context.performed)
        {
            PerformInteraction();
        }
    }

    private void PerformInteraction()
    {
        Debug.Log($"<color=cyan>Etkileşim:</color> <b>{gameObject.name}</b> ile etkileşime girildi!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            
            // --- TCL İÇİN KRİTİK GÜVENLİK KONTROLÜ ---
            // Obje sahne hiyerarşisinde aktif değilse Show() çağırma, hata almayalım!
            if (uiAnimator != null && uiAnimator.gameObject.activeInHierarchy) 
            {
                uiAnimator.Show();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;

            // --- TCL İÇİN KRİTİK GÜVENLİK KONTROLÜ ---
            // Obje aktif değilse Hide() çağırmak o kırmızı hatalara sebep olur.
            if (uiAnimator != null && uiAnimator.gameObject.activeInHierarchy) 
            {
                uiAnimator.Hide();
            }
        }
    }
}