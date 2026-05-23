using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RonneAI : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 1.2f;      // Ronne biraz ağırbaşlı bir fırıncı olduğu için yavaş yürüsün canım
    public float waitTimeMin = 3f;
    public float waitTimeMax = 7f;

    [Header("Yol Noktaları")]
    public List<Transform> waypoints; // Point1, Point2...

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private int currentPointIndex = -1;

    void Awake()
    {
        // Ronne'nin Animator'ı child objede (Visuals) olduğu için:
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            StartCoroutine(RonneRoutine());
        }
    }

    IEnumerator RonneRoutine()
    {
        while (true)
        {
            // 1. DURAKLAMA VE BEKLEME
            anim.SetBool("isWalking", false);
            float waitTime = Random.Range(waitTimeMin, waitTimeMax);
            yield return new WaitForSeconds(waitTime);

            // 2. YENİ BİR TEZGAH/NOKTA SEÇME
            int nextPoint = currentPointIndex;
            while (nextPoint == currentPointIndex)
            {
                nextPoint = Random.Range(0, waypoints.Count);
            }
            currentPointIndex = nextPoint;
            Transform target = waypoints[currentPointIndex];

            // 3. YÜRÜME
            anim.SetBool("isWalking", true);

            // Ronne hedefine göre sağa/sola baksın canım
            if (spriteRenderer != null)
                spriteRenderer.flipX = target.position.x < transform.position.x;

            while (Vector2.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}