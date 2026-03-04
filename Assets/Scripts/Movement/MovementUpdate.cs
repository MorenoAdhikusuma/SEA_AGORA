using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MovementUpdate : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 12f;
    public float deceleration = 15f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    [Header("Jump Protection")]
    public float jumpGroundIgnoreTime = 0.05f;

    [Header("Multi-Jump")]
    public int extraJumps = 1;
    public float airJumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    public bool CanMove => movementLocks.Count == 0;
    public float HorizontalSpeed => currentSpeed;
    public float InputX => moveInputX;
    public bool IsGrounded => isGrounded;

    Animator animator;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    
    float moveInputX;
    float currentSpeed;

    [Header("Debug")]
    [SerializeField][ReadOnly] bool canMove;
    [SerializeField][ReadOnly] float coyoteCounter;
    [SerializeField][ReadOnly] float jumpBufferCounter;
    [SerializeField][ReadOnly] float jumpIgnoreTimer;
    [SerializeField][ReadOnly] int jumpsRemaining;

    readonly HashSet<object> movementLocks = new();
    bool isGrounded;

    // Cache components
    DashAbility dashAbility;
    WallInteraction wallInteraction;

    public void LockMovement(object owner)
    {
        if (movementLocks.Add(owner))
        {
            moveInputX = 0;
            currentSpeed = 0;
        }
    }

    public void UnlockMovement(object owner)
    {
        movementLocks.Remove(owner);
    }
    
    public void SetCurrentSpeed(float speed)
    {
        currentSpeed = speed;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dashAbility = GetComponent<DashAbility>();
        wallInteraction = GetComponent<WallInteraction>();
    }

    void Update()
    {
        if (jumpIgnoreTimer > 0)
        {
            jumpIgnoreTimer -= Time.deltaTime;
            isGrounded = false;
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        }

        // Coyote time
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            jumpsRemaining = extraJumps;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        jumpBufferCounter -= Time.deltaTime;

        // Jump
        if (jumpBufferCounter > 0)
        {
            if (coyoteCounter > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger("Jump");
                coyoteCounter = 0;
                jumpBufferCounter = 0;
            }
            else if (jumpsRemaining > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger("Jump");
                jumpsRemaining--;
                jumpBufferCounter = 0;
            }
        }

        // Only flip sprite if NOT wall sliding or wall jumping
        bool canFlipSprite = true;
        if (wallInteraction != null)
        {
            canFlipSprite = !wallInteraction.IsWallSliding && !wallInteraction.IsWallJumping;
        }

        if (canFlipSprite && moveInputX != 0)
        {
            spriteRenderer.flipX = moveInputX < 0;
        }

        animator.SetFloat("Speed", Mathf.Abs(moveInputX));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
        animator.SetBool("CanJump", coyoteCounter > 0);

        canMove = CanMove;
    }

    void FixedUpdate()
    {
        // Don't override velocity if dashing OR wall jumping!
        if (dashAbility != null && dashAbility.IsDashing)
            return;
            
        if (wallInteraction != null && wallInteraction.IsWallJumping)
            return;

        float targetSpeed = moveInputX * moveSpeed;

        // Use acceleration for smoother movement
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInputX = CanMove ? context.ReadValue<float>() : 0;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!CanMove) return;

        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck is not assigned on " + gameObject.name);
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
