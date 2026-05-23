using UnityEngine;

public class NPCAnimationBridge : MonoBehaviour
{
    // Liora'nın ayak sesi çıkarması için bu fonksiyonu hazırladık canım
    public void Step()
    {
        if (UIManager.Instance != null)
        {
            // Darion ile aynı havuzdan (UIManager'daki seslerden) faydalansın
          //  UIManager.Instance.PlayFootstepSound();
        }
    }
}