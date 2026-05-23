using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using static VanthLoreSceneManager; // 👑 Kutsal veri köprüsü tam gaz aktif!

public class DarionController : MonoBehaviour
{
    public static DarionController Instance;

    private string currentAnimState;

    [Header("--- GROUND & PHYSICS CONFIG ---")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isHitActive = false;
    public string hitAnimName = "Darion_Sende_Anim_V1";
    public string hitBackAnimName = "Darion_GetHitBack_Anima";
    private bool canJump = true;
    private bool jumpTriggered = false;

    [Header("--- COMPONENTS ---")]
    public Rigidbody2D rb;
    public Animator animator;
    public Transform visualsContainer;
    public GameObject weaponSlotContainer;

    [Header("--- RUNTIME STATS (SERVER SIDE DRIVEN) ---")]
    public bool isInitialized = false;
    public bool isDead = false;
    public bool isOnSpecialPath = false;

    // 🛡️ ANTI-CHEAT: Değerler tamamen bu nesnenin RAM yatağında izole korunuyor canım benim
    private float currentHealth;
    private float maxHealth;
    private float currentMana;
    private float maxMana;
    private float currentWalkSpeed;
    private float currentRunSpeed;
    private float activeMovementSpeed; // StatsManager bağımlılığını kökten koparan o şanlı değişken 🎯

    [HideInInspector] public Vector2 stickInput;
    private Vector2 moveInput;
    private bool isClicking;
    private float _serverJumpForce = 6.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.actions.Disable();
                playerInput.actions.FindActionMap("Player").Enable();
            }
        }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // 🎯 KUTSAL ENJEKSİYON 1: Sunucudan profil verisi inince damarlar mühürleniyor
    public void InitializePlayerStats(VanthLorePlayerProfileDTO profile)
    {
        maxHealth = profile.max_health;
        currentHealth = profile.current_health;
        maxMana = profile.max_mana;
        currentMana = profile.current_mana;

        if (weaponSlotContainer != null) weaponSlotContainer.SetActive(profile.is_armed);

        UpdateNetworkUI();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateExperienceUI(profile.current_level, profile.current_exp, profile.max_exp);
        }

        isInitialized = true;
        Debug.Log($"<color=lime>[VanthLore Live-Ops]</color> Darion statları başarıyla PostgreSQL verisiyle eşitlendi canım.");
    }

    // 🎯 KUTSAL ENJEKSİYON 2: Yeni odaya girildiğinde harita fizikleri buluttan basılıyor
    public void ApplyServerPhysics(VanthLoreRoomPhysicsDTO physics)
    {
        transform.localPosition = new Vector3(physics.spawn_x, physics.spawn_y, 0f);
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = new Vector3(physics.forced_scale, physics.forced_scale, physics.forced_scale);

        currentWalkSpeed = physics.walk_speed;
        currentRunSpeed = physics.run_speed;
        _serverJumpForce = physics.jump_force;

        if (rb != null)
        {
            rb.gravityScale = physics.gravity_scale;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void UpdateNetworkUI()
    {
        // Darion kendi güvenli yerel RAM yatağındaki saf statları UIManager'a üflüyor ⚡
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHUD(currentHealth, maxHealth, currentMana, maxMana);
        }
    }
    void Update()
    {
        if (!isInitialized || isDead) return;

        float inputMag = stickInput.magnitude;
        StatsManager.Instance.isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (stickInput.y < 0.3f && StatsManager.Instance.isGrounded)
        {
            jumpTriggered = false;
            canJump = true;
        }

        if (stickInput.y > 0.45f && StatsManager.Instance.isGrounded && canJump)
        {
            Jump();
            canJump = false;
        }

        // 🎯 TEMİZLİK ŞOVU: StatsManager hız bağımlılığı puf diye uçtu, saf yerel hesaplama geldi!
        if (inputMag > 0.05f)
        {
            moveInput.x = stickInput.x;
            if (!isOnSpecialPath) activeMovementSpeed = (inputMag >= 0.7f) ? currentRunSpeed : currentWalkSpeed;
        }
        else if (isClicking && !IsPointerOverUI()) { HandlePointerMovement(); activeMovementSpeed = currentWalkSpeed; }
        else { moveInput.x = 0; activeMovementSpeed = 0; }

        UpdateCharacterVisuals();
    }

    void FixedUpdate()
    {
        if (!isInitialized || isDead) return;
        rb.linearVelocity = new Vector2(moveInput.x * activeMovementSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        jumpTriggered = true;
        animator.SetBool("isJumping", true);
    }

    public void ApplyPhysicalJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, _serverJumpForce);
        jumpTriggered = false;
    }

    public void RestoreHealth(float amount)
    {
        if (currentHealth >= maxHealth) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateNetworkUI();
    }

    public void RestoreMana(float amount)
    {
        if (currentMana >= maxMana) return;
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        UpdateNetworkUI();
    }

    public void PlayHitAnimation(Vector3 attackerPos)
    {
        if (isHitActive) return;

        float directionToAttacker = attackerPos.x - transform.position.x;
        bool isFacingRight = StatsManager.Instance.isFacingRight;
        string finalHitAnim = hitAnimName;

        bool hitFromFront = (directionToAttacker > 0 && isFacingRight) || (directionToAttacker < 0 && !isFacingRight);
        if (!hitFromFront) finalHitAnim = hitBackAnimName;

        if (SkillBarManager.Instance != null) SkillBarManager.Instance.CancelPendingSkill();
        StartCoroutine(HitStunRoutine(finalHitAnim));
    }

    IEnumerator HitStunRoutine(string animName)
    {
        isHitActive = true;
        currentAnimState = animName;
        animator.CrossFade(animName, 0f, 0);
        yield return new WaitForSeconds(0.5f);
        isHitActive = false;
    }

    public void Die()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;
    }

    public void OnMove(InputValue value) => stickInput = value.Get<Vector2>();
    public void OnClick(InputValue value) => isClicking = value.isPressed;

    void LateUpdate()
    {
        if (visualsContainer == null) visualsContainer = transform.Find("Visuals");
        if (visualsContainer != null)
        {
            visualsContainer.localPosition = Vector3.zero;
            if (!isOnSpecialPath)
            {
                visualsContainer.localScale = new Vector3(transform.localScale.x, Mathf.Abs(transform.localScale.y), transform.localScale.z);
            }
        }
    }

    void UpdateCharacterVisuals()
    {
        if (isHitActive) return;

        float currentPhysX = Mathf.Abs(rb.linearVelocity.x);
        bool hasHorizontalInput = Mathf.Abs(moveInput.x) > 0.1f;
        bool isCrouching = stickInput.y < -0.5f && StatsManager.Instance.isGrounded && !hasHorizontalInput;

        bool isWalking = false;
        bool isRunning = false;
        string newState = "";

        if (!StatsManager.Instance.isGrounded) newState = "Darion_Jump_Hang_Anima_V1";
        else if (hasHorizontalInput || currentPhysX > 0.1f)
        {
            if (currentPhysX >= currentRunSpeed * 0.7f) { newState = "Darion_Run_V1"; isRunning = true; }
            else { newState = "Darion_Walk_2_V1"; isWalking = true; }
        }
        else if (isCrouching) newState = "Darion_Crouch_Anima";
        else newState = "Darion_Idle_V1";

        if (newState != currentAnimState)
        {
            currentAnimState = newState;
            animator.CrossFade(newState, 0.1f);
        }

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isGrounded", StatsManager.Instance.isGrounded);
        animator.SetBool("isJumping", (stickInput.y > 0.5f && jumpTriggered) || !StatsManager.Instance.isGrounded);

        animator.SetBool("isArmed", weaponSlotContainer != null && weaponSlotContainer.activeSelf);
        animator.SetBool("isHelmetEquipped", StatsManager.Instance.isHelmetEquipped);
        animator.SetBool("isBootEquipped", StatsManager.Instance.isBootEquipped);
        animator.SetBool("isGauntletEquipped", StatsManager.Instance.isGauntletEquipped);
        animator.SetBool("isPadsEquipped", StatsManager.Instance.isPadEquipped);
        animator.SetBool("isPauldronEquipped", StatsManager.Instance.isPauldronEquipped);

        if (hasHorizontalInput)
        {
            StatsManager.Instance.isFacingRight = moveInput.x > 0;
            Vector3 localScale = transform.localScale;
            localScale.x = StatsManager.Instance.isFacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }
    }

    private void HandlePointerMovement()
    {
        Vector2 pointerPos = Pointer.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(pointerPos);
        float diffX = worldPos.x - transform.position.x;
        if (Mathf.Abs(diffX) > StatsManager.Instance.stopDistance) moveInput.x = diffX > 0 ? 1 : -1;
        else moveInput.x = 0;
    }

    private bool IsPointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}