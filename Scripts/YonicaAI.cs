using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class YonicaAI : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 1.5f;      // Yonica için standart bir yürüme hızı
    public float waitTimeMin = 2f;
    public float waitTimeMax = 5f;

    [Header("Yol Noktaları")]
    public List<Transform> waypoints;   // Gideceği noktalar (Point1, Point2 vb.)

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
            StartCoroutine(YonicaPatrolRoutine());
        }
    }

    IEnumerator YonicaPatrolRoutine()
    {
        while (true)
        {
            // 1. DURAKLAMA (IDLE)
            anim.SetBool("isWalking", false);
            //Debug.Log("<color=pink>Yonica:</color> Çiçeklerini düzeltiyor...");
            
            float waitTime = Random.Range(waitTimeMin, waitTimeMax);
            yield return new WaitForSeconds(waitTime);

            // 2. YENİ HEDEF SEÇİMİ
            int nextPoint = currentPointIndex;
            while (nextPoint == currentPointIndex)
            {
                nextPoint = Random.Range(0, waypoints.Count);
            }
            currentPointIndex = nextPoint;
            Transform target = waypoints[currentPointIndex];

            // 3. YÜRÜME BAŞLASIN
            anim.SetBool("isWalking", true);

            // Yön ayarı (Müzisyenin ritmi gibi, sprite sola mı baksın sağa mı?)
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