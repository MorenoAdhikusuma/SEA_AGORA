using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int health = 3;

    [Header("Patrol Settings")]
    public Transform leftPoint;
    public Transform rightPoint;
    public float patrolSpeed = 2f;

    [Header("Combat")]
    public int damageToPlayer = 1;
    public float attackCooldown = 1f;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private float direction = 1f;
    private bool isDead = false;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private float lastAttackTime = -999f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // Detach patrol points from enemy so they don't move with it
        if (leftPoint != null)
            leftPoint.parent = null;
        if (rightPoint != null)
            rightPoint.parent = null;
    }

    void Update()
    {
        if (isDead) return;

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

        // Patrol movement - only when NOT knocked back
        Patrol();

        // Update animation
        if (anim != null)
        {
            anim.SetBool("Run", true);
        }
    }

    void Patrol()
    {
        if (leftPoint == null || rightPoint == null) return;

        // Move in current direction
        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);
        }

        // Check if we've reached patrol boundaries
        if (direction > 0 && transform.position.x >= rightPoint.position.x)
        {
            Flip();
        }
        else if (direction < 0 && transform.position.x <= leftPoint.position.x)
        {
            Flip();
        }
    }

    void Flip()
    {
        direction *= -1;
        if (sr != null)
        {
            sr.flipX = direction < 0;
        }
    }

    // Implement IDamageable interface
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (anim != null)
            anim.SetTrigger("Hit");

        Debug.Log($"{gameObject.name} (Patrol Enemy) took {damage} damage. Health: {health}");

        ApplyKnockback();

        if (health <= 0)
        {
            Die();
        }
    }

    void ApplyKnockback()
    {
        if (isDead) return;

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        // Knockback in opposite direction of patrol movement
        float knockbackDir = -direction;

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.linearVelocity = new Vector2(knockbackDir * knockbackForce, rb.linearVelocity.y);
        }
    }

    public void Die()
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

    // Damage player on collision
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            DamagePlayer(collision.gameObject);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                DamagePlayer(collision.gameObject);
            }
        }
    }

    void DamagePlayer(GameObject playerObj)
    {
        if (isDead) return;

        lastAttackTime = Time.time;

        // Try new PlayerHealth system
        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageToPlayer, transform.position);
            Debug.Log($"{gameObject.name} (Patrol Enemy) damaged player!");
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

    void OnDrawGizmosSelected()
    {
        if (leftPoint != null && rightPoint != null)
        {
            // Draw patrol path
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(leftPoint.position, rightPoint.position);

            // Draw patrol points
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(leftPoint.position, 0.3f);
            Gizmos.DrawWireSphere(rightPoint.position, 0.3f);
        }
    }
}