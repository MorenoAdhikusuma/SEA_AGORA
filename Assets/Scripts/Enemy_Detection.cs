using UnityEngine;

public class Enemy_Detection : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int health = 5;

    [Header("Detection & Chase")]
    public Transform player;
    public float detectionRadius = 8f;
    public float stopDistance = 1.5f;
    public float chaseSpeed = 4f;

    [Header("Idle Behavior")]
    public bool returnToSpawn = true;
    public float idleSpeed = 1.5f;

    [Header("Combat")]
    public int damageToPlayer = 1;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float attackAnimationDuration = 0.6f;

    [Header("Attack Hitbox")]
    public Transform attackPoint; // Point where attack originates from
    public Vector2 attackPointOffset = new Vector2(1f, 0f); // Offset from enemy center (X will flip)
    public float attackHitboxRadius = 1f; // Radius of the attack hitbox
    public LayerMask playerLayer; // Layer to detect player

    [Header("Knockback")]
    public float knockbackForce = 7f;
    public float knockbackDuration = 0.3f;

    private Vector3 originalPosition;
    private bool isPlayerDetected;
    private bool isDead = false;
    private bool isKnockedBack = false;
    private bool isAttacking = false;
    private float knockbackTimer = 0f;
    private float attackTimer = 0f;
    private float lastAttackTime = -999f;

    private Animator anim;
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    void Start()
    {
        originalPosition = transform.position;
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Set default player layer if not assigned
        if (playerLayer == 0)
        {
            playerLayer = LayerMask.GetMask("Player");
        }

        // Create attack point if it doesn't exist
        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPoint = attackPointObj.transform;
        }

        // Make enemy immovable by player collision
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
            rb.mass = 1000f; // Make enemy very heavy so player can't push
        }
    }

    void Update()
    {
        if (isDead) return;

        // Update attack point position based on facing direction
        UpdateAttackPointPosition();

        // Handle attack animation
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                isAttacking = false;
            }
            return; // Don't move during attack
        }

        // Handle knockback - MUST check this before any movement
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
            }
            return; // Exit early - don't do ANY movement during knockback
        }

        // Detect player first
        DetectPlayer();

        // Determine target and speed based on detection
        if (isPlayerDetected)
        {
            // Chase the player
            MoveToPosition(player.position, chaseSpeed);

            // Check if in attack range
            CheckAndAttack();
        }
        else if (returnToSpawn)
        {
            // Return to spawn point
            MoveToPosition(originalPosition, idleSpeed);
        }
        else
        {
            // Stand still if not returning to spawn
            if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }

        // Update animations
        UpdateAnimations();
    }

    void UpdateAttackPointPosition()
    {
        if (attackPoint == null) return;

        // Flip attack point based on sprite direction
        Vector2 flippedOffset = attackPointOffset;
        if (sr != null && sr.flipX)
        {
            flippedOffset.x = -attackPointOffset.x; // Flip X offset
        }

        attackPoint.localPosition = flippedOffset;
    }

    void DetectPlayer()
    {
        if (player == null) return;

        // Check if player is within detection radius
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isPlayerDetected = distanceToPlayer <= detectionRadius;
    }

    void CheckAndAttack()
    {
        if (player == null || isDead || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if player is in attack range and cooldown is ready
        if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            StartAttack();
        }
    }

    void StartAttack()
    {
        if (player == null || isDead) return;

        // Stop movement
        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // Start attack animation
        isAttacking = true;
        attackTimer = attackAnimationDuration;
        lastAttackTime = Time.time;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // REMOVED: Don't deal damage immediately - wait for animation event!
    }

    void DealDamage()
    {
        if (player == null || isDead || attackPoint == null) return;

        // Use Physics2D.OverlapCircle for precise hitbox detection
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackHitboxRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                DamagePlayerObject(hit.gameObject);
                Debug.Log($"{gameObject.name} hit player with hitbox!");
                return;
            }
        }

        Debug.Log($"{gameObject.name} attack missed - player not in hitbox!");
    }

    void DamagePlayerObject(GameObject playerObj)
    {
        if (playerObj == null) return;

        // Try new PlayerHealth system
        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageToPlayer, transform.position);
        }
        else
        {
            // Fallback to old system
            if (Game_Manager.Instance != null)
            {
                Game_Manager.Instance.MC_Hit(playerObj);
            }

            // Manual knockback
            Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockbackDirection = (playerObj.transform.position - transform.position).normalized;
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    // This MUST be called from Animation Event for precise timing
    public void OnAttackHit()
    {
        DealDamage();
    }

    void MoveToPosition(Vector3 target, float speed)
    {
        if (isDead) return;

        float dist = Vector2.Distance(transform.position, target);

        if (dist > stopDistance)
        {
            Vector2 direction = ((Vector2)target - (Vector2)transform.position).normalized;

            // Move towards target
            if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            {
                rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
            }

            // Flip sprite based on direction
            if (sr != null)
            {
                sr.flipX = direction.x < 0;
            }
        }
        else
        {
            // Stop when close enough
            if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        // Set running animation based on velocity (only if not attacking)
        if (!isAttacking)
        {
            bool isMoving = rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f;
            anim.SetBool("Run", isMoving);
        }
        else
        {
            // Make sure run animation is off during attack
            anim.SetBool("Run", false);
        }
    }

    // Implement IDamageable interface
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (anim != null)
            anim.SetTrigger("Hit");

        Debug.Log($"{gameObject.name} (Chase Enemy) took {damage} damage. Health: {health}");

        // Cancel attack if currently attacking
        if (isAttacking)
        {
            isAttacking = false;
            attackTimer = 0f;
        }

        ApplyKnockback();

        if (health <= 0)
        {
            Die();
        }
    }

    void ApplyKnockback()
    {
        if (player == null || isDead) return;

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        // Knockback away from player
        Vector2 knockbackDir = ((Vector2)transform.position - (Vector2)player.position).normalized;

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.linearVelocity = new Vector2(knockbackDir.x * knockbackForce, rb.linearVelocity.y);
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (anim != null)
            anim.SetTrigger("Death");

        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Disable collision
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        Destroy(gameObject, 1f);
    }

    // REMOVED: Collision damage - enemy only damages through attack animations now
    // Collision detection is kept for physics but doesn't trigger attacks

    void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = isPlayerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw stop distance
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Draw attack range (when enemy starts attack)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw attack hitbox if attack point is assigned
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackHitboxRadius);
        }
        else
        {
            // Preview where attack point would be
            Vector2 previewOffset = attackPointOffset;
            if (sr != null && sr.flipX)
            {
                previewOffset.x = -attackPointOffset.x;
            }
            Vector3 previewPos = transform.position + (Vector3)previewOffset;
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red
            Gizmos.DrawWireSphere(previewPos, attackHitboxRadius);
        }

        // Draw line to spawn point
        if (Application.isPlaying && returnToSpawn)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, originalPosition);
            Gizmos.DrawWireSphere(originalPosition, 0.3f);
        }
    }
}