using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    private void OnEnable()
    {
        // Panel her açıldığında (SetActive(true)) bu blok tetiklenir
        if (CharacterVisualManager.Instance != null)
        {
            
        }
    }
}