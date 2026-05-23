using UnityEngine;
using System.Collections.Generic;

public class ModularSpriteSync : MonoBehaviour
{
    [Header("Referanslar")]
    public Animator mainBodyAnimator;
    private SpriteRenderer slotRenderer;

    [Header("Animator Parametreleri")]
    // Buraya Animator'daki parametre isimlerini tam yazmalısın canım
    public string equippedBool = "isBootEquipped";
    public string runningBool = "isRunning";

    [Header("Animasyon Kareleri")]
    // Adobe'den gelen o 23 kare buraya!
    public List<Sprite> runFrames = new List<Sprite>();
    public Sprite idleSprite;

    void Start()
    {
        slotRenderer = GetComponent<SpriteRenderer>();
        if (slotRenderer != null)
        {
            slotRenderer.sortingLayerName = "Armor";
            slotRenderer.sortingOrder = 10;
        }
    }

    void LateUpdate()
    {
        if (mainBodyAnimator == null || slotRenderer == null) return;

        bool isEquipped = mainBodyAnimator.GetBool(equippedBool);
        bool isRunning = mainBodyAnimator.GetBool(runningBool);

        // Konsola durumu yazdırıyoruz (Hangi kapıda takıldığımızı anlamak için)
       Debug.Log($"Equipped: {isEquipped} | Running: {isRunning}"); 

        if (!isEquipped)
        {
            slotRenderer.enabled = false; // Takılı değilse direkt Renderer'ı kapat
            return;
        }

        slotRenderer.enabled = true;

        if (isRunning && runFrames.Count > 0)
        {
            AnimatorStateInfo stateInfo = mainBodyAnimator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName("Darion_Run_V1"))
            {
                float normalizedTime = stateInfo.normalizedTime % 1f;
                int currentFrame = Mathf.FloorToInt(normalizedTime * runFrames.Count);

                if (currentFrame < runFrames.Count)
                    slotRenderer.sprite = runFrames[currentFrame];
            }
            else
            {
                // Eğer buraya düşüyorsa Animasyon adı yanlıştır!
                Debug.LogWarning("Animasyon adı eşleşmiyor! Şu anki: " + stateInfo.fullPathHash);
            }
        }
        else
        {
            if (idleSprite != null) slotRenderer.sprite = idleSprite;
        }

        transform.localPosition = Vector3.zero;
    }
}