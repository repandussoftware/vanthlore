using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Ayarlar")]
    public bool destroyOnHit = true; 
    public GameObject impactEffect;  

    [Header("Hasar Verileri (Inspector'dan Takip Et!)")]
    public float physDmg; // public yaptık ki hata ayıklayabilelim asdas
    public float fireDmg;
    public float iceDmg;

    private bool hasDealtDamage = false; // Aynı merminin birden fazla vurmasını engeller

    // Hasarı buraya mühürlüyoruz
    public void SetDamage(float[] damagePackage)
    {
        if (damagePackage != null && damagePackage.Length >= 3)
        {
            physDmg = damagePackage[0];
            fireDmg = damagePackage[1];
            iceDmg = damagePackage[2];

            // Hasar yüklendiği an konsola fısıldayalım asdas
            Debug.Log($"<color=green>DamageDealer:</color> Hasar Paketi Yüklendi! (Phys: {physDmg} / Fire: {fireDmg})");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. ZATEN VURDUYSAK VEYA DÜŞMAN DEĞİLSE ÇIK
        if (hasDealtDamage || !collision.CompareTag("Enemy")) 
        {
            // Ama eğer zemine çarptıysak ve mermiysek yok olalım asdas
            if (collision.CompareTag("Ground") && destroyOnHit) HandleImpact(null);
            return;
        }

        // 2. HASAR KONTROLÜ (0 Vurma Sorunu Çözümü)
        // Eğer hasar hala yüklenmediyse vurma, belki bir sonraki karede yüklenir
        if (physDmg <= 0 && fireDmg <= 0 && iceDmg <= 0)
        {
            Debug.LogWarning($"<color=yellow>Dikkat:</color> {gameObject.name} hasar yüklenmeden {collision.name} hedefine çarptı!");
            return;
        }

        // 3. DÜŞMANI YAKALA VE VUR
        EnemyStats enemy = collision.GetComponent<EnemyStats>();
        if (enemy != null)
        {
            hasDealtDamage = true; // İlk temasta mühürle!

            // Mutfaktan gelen hasarı kurda gönderiyoruz cam gibi!
            enemy.TakeHit(physDmg, fireDmg, iceDmg);

            Debug.Log($"<color=red>DARBE BAŞARILI!</color> {collision.name} hedefine {physDmg + fireDmg + iceDmg} toplam hasar aktarıldı.");
            
            HandleImpact(collision.transform);
        }
    }

    private void HandleImpact(Transform target)
    {
        // Vuruş efekti oluştur
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // Mermiyse dünyadan sil asdas
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}