using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TyurinAI : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 0.8f;      // Tyurin yaşlı olduğu için yavaş ve vakur yürüsün
    public float waitTimeMin = 4f;      // Daha uzun dinlenme süreleri
    public float waitTimeMax = 8f;

    [Header("Yol Noktaları")]
    public List<Transform> waypoints;   // Gideceği duraklar (Point1, Point2 vb.)

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private int currentPointIndex = -1;

    void Awake()
    {
        // Animator ve SpriteRenderer 'Visuals' alt objesinde olduğu için:
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            StartCoroutine(TyurinPatrolRoutine());
        }
    }

    IEnumerator TyurinPatrolRoutine()
    {
        while (true)
        {
            // 1. BEKLEME (IDLE)
            anim.SetBool("isWalking", false);
            //Debug.Log("<color=white>Tyurin:</color> Biraz soluklanıyor...");
            
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

            // Tyurin hedefe göre sağa mı sola mı baksın?
            if (spriteRenderer != null)
                spriteRenderer.flipX = target.position.x < transform.position.x;

            // Hedefe varana kadar yavaşça yürü
            while (Vector2.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}