using UnityEngine;
using System.Collections.Generic;

public enum MovementMode { Walk, Fly, Climb }

public class UniversalPetAI : MonoBehaviour
{
    private Animator animator;

    [Header("Davranış Modu (DB'den Ezilebilir)")]
    [Tooltip("Eğer veritabanından tikli gelirse hayvan asla yürümez, doğduğu noktada sabit kalır.")]
    public bool isStaticPet = false; 
    public MovementMode moveMode = MovementMode.Walk;

    [Header("Hız ve Bekleme Ayarları (DB'den Ezilebilir)")]
    public float movementSpeed = 4f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 6f;

    [Header("Animator Parametre İsimleri (DB'den Ezilebilir)")]
    public string movingBoolName = "isWalking"; 
    [Tooltip("Animator'da hangi idle varyasyonunun oynayacağını seçen Int parametresinin adı.")]
    public string idleStateIntName = "idleType"; // Helen'in uyku/uyanma durumları için harika bir kontrol!

    // DB'den dinamik dolacak iç listeler canım
    private List<Vector3> possiblePositions = new List<Vector3>();
    private List<int> pointIdleTypes = new List<int>(); // Her noktanın kendine has idle/uyku kodu

    private Vector3 currentTargetPos;
    private bool isMoving = false;
    private float waitTimer;
    private int currentPointIndex = -1;
    private bool isInitialized = false;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// EFSANEVİ BAŞLATICI: PetSpawnManager tarafından çağrılır ve veritabanındaki tüm canlı 
    /// parametreleri (hız, süre, statiklik, koordinat) bu yapay zekaya enjekte eder canım.
    /// </summary>
    public void InitializePetFromDatabase(
        List<Vector3> pointsFromDB, 
        List<int> idleTypesFromDB,
        bool dbIsStatic,
        float dbSpeed,
        float dbMinWait,
        float dbMaxWait,
        string dbAnimBoolName)
    {
        // 1. AYARLARI VERİTABANINDAN GELEN VERİLERLE EZİYORUZ
        this.isStaticPet = dbIsStatic;
        this.movementSpeed = dbSpeed;
        this.minWaitTime = dbMinWait;
        this.maxWaitTime = dbMaxWait;
        this.movingBoolName = dbAnimBoolName;

        // 2. NOKTALARI VE O NOKTALARIN IDLE TİPLERİNİ AKILLICA EŞLEŞTİR
        this.possiblePositions = pointsFromDB;
        
        if (idleTypesFromDB != null && idleTypesFromDB.Count == possiblePositions.Count)
        {
            this.pointIdleTypes = idleTypesFromDB;
        }
        else
        {
            // Eğer DB'den idle tipleri eksik veya boş gelirse sistemi çökertmiyoruz, hepsini varsayılan 0 yapıyoruz canım
            this.pointIdleTypes = new List<int>(new int[possiblePositions.Count]);
        }

        if (possiblePositions == null || possiblePositions.Count == 0) return;

        // 3. İLK UYANMA NOKTASINI SEÇ VE IŞINLA
        currentPointIndex = Random.Range(0, possiblePositions.Count);
        currentTargetPos = possiblePositions[currentPointIndex];
        transform.position = currentTargetPos;

        // 4. İLK DOĞDUĞU NOKTANIN IDLE ANIMASYONUNU TETİKLE (Örn: Helen direkt yatakta uyuyarak başlasın)
        ApplyLocationSpecificIdle();

        ResetWaitTimer();
        isInitialized = true;

        Debug.Log($"<color=cyan>{gameObject.name}</color> DB ile Kusursuz Senkronize Oldu! Static mi: {isStaticPet} | Hız: {movementSpeed}");
    }

    void Update()
    {
        // GÜVENLİK DUVARI: DB verileri henüz yüklenmediyse, hayvan sabitse (Static) veya gezecek noktası yoksa hareket etme!
        if (!isInitialized || isStaticPet || possiblePositions == null || possiblePositions.Count <= 1) return;

        if (isMoving) 
            HandleMovement();
        else 
            HandleIdle();
    }

    private void HandleIdle()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0) StartNewJourney();
    }

    private void StartNewJourney()
    {
        int nextIndex = Random.Range(0, possiblePositions.Count);
        while (nextIndex == currentPointIndex && possiblePositions.Count > 1)
        {
            nextIndex = Random.Range(0, possiblePositions.Count);
        }

        currentPointIndex = nextIndex;
        currentTargetPos = possiblePositions[currentPointIndex];
        isMoving = true;

        // Yürüme/Uçma animasyon bool'unu aktif et canım
        if (animator != null && !string.IsNullOrEmpty(movingBoolName))
        {
            animator.SetBool(movingBoolName, true);
        }

        FlipSprite(currentTargetPos.x > transform.position.x);
    }

    private void HandleMovement()
    {
        transform.position = Vector2.MoveTowards(transform.position, currentTargetPos, movementSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentTargetPos) < 0.05f)
        {
            ArrivedAtPoint();
        }
    }

    private void ArrivedAtPoint()
    {
        isMoving = false;

        // Hareket animasyonunu kapat
        if (animator != null && !string.IsNullOrEmpty(movingBoolName))
        {
            animator.SetBool(movingBoolName, false);
        }

        // Vardığımız yeni noktanın kendine has Idle animasyonunu (Oturma, uyuma vb.) tetikliyoruz canım
        ApplyLocationSpecificIdle();

        ResetWaitTimer();
    }

    /// <summary>
    /// Bulunduğu noktanın indexine göre Animator'daki 'idleType' parametresini günceller canım.
    /// </summary>
    private void ApplyLocationSpecificIdle()
    {
        if (animator == null || string.IsNullOrEmpty(idleStateIntName)) return;

        int currentIdleType = 0;
        if (pointIdleTypes.Count > currentPointIndex)
        {
            currentIdleType = pointIdleTypes[currentPointIndex];
        }

        // Animator'daki Int parametresini ezerek yeni sarsılmaz duruşunu veriyoruz
        animator.SetInteger(idleStateIntName, currentIdleType);
    }

    private void ResetWaitTimer()
    {
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    private void FlipSprite(bool lookRight)
    {
        Vector3 localScale = transform.localScale;
        // Sağa giderken X scale değerini ters çeviriyoruz (Tünek ve yön doğrulaması için)
        localScale.x = lookRight ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }
}