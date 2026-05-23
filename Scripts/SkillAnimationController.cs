using UnityEngine;
using UnityEngine.UI;

public class AnimationTestController : MonoBehaviour
{
    [Header("Animasyon Ayarları")]
    public string animationParameterName = "isAttack"; // Animator'daki Bool veya Trigger adı

    public void PlaySwordAnimation()
    {
        // 1. Oyuncuyu etiket ile bul
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            // 2. Oyuncunun üzerindeki Animator bileşenini al
            Animator anim = player.GetComponent<Animator>();

            if (anim != null)
            {
                // 3. Animasyonu başlat (Trigger kullanıyorsan SetTrigger, Bool ise SetBool kullanabilirsin)
                anim.SetTrigger(animationParameterName);
                
                Debug.Log("<color=green>Test:</color> Sword animasyonu tetiklendi!");
            }
            else
            {
                Debug.LogError("Hata: Player üzerinde Animator bulunamadı!");
            }
        }
        else
        {
            Debug.LogError("Hata: 'Player' etiketli obje sahnede yok!");
        }
    }
}