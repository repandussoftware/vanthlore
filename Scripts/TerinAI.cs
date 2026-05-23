using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerinAI : MonoBehaviour
{
    [Header("Hareket Hızları")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("Zaman Ayarları")]
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("Yol Noktaları")]
    public List<Transform> waypoints;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private int currentPointIndex = -1;
    private float currentActiveSpeed;
    // TerinAI.cs içine bu değişkeni ekle:
    public bool isInteracting = false;

    void Awake()
    {
        // Animator Visuals child objesinde olduğu için:
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            StartCoroutine(TerinRoutine());
        }
    }

    IEnumerator TerinRoutine()
    {
        while (true)
        {
            // Eğer etkileşim varsa burada bekle, hiçbir şey yapma hocam
            if (isInteracting)
            {
                anim.SetInteger("state", 0); // Durma animasyonu
                yield return new WaitUntil(() => !isInteracting);
            }
            // 1. DUR VE BEKLE (IDLE - state 0)
            anim.SetInteger("state", 0);
            //Debug.Log("<color=white>Terin:</color> Dinleniyor...");

            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // 2. YENİ NOKTA SEÇ (Bir öncekinden farklı olsun canım)
            int nextPoint = currentPointIndex;
            while (nextPoint == currentPointIndex)
            {
                nextPoint = Random.Range(0, waypoints.Count);
            }
            currentPointIndex = nextPoint;
            Transform target = waypoints[currentPointIndex];

            // 3. RASTGELE YÜRÜ YA DA KOŞ (Ritmik Çeşitlilik!)
            // %50 şansla koşsun, %50 şansla yürüsün hocam
            if (Random.value > 0.5f)
            {
                anim.SetInteger("state", 2); // Run
                currentActiveSpeed = runSpeed;
                //Debug.Log("<color=yellow>Terin:</color> Acele ediyor, koşmaya başladı!");
            }
            else
            {
                anim.SetInteger("state", 1); // Walk
                currentActiveSpeed = walkSpeed;
                //Debug.Log("<color=cyan>Terin:</color> Sakin sakin yürüyor.");
            }

            // Yön ayarı (Sprite'ı hedefe doğru çevirelim)
            if (spriteRenderer != null)
                spriteRenderer.flipX = target.position.x < transform.position.x;

            // 4. HEDEFE VARANA KADAR HAREKET ET
            while (Vector2.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.position, currentActiveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}