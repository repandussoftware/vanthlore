using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LesseAI : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 1.0f;      // Lesse biraz yavaş ve bitkin görünüyor, hızı düşük tuttum
    public float waitTimeMin = 3f;
    public float waitTimeMax = 6f;

    [Header("Yol Noktaları")]
    public List<Transform> waypoints;   // Lesse'nin gideceği duraklar

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private int currentPointIndex = -1;

    void Awake()
    {
        // Bileşenler 'Visuals' altında olduğu için:
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            StartCoroutine(LessePatrolRoutine());
        }
    }

    IEnumerator LessePatrolRoutine()
    {
        while (true)
        {
            // 1. DİNLENME (IDLE)
            anim.SetBool("isWalking", false);
            
            float waitTime = Random.Range(waitTimeMin, waitTimeMax);
            yield return new WaitForSeconds(waitTime);

            // 2. YENİ NOKTA SEÇİMİ
            int nextPoint = currentPointIndex;
            while (nextPoint == currentPointIndex)
            {
                nextPoint = Random.Range(0, waypoints.Count);
            }
            currentPointIndex = nextPoint;
            Transform target = waypoints[currentPointIndex];

            // 3. YÜRÜME BAŞLASIN
            anim.SetBool("isWalking", true);

            // Sprite yönünü hedefe göre çevirelim hocam
            if (spriteRenderer != null)
                spriteRenderer.flipX = target.position.x < transform.position.x;

            // Hedefe varana kadar ilerle
            while (Vector2.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}