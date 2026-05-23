using UnityEngine;
using System.Collections; // Required for Coroutines

// Bu scripti Liora gibi 'Manager' altındaki 'Halan' ana objesine takacağız.
// Animator ise 'Halan' -> 'Visuals' child objesinde.
public class HalanBlacksmithAI : MonoBehaviour
{
    [Header("Animator Ayarları")]
    // Animator'daki Boolean parametresinin adı harfiyen aynı olmalı
    [SerializeField] private string isWorkingBoolParamName = "isMakeWeapon";
    private Animator halanAnimator;

    [Header("Zaman Ayarları (Saniye)")]
    // Halan'ın boşta bekleyeceği rastgele süre aralığı
    [SerializeField] private float minIdleTime = 3f;
    [SerializeField] private float maxIdleTime = 7f;

    // Halan'ın demir döveceği rastgele süre aralığı
    [SerializeField] private float minWorkTime = 5f;
    [SerializeField] private float maxWorkTime = 10f;

    void Awake()
    {
        // 1. KRİTİK ADIM: Halan ana objesinde bu script var, ama Animator çocuk objede (Visuals).
        // Bu yüzden 'GetComponentInParent' veya çocukta arama yapacağız canım.
        halanAnimator = GetComponentInChildren<Animator>();

        if (halanAnimator == null)
        {
            // Eğer referansı bulamazsa konsola bir hata mesajı atsın ki bilelim
            //Debug.LogError($"<color=red>HalanBlacksmithAI:</color> {gameObject.name} veya çocuklarında 'Animator' bileşeni bulunamadı! Lütfen referansı kontrol et canım.");
        }
    }

    void Start()
    {
        // 2. KRİTİK ADIM: Eğer animator bulunduysa, döngüyü başlatalım
        if (halanAnimator != null)
        {
            // Bu Coroutine, Halan yok olana kadar sonsuza kadar devam edecek
            StartCoroutine(BlacksmithWorkLoopRoutine());
        }
    }

    // Bu, Halan'ın durumlarını değiştiren sihirli Coroutine döngümüz
    IEnumerator BlacksmithWorkLoopRoutine()
    {
        // Sonsuz döngü, Broken only by GameObject destruction or script disabling
        while (true)
        {
            // 3. ADIM: BEKLEME (IDLE) DURUMUNA GEÇ
            // bool parametresini false yapıp idle'a dönüyoruz.
            // Bu satır senin o missingExitTime uyarısını susturan sihirli satır canım.
            halanAnimator.SetBool(isWorkingBoolParamName, false);
            //Debug.Log("<color=cyan>Halan:</color> Bekleme moduna geçildi, dinleniliyor.");

            // Belirlediğin rastgele süre kadar bekle (dinlensin)
            yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));

            // 4. ADIM: ÇALIŞMA (WORK) DURUMUNA GEÇ
            // bool parametresini true yapıp makeWeapon animasyonuna geçiyoruz.
            halanAnimator.SetBool(isWorkingBoolParamName, true);
            //Debug.Log("<color=yellow>Halan:</color> Demir dövme moduna geçildi, zırh yapılıyor.");

            // Belirlediğin rastgele süre kadar bekle (zırh yapsın)
            yield return new WaitForSeconds(Random.Range(minWorkTime, maxWorkTime));
        }
    }
}