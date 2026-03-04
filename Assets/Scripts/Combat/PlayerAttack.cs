using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 1;
    public float attackRange = 1f;
    public float attackCooldown = 0.3f;
    public float attackDistance = 0.5f; // Distance from player center to attack point
    
    [Header("Combo System")]
    public bool enableCombo = true;
    public int maxComboCount = 3;
    public float comboResetTime = 1f;
    
    [Header("Attack Detection")]
    public Transform attackPoint; // Optional - can be null now
    public LayerMask enemyLayer;
    
    [Header("Movement Lock")]
    public bool lockMovementDuringAttack = true;
    public float movementLockDuration = 0.3f;
    
    [Header("Ground Attack Only")]
    public bool requireGrounded = true;
    
    [Header("References")]
    public MovementUpdate movementController;
    
    Animator animator;
    SpriteRenderer spriteRenderer;
    
    bool canAttack = true;
    int currentCombo = 0;
    float lastAttackTime;
    bool isAttacking;
    bool attackQueued;
    
    Collider2D[] hitResults = new Collider2D[10];
    
    [Header("Debug")]
    [SerializeField][ReadOnly] bool attacking;
    [SerializeField][ReadOnly] int combo;
    [SerializeField][ReadOnly] bool queued;
    [SerializeField][ReadOnly] float timeSinceLastAttack;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (movementController == null)
            movementController = GetComponent<MovementUpdate>();
    }
    
    void Update()
    {
        // Reset combo if too much time has passed since last attack
        if (Time.time - lastAttackTime > comboResetTime && !isAttacking)
        {
            if (currentCombo != 0)
            {
                currentCombo = 0;
                if (animator != null)
                {
                    animator.SetInteger("ComboCount", 0);
                    // Important: Reset the trigger to prevent phantom attacks
                    animator.ResetTrigger("Attack");
                }
                Debug.Log("Combo reset to 0 (timeout)");
            }
        }
        
        UpdateDebug();
    }
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Check if player is grounded (if required)
            if (requireGrounded && movementController != null && !movementController.IsGrounded)
            {
                Debug.Log("Cannot attack in air!");
                return;
            }
            
            if (isAttacking && enableCombo)
            {
                // Queue the next attack
                attackQueued = true;
                Debug.Log("Attack queued!");
            }
            else if (canAttack)
            {
                StartAttack();
            }
        }
    }
    
    void StartAttack()
    {
        if (!canAttack) return;
        
        // Double check grounded requirement
        if (requireGrounded && movementController != null && !movementController.IsGrounded)
        {
            return;
        }
        
        isAttacking = true;
        canAttack = false;
        lastAttackTime = Time.time;
        attackQueued = false;
        
        // Set combo count FIRST, then trigger attack
        if (animator != null)
        {
            animator.SetInteger("ComboCount", currentCombo);
            animator.SetTrigger("Attack");
            Debug.Log($"Playing attack animation with ComboCount: {currentCombo}");
        }
        
        // Lock movement
        if (lockMovementDuringAttack && movementController != null)
        {
            movementController.LockMovement(this);
        }
        
        // Play attack sound ONLY if grounded
        if (movementController != null && movementController.IsGrounded)
        {
            if (Audio_Manager.Instance != null)
            {
                Audio_Manager.Instance.PlaySFX(Audio_Manager.Instance.sword);
            }
        }
        
        Debug.Log($"Attack started - Combo: {currentCombo}");
        
        StartCoroutine(AttackCooldown());
    }
    
    // Called by Animation Event at the damage frame
    public void DealDamage()
    {
        // Calculate attack position based on facing direction
        Vector2 attackPosition = GetAttackPosition();
        
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            attackPosition,
            attackRange,
            hitResults,
            enemyLayer
        );
        
        Debug.Log($"DealDamage: Found {hitCount} enemies at position {attackPosition}");
        
        for (int i = 0; i < hitCount; i++)
        {
            IDamageable damageable = hitResults[i].GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"Hit {hitResults[i].name}!");
            }
        }
    }
    
    // Get attack position based on player's facing direction
    Vector2 GetAttackPosition()
    {
        // If attackPoint Transform is assigned, use it (for backward compatibility)
        if (attackPoint != null)
        {
            return attackPoint.position;
        }
        
        // Otherwise, calculate based on facing direction
        float direction = spriteRenderer.flipX ? -1f : 1f;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(direction * attackDistance, 0f);
        return attackPos;
    }
    
    // Called by Animation Event when attack animation ends
    public void OnAttackAnimationEnd()
    {
        Debug.Log($"Attack animation ended. Current Combo: {currentCombo}, Queued: {attackQueued}");
        
        isAttacking = false;
        
        // Unlock movement
        if (lockMovementDuringAttack && movementController != null)
        {
            movementController.UnlockMovement(this);
        }
        
        // IMPORTANT: Always reset the Attack trigger to prevent it from lingering
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
        }
        
        // Check if next attack was queued
        if (attackQueued && enableCombo && canAttack)
        {
            // Increment combo for next attack
            currentCombo++;
            
            // Wrap around if exceeds max
            if (currentCombo >= maxComboCount)
            {
                currentCombo = 0;
            }
            
            Debug.Log($"Continuing combo to: {currentCombo}");
            StartAttack();
        }
        else
        {
            // No queued attack - increment combo for next time
            currentCombo++;
            
            // If we've reached max combo, reset to 0
            if (currentCombo >= maxComboCount)
            {
                currentCombo = 0;
                if (animator != null)
                {
                    animator.SetInteger("ComboCount", 0);
                }
                Debug.Log("Combo chain completed, reset to 0");
            }
            else
            {
                Debug.Log($"No queued attack, combo at {currentCombo}. Will timeout in {comboResetTime}s");
            }
        }
    }
    
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    void UpdateDebug()
    {
        attacking = isAttacking;
        combo = currentCombo;
        queued = attackQueued;
        timeSinceLastAttack = Time.time - lastAttackTime;
    }
    
    void OnDrawGizmosSelected()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Draw attack range based on facing direction
        Vector2 attackPos = GetAttackPosition();
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);
        
        // Draw line showing attack direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, attackPos);
    }
}

// Make sure this interface is accessible
public interface IDamageable
{
    void TakeDamage(int damage);
}
