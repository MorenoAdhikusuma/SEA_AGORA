using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Damage Feedback")]
    public float invincibilityDuration = 1f;
    public float flashDuration = 0.1f;
    public int flashCount = 5;
    public Color damageColor = Color.red;

    [Header("Knockback")]
    public float knockbackForce = 10f;
    public float knockbackUpwardForce = 5f;

    [Header("Respawn")]
    public Transform spawnPoint;
    public float respawnDelay = 1f;

    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector3 initialSpawnPosition;

    [Header("Debug")]
    [SerializeField][ReadOnly] int health;
    [SerializeField][ReadOnly] bool invincible;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Store original sprite color
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Store initial spawn position
        initialSpawnPosition = transform.position;
    }

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Sync with Game_Manager
        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.maxPlayerHealth = maxHealth;
            Game_Manager.Instance.ResetHealth();
        }

        Debug.Log($"PlayerHealth: Initialized with {currentHealth} health");
    }

    // Implement IDamageable interface
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;

        // Update Game_Manager health (triggers health bar update)
        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.SetHealth(currentHealth);
        }

        Debug.Log($"Player took {damage} damage! Health: {currentHealth}/{maxHealth}");

        // Trigger damage feedback
        StartCoroutine(DamageFlash());
        StartCoroutine(InvincibilityFrames());

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Overload for damage with knockback direction
    public void TakeDamage(int damage, Vector2 damageSource)
    {
        if (isInvincible) return;

        TakeDamage(damage);
        ApplyKnockback(damageSource);
    }

    void ApplyKnockback(Vector2 damageSource)
    {
        if (rb == null) return;

        // Calculate knockback direction (away from damage source)
        Vector2 knockbackDirection = ((Vector2)transform.position - damageSource).normalized;

        // Apply knockback force with upward component
        Vector2 knockbackVelocity = new Vector2(
            knockbackDirection.x * knockbackForce,
            knockbackUpwardForce
        );

        // Reset velocity first, then apply knockback
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackVelocity, ForceMode2D.Impulse);

        Debug.Log($"Knockback applied: {knockbackVelocity}");
    }

    IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        // Flash red multiple times
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashDuration);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        // Ensure we end with original color
        spriteRenderer.color = originalColor;
    }

    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Player died!");

        // Start respawn sequence
        StartCoroutine(RespawnSequence());
    }

    IEnumerator RespawnSequence()
    {
        // Trigger death animation if available
        if (animator != null)
        {
            animator.SetBool("Death", true);
        }

        // Make player invincible during death
        isInvincible = true;

        // Stop player movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);

        // Respawn player
        Respawn();
    }

    void Respawn()
    {
        // Reset health to max
        currentHealth = maxHealth;

        // Update Game_Manager (triggers health bar update)
        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.ResetHealth();
        }

        // Move to spawn point
        Vector3 respawnPosition = spawnPoint != null ? spawnPoint.position : initialSpawnPosition;
        transform.position = respawnPosition;

        // Reset velocity
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Reset animations
        if (animator != null)
        {
            animator.SetBool("Death", false);
            animator.Play("Idle"); // Reset to idle state
        }

        // Reset sprite color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // Give brief invincibility after respawn
        StartCoroutine(RespawnInvincibility());

        Debug.Log($"Player respawned at {respawnPosition} with {currentHealth}/{maxHealth} health");
    }

    IEnumerator RespawnInvincibility()
    {
        isInvincible = true;

        // Flash briefly to show invincibility
        if (spriteRenderer != null)
        {
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return new WaitForSeconds(1f);

        isInvincible = false;
    }

    // Public method to heal player
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // Update Game_Manager (triggers health bar update)
        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.SetHealth(currentHealth);
        }

        Debug.Log($"Player healed {amount}! Health: {currentHealth}/{maxHealth}");
    }

    // Public method to set spawn point dynamically
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
        Debug.Log($"Spawn point updated to {newSpawnPoint.position}");
    }

    void UpdateDebug()
    {
        health = currentHealth;
        invincible = isInvincible;
    }

    void Update()
    {
        UpdateDebug();
    }

    // Public getters
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;
    public bool IsDead => currentHealth <= 0;
}