using UnityEngine;
using System.Collections.Generic;

public class CrowRandomFly : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Konaklama Noktaları")]
    public List<Transform> perchPoints; // Sahnede karganın konabileceği boş GameObjeleri buraya at canım.

    [Header("Test Ayarları")]
    [Tooltip("-1 yaparsan rastgele başlar, 0 veya daha büyük bir sayı girersen o listedeki sıradan başlar.")]
    public int startAtPointIndex = -1; // İşte istediğin test değişkeni!

    [Header("Hareket Ayarları")]
    public float flySpeed = 6f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 6f;

    private Transform currentTarget;
    private bool isFlying = false;
    private float waitTimer;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (perchPoints.Count > 0)
        {
            // Eğer test için bir index girildiyse onu kullan, yoksa rastgele seç
            if (startAtPointIndex >= 0 && startAtPointIndex < perchPoints.Count)
            {
                currentTarget = perchPoints[startAtPointIndex];
            }
            else
            {
                currentTarget = perchPoints[Random.Range(0, perchPoints.Count)];
            }

            transform.position = currentTarget.position;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
        }
    }
    void Update()
    {
        if (isFlying)
        {
            HandleFlight();
        }
        else
        {
            HandleIdle();
        }
    }

    void HandleIdle()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0)
        {
            StartFlight();
        }
    }

    void StartFlight()
    {
        // Kendisi hariç yeni bir rastgele nokta seçelim
        Transform nextPoint = perchPoints[Random.Range(0, perchPoints.Count)];
        while (nextPoint == currentTarget && perchPoints.Count > 1)
        {
            nextPoint = perchPoints[Random.Range(0, perchPoints.Count)];
        }

        currentTarget = nextPoint;
        isFlying = true;

        // Animator'deki bool'u tetikliyoruz
        animator.SetBool("isFly", true);

        // Uçtuğu yöne göre kargayı çevirelim (Doğrultu Ayarı)
        FlipSprite();
    }

    void HandleFlight()
    {
        // Pozisyonu hedefe doğru süzüyoruz (Transform Pozisyon)
        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, flySpeed * Time.deltaTime);

        // Hedefe vardık mı?
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.05f)
        {
            Land();
        }
    }

    void Land()
    {
        isFlying = false;
        animator.SetBool("isFly", false); //
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    void FlipSprite()
    {
        // Hedef karakterin sağındaysa sağa, solundaysa sola bak (Doğrultu Kontrolü)
        if (currentTarget.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}