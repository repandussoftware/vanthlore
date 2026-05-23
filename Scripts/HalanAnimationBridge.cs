using UnityEngine;

// HalanBlacksmithAI.cs içine ekle veya Bridge scripti oluştur
public class HalanAnimationBridge : MonoBehaviour
{
    public AudioSource hammerAudioSource;
    public AudioClip hammerHitSound;

    // Animasyon penceresinde çekicin örse değdiği kareye bu fonksiyonu ekle canım
    public void PlayHammerHitSound()
    {
        if (hammerAudioSource != null && hammerHitSound != null)
        {
            hammerAudioSource.PlayOneShot(hammerHitSound);
            
           // Debug.Log("<color=orange>Halan:</color> Demir dövüldü!");
        }
    }
}
