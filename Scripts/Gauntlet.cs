using UnityEngine;
using System.Collections.Generic;

public class GauntletSpriteSync : MonoBehaviour
{
    public Animator mainBodyAnimator;
    public string animName = "Darion_Run_V1"; // Hata alırsan burayı "Base Layer.Darion_Run_V1" yapabilirsin
    public string equippedBool = "isGauntletEquipped";
    public List<Sprite> runFrames = new List<Sprite>();
    public Sprite idleSprite;

    private SpriteRenderer sr;

    void Start() {
        sr = GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "Armor";
        sr.sortingOrder = 10; // En üstte
    }

    void LateUpdate() {
        if (!mainBodyAnimator.GetBool(equippedBool)) { sr.enabled = false; return; }
        sr.enabled = true;

        AnimatorStateInfo state = mainBodyAnimator.GetCurrentAnimatorStateInfo(0);
        if (mainBodyAnimator.GetBool("isRunning") && state.IsName(animName)) {
            int frame = Mathf.FloorToInt((state.normalizedTime % 1f) * runFrames.Count);
            if (frame < runFrames.Count) sr.sprite = runFrames[frame];
        } else {
            sr.sprite = idleSprite;
        }
        transform.localPosition = Vector3.zero;
    }
}