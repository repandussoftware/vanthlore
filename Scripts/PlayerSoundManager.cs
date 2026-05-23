using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    [Header("Kılıç Sesleri")]
    public AudioSource audioSource;
    public AudioClip[] swordSwingSounds; // Buraya 3 ses dosyanı sürükleyeceksin

    // Bu fonksiyonu animasyonun içinden çağıracağız
    public void PlayRandomSwingSound()
    {
        if (swordSwingSounds.Length > 0 && audioSource != null)
        {
            // 0 ile 3 arasında rastgele bir sayı seçer
            int randomIndex = Random.Range(0, swordSwingSounds.Length);
            
            // Seçilen sesi bir kez çalar
            audioSource.PlayOneShot(swordSwingSounds[randomIndex]);
            
            Debug.Log($"<color=silver>Ses Çalındı:</color> {swordSwingSounds[randomIndex].name}");
        }
    }
}