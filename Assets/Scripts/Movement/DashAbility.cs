using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DashAbility : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;
    
    [Header("Momentum Settings")]
    [Tooltip("Speed player maintains after dash ends")]
    public float postDashSpeed = 8f;

    [Header("Visual Effects")]
    public TrailRenderer trailRenderer;

    [Header("References")]
    public MovementUpdate movementController;

    [Header("Debug")]
    [SerializeField][ReadOnly] bool dashing;
    [SerializeField][ReadOnly] float cooldownTimer;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;

    bool isDashing;
    bool canDash = true;
    float dashCooldownTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (movementController == null)
            movementController = GetComponent<MovementUpdate>();

        if (trailRenderer != null)
            trailRenderer.emitting = false;
    }

    void Update()
    {
        // Update cooldown timer
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
            }
        }

        UpdateDebug();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash && !isDashing)
        {
            StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        // Lock movement during dash
        if (movementController != null)
        {
            movementController.LockMovement(this);
        }

        // Store original gravity
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Determine dash direction (use sprite flip to determine facing)
        float dashDirection = spriteRenderer.flipX ? -1f : 1f;

        // Enable trail
        if (trailRenderer != null)
            trailRenderer.emitting = true;

        // Trigger dash animation if available
        if (animator != null)
            animator.SetBool("IsDashing", true);

        // Play dash sound if you add it to Audio_Manager
        // Audio_Manager.Instance.PlaySFX(Audio_Manager.Instance.dash);

        // Apply dash velocity continuously during dash
        float dashTimer = 0f;
        while (dashTimer < dashTime)
        {
            // Force velocity every frame to prevent override
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            
            dashTimer += Time.deltaTime;
            yield return null; // Wait for next frame
        }

        // Restore gravity
        rb.gravityScale = originalGravity;

        // Disable trail
        if (trailRenderer != null)
            trailRenderer.emitting = false;

        // Reset dash animation
        if (animator != null)
            animator.SetBool("IsDashing", false);

        // Set post-dash momentum so player doesn't stop
        rb.linearVelocity = new Vector2(dashDirection * postDashSpeed, rb.linearVelocity.y);

        isDashing = false;

        // Unlock movement AFTER setting velocity
        if (movementController != null)
        {
            movementController.UnlockMovement(this);
            
            // Set the current speed to match post-dash velocity for smooth transition
            movementController.SetCurrentSpeed(dashDirection * postDashSpeed);
        }
    }

    void UpdateDebug()
    {
        dashing = isDashing;
        cooldownTimer = dashCooldownTimer;
    }

    public bool IsDashing => isDashing;
}