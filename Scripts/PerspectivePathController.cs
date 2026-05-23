using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;



public class TriggerPerspectiveController : MonoBehaviour
{
     /*
 [Header("Yollar (Collider Objeleri)")]
    public GameObject downHallCollider;
    public GameObject downHall;
    public GameObject downStairsCollider;
    public GameObject downStairs;

    [Header("Tetikleyici Alan (Trigger)")]
    public Collider2D transitionZone;

    [Header("Karakter Ayarları")]
    public Vector3 normalScale = new Vector3(0.65f, 0.65f, 0.65f);
    public Vector3 largeScale = new Vector3(0.75f, 0.75f, 0.75f);
    public float scalingDuration = 0.5f;

    [Header("Fizik ve Hız Ayarları (Dinamik)")]
    public float hallSpeed = 5f;
    public float stairsSpeedRight = 4f;
    public float stairsSpeedLeft = 20f;
    public float stairsGravityRight = 20f;
    public float stairsGravityLeft = 1f;

    [Header("Trabzan Ayarları")]
    public GameObject stairRailings;

    [Header("Scale Points Ayarları")]
    public GameObject[] scaleObjects;
    public float scaleAmount = 0.07f;

    private Transform playerTransform;
    private Rigidbody2D playerRb;
    private DarionController controller;
    [HideInInspector] public float currentSpeed;

    private int currentScaleIndex = -1;
    private Vector3 currentTargetScale;
    private bool isOnStairs = false;
    private bool isInsideZone = false;
    private bool isTouchingHallPath = false;
    private bool isTouchingStairsPath = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody2D>();
            controller = playerObj.GetComponent<DarionController>();
        }
        currentSpeed = hallSpeed;
        currentTargetScale = normalScale;
        SwitchToHall(true);
    }

    void Update()
    {
        if (playerTransform == null) return;

        CheckPathContact();
        HandlePhysicsAndSpeed();
        CheckScalePoints();

        // JOYSTICK YÖN KONTROLÜ
        if (isInsideZone && controller != null)
        {
            Vector2 stick = controller.stickInput;

            // GÜNEY-DOĞU (South-East): Merdivene in
            if (stick.x > 0.3f && stick.y < -0.3f)
            {
                if (!isOnStairs) SwitchToStairs();
            }
            // SADECE DOĞU (East/Right): Koridorda kal
            else if (stick.x > 0.4f && Mathf.Abs(stick.y) < 0.3f)
            {
                if (isOnStairs) SwitchToHall();
            }
        }
    }

    // --- DİĞER FONKSİYONLAR (Scaling ve Fizik) ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.IsTouching(transitionZone))
        {
            isInsideZone = true;
            StartScaling(normalScale);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isInsideZone = false;
    }

    void CheckScalePoints()
    {
        if (!isOnStairs || controller == null || playerTransform == null) return;
        Collider2D playerCol = playerTransform.GetComponent<Collider2D>();
        int detectedIndex = -1;

        for (int i = 0; i < scaleObjects.Length; i++)
        {
            Collider2D pointCol = scaleObjects[i].GetComponent<Collider2D>();
            if (pointCol != null && playerCol.IsTouching(pointCol)) { detectedIndex = i; break; }
        }

        if (detectedIndex != -1 && detectedIndex != currentScaleIndex)
        {
            currentScaleIndex = detectedIndex;
            CalculateNewScale();
        }
    }

    void CalculateNewScale()
    {
        float multiplier = 1f + ((currentScaleIndex + 1) * scaleAmount);
        Vector3 target = new Vector3(normalScale.x * multiplier, normalScale.y * multiplier, normalScale.z);
        if (target != currentTargetScale) { currentTargetScale = target; StartScaling(currentTargetScale); }
    }

    void CheckPathContact()
    {
        if (playerTransform == null) return;
        BoxCollider2D hallCol = downHallCollider.GetComponent<BoxCollider2D>();
        BoxCollider2D stairsCol = downStairsCollider.GetComponent<BoxCollider2D>();
        Collider2D playerCol = playerTransform.GetComponent<Collider2D>();

        bool touchingStairsNow = (stairsCol != null && playerCol != null) && playerCol.IsTouching(stairsCol);
        if (touchingStairsNow && !isTouchingStairsPath) isTouchingStairsPath = true;
        else if (!touchingStairsNow && isTouchingStairsPath) { isTouchingStairsPath = false; OnPlayerExitStairsPath(); }

        bool touchingHallNow = (hallCol != null && playerCol != null) && playerCol.IsTouching(hallCol);
        if (touchingHallNow && !isTouchingHallPath) isTouchingHallPath = true;
        else if (!touchingHallNow && isTouchingHallPath) isTouchingHallPath = false;
    }

    void OnPlayerExitStairsPath()
    {
        if (controller != null)
        {
            // 1. Kontrolü tekrar joystick'e devrediyoruz
            controller.isOnSpecialPath = false;

            // 2. Değişken ismini güncelliyoruz (moveSpeed -> currentActiveSpeed)
            controller.currentActiveSpeed = hallSpeed;
        }

        if (playerRb != null)
        {
            // 3. Yerçekimini varsayılan değerine çekiyoruz
            playerRb.gravityScale = 5f;
        }
    }

    void HandlePhysicsAndSpeed()
    {
        if (playerRb == null || controller == null) return;

        if (isOnStairs && isTouchingStairsPath)
        {
            // 1. Darion'a "Hızı ben kontrol ediyorum" diyoruz
            controller.isOnSpecialPath = true;

            // 2. Merdiven hızını ve yerçekimini ayarla
            float stairsSpeed = controller.isFacingRight ? stairsSpeedRight : stairsSpeedLeft;
            playerRb.gravityScale = controller.isFacingRight ? stairsGravityRight : stairsGravityLeft;

            // 3. Yeni değişken ismini (currentActiveSpeed) kullanıyoruz
            controller.currentActiveSpeed = stairsSpeed;
        }
        else
        {
            // Merdiven bittiğinde kontrolü joystick'e geri veriyoruz
            controller.isOnSpecialPath = false;
            playerRb.gravityScale = 5f; // Normal yerçekimine dön
        }
    }

    void SwitchToStairs()
    {
        isOnStairs = true;
        downHall.SetActive(false);
        downStairs.SetActive(true);
        StartScaling(largeScale);

        // --- GÜNCELLEME: Merdivendeyken Katman 6 'Default' olsun ---
        UpdateSortingLayer("Default");
    }

    void SwitchToHall(bool instant = false)
    {
        isOnStairs = false;
        downHall.SetActive(true);
        downStairs.SetActive(false);
        if (instant && playerTransform != null) playerTransform.localScale = normalScale;
        else StartScaling(normalScale);

        // --- GÜNCELLEME: Koridordayken Katman 6 'Trabzan' olsun ---
        UpdateSortingLayer("Trabzan");
    }

    void UpdateSortingLayer(string layerName)
    {
        // Inspector'daki 'stairRailings' kutucuğuna 'Katman 6'yı sürüklediğinden emin ol canım
        if (stairRailings != null)
        {
            var sr = stairRailings.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = layerName;
                Debug.Log($"<color=cyan>Sorting Layer Değişti:</color> {layerName}");
            }
        }
    }

    void StartScaling(Vector3 target)
    {
        if (playerTransform == null) return;
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(target));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Vector3 startScale = playerTransform.localScale;
        float t = 0;
        while (t < scalingDuration)
        {
            t += Time.deltaTime;
            float progress = t / scalingDuration;
            float currentDirection = (controller != null && !controller.isFacingRight) ? -1f : 1f;
            float lerpedX = Mathf.Lerp(Mathf.Abs(startScale.x), Mathf.Abs(targetScale.x), progress);
            float lerpedY = Mathf.Lerp(startScale.y, targetScale.y, progress);
            playerTransform.localScale = new Vector3(lerpedX * currentDirection, lerpedY, targetScale.z);
            yield return null;
        }
    }
     */
   
}