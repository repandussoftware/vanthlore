using UnityEngine;
using System.Collections;

public class BaseEnemyAI : MonoBehaviour, IEnemyAI
{
    [Header("Veri ve Bölge")]
    public EnemyData data; // ScriptableObject verisi
    public BoxCollider2D patrolArea;

    private Transform player;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private bool isDead = false;
    private bool isChasing = false;
    private bool isReturningHome = false;
    private bool canMove = true; // Interface'den gelen hareket kontrolü
    private float nextAttackTime = 0f;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Runtime'da animatörü datadan set edebiliriz
        if (data != null && data.animatorController != null)
            anim.runtimeAnimatorController = data.animatorController;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        StartCoroutine(PatrolRoutine());
    }

    void Update()
    {
        // Eğer player bir şekilde kaybolduysa veya henüz bulunamadıysa tekrar ara
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return; // Bu karede aradık, bir sonraki karede işlemlere başlarız
        }

        if (isDead || player == null || patrolArea == null || !canMove) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isOutsideArea = !patrolArea.OverlapPoint(transform.position);

        // Takip ve Sınır Kontrolü
        if (distanceToPlayer < data.chaseRange && !isReturningHome)
        {
            if (isOutsideArea && Vector2.Distance(transform.position, patrolArea.bounds.center) > patrolArea.bounds.extents.x + 2f)
            {
                StopChasingAndGoHome();
            }
            else
            {
                isChasing = true;
            }
        }
        else { isChasing = false; }

        // Savaş ve Hareket Kararı
        if (isChasing)
        {
            if (distanceToPlayer <= data.attackRange)
            {
                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + data.attackCooldown;
                }
                anim.SetBool("isWalking", false);
            }
            else { ChasePlayer(); }
        }
    }

    // --- IEnemyAI Interface Uygulamaları ---

    public void Attack()
    {
        anim.SetTrigger("isAttack");
    }

    public void TakeDamage()
    {
        if (isDead) return;
        anim.SetTrigger("isGetHit");
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        isChasing = false;
        isReturningHome = false;
        canMove = false;

        StopAllCoroutines();
        anim.SetBool("isWalking", false);
        anim.SetTrigger("isDie"); // Ölüm animasyonunu tetikle
       // PlaySound(data.deathClip);

        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<Rigidbody2D>() != null) GetComponent<Rigidbody2D>().simulated = false;
    }

    public void Move(bool canMove)
    {
        this.canMove = canMove;
        if (!canMove) anim.SetBool("isWalking", false);
    }

    // --- Yardımcı Fonksiyonlar ---

    private void ChasePlayer()
    {
        if (isDead || isReturningHome || !canMove) return;
        Vector2 target = new Vector2(player.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, target, data.chaseSpeed * Time.deltaTime);
        anim.SetBool("isWalking", true);
        spriteRenderer.flipX = player.position.x < transform.position.x;
    }

    private void StopChasingAndGoHome()
    {
        isChasing = false;
        isReturningHome = true;
        StartCoroutine(ReturnToAreaRoutine());
    }

    IEnumerator ReturnToAreaRoutine()
    {
        anim.SetBool("isWalking", true);
        Vector2 homePoint = patrolArea.bounds.center;

        while (Vector2.Distance(transform.position, homePoint) > 0.5f && !isDead)
        {
            transform.position = Vector2.MoveTowards(transform.position, homePoint, data.chaseSpeed * Time.deltaTime);
            spriteRenderer.flipX = homePoint.x < transform.position.x;
            yield return null;
        }
        isReturningHome = false;
    }

    IEnumerator PatrolRoutine()
    {
        while (!isDead)
        {
            if (isChasing || isReturningHome || !canMove)
            {
                yield return new WaitUntil(() => !isChasing && !isReturningHome && canMove);
            }

            float randomX = Random.Range(patrolArea.bounds.min.x, patrolArea.bounds.max.x);
            Vector2 targetPosition = new Vector2(randomX, transform.position.y);

            anim.SetBool("isWalking", true);
            spriteRenderer.flipX = targetPosition.x < transform.position.x;

            while (Vector2.Distance(transform.position, targetPosition) > 0.1f && !isDead && !isChasing && !isReturningHome && canMove)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, data.moveSpeed * Time.deltaTime);
                yield return null;
            }

            anim.SetBool("isWalking", false);
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }

    // Animation Event: Tam ısırma/pençe anında çalışır
    // Animation Event'lerin görmesi için 'public' kalmalı canım
    public void TriggerDamageEvent()
    {
        if (isDead) return;

        // 1. Data Kontrolü
        if (data == null)
        {
            Debug.LogError("Düşman datası atanmamış!");
            return;
        }

        // 2. Player Kontrolü
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            else return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= data.attackImpactRange)
        {
            // 3. Singleton Kontrolü (Hatanın kaynağı burası olabilir)
            if (CombatFormulaManager.Instance != null)
            {
                CombatFormulaManager.Instance.CalculatePlayerTokenDamage(data.normalAttackPower,0,0, transform.position);
                Debug.Log($"{data.enemyName} oyuncuya vurdu!");
            }
            else
            {
                Debug.LogError("Sahnede CombatFormulaManager bulunamadı! Hiyerarşiyi kontrol et canım.");
            }
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip);
        }
    }
}