using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour, IEnemyAI
{
    [Header("Veri Referansı")]
    public EnemyData data;

    [Header("Görsel Efektler & Sarsılma")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.1f;

    // Artık originalPosition değişkenine ihtiyacımız yok canım
    private Coroutine shakeCoroutine;

    [Header("Dinamik Kimlik")]
    public int level = 1;
    public enum EnemyType { Normal, Elite, Boss }
    public EnemyType type = EnemyType.Normal;

    [Header("Mevcut Durum")]
    public float currentHealth;

    private IEnemyAI brain;
    private bool isDead = false;

    [Header("Ganimet")]
    public GameObject lootBagPrefab;

    void Awake()
    {
        if (data != null) currentHealth = data.maxHealth;

        IEnemyAI[] allAIs = GetComponents<IEnemyAI>();
        foreach (var ai in allAIs)
        {
            if (ai != (IEnemyAI)this)
            {
                brain = ai;
                break;
            }
        }
    }

    public void TakeHit(float physicalDamage, float fireDamage = 0, float iceDamage = 0)
    {
        if (isDead || currentHealth <= 0) return;

        float damageReceived = Mathf.Max(physicalDamage - data.normalDefencePower, 1) +
                               Mathf.Max(fireDamage - data.fireDefencePower, 0) +
                               Mathf.Max(iceDamage - data.iceDefencePower, 0);

        currentHealth -= damageReceived;

        PlayHitVisuals();
        TakeDamage();

        if (currentHealth <= 0) Die();
    }

    private void PlayHitVisuals()
    {
        if (hitEffect != null)
        {
            hitEffect.Stop();
            hitEffect.Play();
        }

        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(Shake());
    }

    // --- IŞINLANMAYI ÖNLEYEN YENİ SARSILMA MANTIĞI ---
    private IEnumerator Shake()
    {
        float elapsed = 0f;
        Vector3 lastOffset = Vector3.zero;

        while (elapsed < shakeDuration)
        {
            // 1. Bir önceki karede eklediğimiz sarsıntı miktarını geri çekiyoruz
            // Böylece AI'nın o karedeki gerçek hareketi bozulmaz
            transform.position -= lastOffset;

            // 2. Yeni bir rastgele sarsıntı (offset) hesaplıyoruz
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            Vector3 newOffset = new Vector3(x, y, 0);

            // 3. Bu yeni sarsıntıyı kurdun o anki güncel pozisyonuna ekliyoruz
            transform.position += newOffset;

            // 4. Bu karedeki kaydırmayı kaydediyoruz ki bir sonraki karede silebilelim
            lastOffset = newOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 5. Sarsılma tamamen bittiğinde en son yapılan kaydırmayı temizleyip
        // kurdu tamamen AI'nın kontrolündeki orijinal koordinatına bırakıyoruz
        transform.position -= lastOffset;
    }

    public void TakeDamage() => brain?.TakeDamage();

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        brain?.Die();

        if (LevelManager.Instance != null)
            LevelManager.Instance.GiveExperience(this.level, this.type);

        if (lootBagPrefab != null)
        {
            GameObject bag = Instantiate(lootBagPrefab, transform.position, Quaternion.identity);
            LootBag lootScript = bag.GetComponent<LootBag>();
            if (lootScript != null) lootScript.InitializeLoot(this.level, this.type);
        }

        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 2.5f);
    }

    void IEnemyAI.Attack() => brain?.Attack();
    void IEnemyAI.Move(bool canMove) => brain?.Move(canMove);

    public float GetTotalAttackPower() => data != null ? (data.normalAttackPower + data.fireAttackPower + data.iceAttackPower) : 0;
}