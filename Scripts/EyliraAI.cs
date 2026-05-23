using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EyliraAI : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 1.5f;
    public float waitTimeMin = 2f;
    public float waitTimeMax = 4f;

    [Header("Yol Noktaları")]
    public List<Transform> waypoints; // Point1, Point2, Point3...
    // Bu indeksteki noktada 'isSearching' animasyonu çalışacak hocam
    public int searchPointIndex = 1; 
    public float searchDuration = 5f;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private int currentPointIndex = -1;

    void Awake()
    {
        // Animator Visuals child objesinde olduğu için GetComponentInChildren kullanıyoruz
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        if (waypoints.Count > 0)
        {
            StartCoroutine(EyliraRoutine());
        }
    }

    IEnumerator EyliraRoutine()
    {
        while (true)
        {
            // 1. YENİ NOKTA SEÇİMİ (Rastgele bir noktaya gitsin canım)
            int nextPoint = currentPointIndex;
            while (nextPoint == currentPointIndex)
            {
                nextPoint = Random.Range(0, waypoints.Count);
            }
            currentPointIndex = nextPoint;
            Transform target = waypoints[currentPointIndex];

            // 2. YÜRÜME AŞAMASI
            anim.SetBool("isWalking", true);
            anim.SetBool("isSearching", false);

            // Yön ayarı (Müzisyenin ritmi gibi, sprite sola mı baksın sağa mı?)
            if (spriteRenderer != null)
                spriteRenderer.flipX = target.position.x < transform.position.x;

            while (Vector2.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // 3. HEDEFE VARDIĞINDA NE YAPACAK?
            anim.SetBool("isWalking", false);

            // Eğer bu nokta bizim 'İksir Arama' noktamız ise:
            if (currentPointIndex == searchPointIndex)
            {
                //Debug.Log("<color=purple>Eylira:</color> İksirleri kontrol ediyor...");
                anim.SetBool("isSearching", true);
                yield return new WaitForSeconds(searchDuration);
                anim.SetBool("isSearching", false);
            }
            else
            {
                // Normal bekleme
                float waitTime = Random.Range(waitTimeMin, waitTimeMax);
                yield return new WaitForSeconds(waitTime);
            }
        }
    }
}