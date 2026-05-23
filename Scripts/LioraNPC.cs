using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LioraNPC : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 2f;
    public float waitTimeMin = 2f;
    public float waitTimeMax = 5f;

    [Header("Yol Noktaları")]
    public List<Transform> waypoints; // Hiyerarşideki Point1, 2, 3 buraya gelecek

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private int currentPointIndex = -1;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        if (waypoints.Count > 0)
        {
            StartCoroutine(PatrolRoutine());
        }
    }

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            // 1. BEKLEME AŞAMASI
            anim.SetBool("isWalking", false);
            float waitTime = Random.Range(waitTimeMin, waitTimeMax);
            yield return new WaitForSeconds(waitTime);

            // 2. YENİ NOKTA SEÇME (Bir öncekinden farklı bir yer seçsin canım)
            int nextPoint = currentPointIndex;
            while (nextPoint == currentPointIndex)
            {
                nextPoint = Random.Range(0, waypoints.Count);
            }
            currentPointIndex = nextPoint;
            Transform target = waypoints[currentPointIndex];

            // 3. YÜRÜME AŞAMASI
            anim.SetBool("isWalking", true);

            // Yönünü ayarla (Sağa mı sola mı bakacak?)
            if (spriteRenderer != null)
                spriteRenderer.flipX = target.position.x < transform.position.x;

            // Hedefe varana kadar yürü
            while (Vector2.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}