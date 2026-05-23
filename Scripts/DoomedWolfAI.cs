using UnityEngine;
using System.Collections;

public class DoomedWolfAI : MonoBehaviour, IEnemyAI
{
    [Header("Bölge Ayarları")]
    public BoxCollider2D patrolArea;
    private bool isReturningHome = false;

    // Spawner'dan bölgeyi set etmek için bu fonksiyonu kullanacağız canım
    public void SetPatrolArea(BoxCollider2D area)
    {
        patrolArea = area;
    }

    [Header("Hareket Ayarları")]
    public float moveSpeed = 2f;
    public float patrolRange = 5f;
    public float waitTimeMin = 1f;
    public float waitTimeMax = 3f;

    [Header("Takip & Savaş Ayarları")]
    public float chaseRange = 5f;
    public float chaseSpeed = 3.5f;
    private Transform player;

    [Header("Saldırı Zamanlaması")]
    public float attackImpactRange = 1.5f;

    [Header("Saldırı Ayarları")]
    public float attackRange = 1.3f;
    public float attackCooldown = 1.5f;
    private float nextAttackTime = 0f;

    [Header("Ses Ayarları")]
    public AudioSource idleSource;
    public AudioSource attackSource;
    public AudioClip attackClip;
    public AudioClip hitClip; // YENİ: Darbe (inleme) sesi buraya gelecek

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private bool isChasing = false;

    void Start()
    {
        startPosition = transform.position;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        StartCoroutine(PatrolRoutine());
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (isDead || player == null || patrolArea == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 1. SINIR KONTROLÜ
        bool isOutsideArea = !patrolArea.OverlapPoint(transform.position);

        // 2. TAKİP MANTIĞI
        if (distanceToPlayer < chaseRange && !isReturningHome)
        {
            // Sınırın çok dışına çıkarsa (extents x değerine 2 birim tolerans ekledik)
            if (isOutsideArea && Vector2.Distance(transform.position, patrolArea.bounds.center) > patrolArea.bounds.extents.x + 2f)
            {
                StopChasingAndGoHome();
            }
            else
            {
                isChasing = true;
            }
        }
        else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            if (distanceToPlayer <= attackRange)
            {
                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + attackCooldown;
                }
                anim.SetBool("isWalking", false);
            }
            else
            {
                ChasePlayer();
            }
        }
    }

    void StopChasingAndGoHome()
    {
        isChasing = false;
        isReturningHome = true;
        StartCoroutine(ReturnToAreaRoutine());
    }

    IEnumerator ReturnToAreaRoutine()
    {
        anim.SetBool("isWalking", true);
        Vector2 homePoint = patrolArea.bounds.center;

        while (Vector2.Distance(transform.position, homePoint) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, homePoint, chaseSpeed * Time.deltaTime);
            spriteRenderer.flipX = homePoint.x < transform.position.x;
            yield return null;
        }

        isReturningHome = false;
    }

    void ChasePlayer()
    {
        if (isDead || isReturningHome) return;
        Vector2 target = new Vector2(player.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, target, chaseSpeed * Time.deltaTime);
        anim.SetBool("isWalking", true);
        spriteRenderer.flipX = player.position.x < transform.position.x;
    }

    IEnumerator PatrolRoutine()
    {
        while (!isDead)
        {
            if (isChasing || isReturningHome)
            {
                yield return new WaitUntil(() => !isChasing && !isReturningHome);
            }

            // Bölge içinde rastgele X seçer
            float randomX = Random.Range(patrolArea.bounds.min.x, patrolArea.bounds.max.x);
            targetPosition = new Vector2(randomX, transform.position.y);

            anim.SetBool("isWalking", true);
            spriteRenderer.flipX = targetPosition.x < transform.position.x;

            while (Vector2.Distance(transform.position, targetPosition) > 0.1f && !isDead && !isChasing && !isReturningHome)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            anim.SetBool("isWalking", false);
            if (!isDead && !isChasing && !isReturningHome)
            {
                yield return new WaitForSeconds(Random.Range(waitTimeMin, waitTimeMax));
            }
        }
    }
    public void TriggerDamageEvent()
    {
        Debug.Log("Triggerlandı");
        if (isDead || player == null) return;

        Debug.Log("Koşulu Geçti");

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackImpactRange)
        {
            Debug.Log("Distance Koşulu Geçti");


            Debug.Log("Take Damageye Geldi");
            CombatFormulaManager.Instance.CalculatePlayerTokenDamage(this.GetComponent<EnemyStats>().data.normalAttackPower, 0,0, transform.position);

        }
    }
    public void Attack() => anim.SetTrigger("isAttack");
    public void TakeDamage()
    {
        if (anim != null)
        {
            anim.SetTrigger("isGetHit");
        }
        else
        {
            anim = GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("isGetHit");
        }
    }
    public void Die()
    {
        isDead = true;
        isChasing = false;
        isReturningHome = false;
        StopAllCoroutines();
        anim.SetBool("isWalking", false);
        anim.SetBool("isDie", true);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
    }

    public void PlayAttackSound()
    {
        if (attackSource != null && attackClip != null)
        {
            // Ritmik doğallık için pitch randomize edelim hocam
            attackSource.pitch = Random.Range(0.9f, 1.2f);
            attackSource.PlayOneShot(attackClip);
        }
    }

    public void PlayHitSound()
    {
        if (attackSource != null && hitClip != null)
        {
            // Her ciyaklama aynı olmasın, kulağı yormasın canım
            attackSource.pitch = Random.Range(0.85f, 1.15f);
            attackSource.PlayOneShot(hitClip);
            Debug.Log("<color=red>Kurt:</color> Darbe sesi çalındı!");
        }
    }
    public void Move(bool canMove) { }
}