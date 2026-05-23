using UnityEngine;
using UnityEngine.EventSystems;

public class UICloseDetector : MonoBehaviour, IPointerDownHandler
{
    public GameObject panelToClose; // Buraya Inspector'dan itemDetailsPanelObj'i sürükle

    public void OnPointerDown(PointerEventData eventData)
    {
        if (panelToClose != null)
            panelToClose.SetActive(false);
    }
}