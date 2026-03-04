using UnityEngine;
using UnityEngine.InputSystem;

public class WallInteraction : MonoBehaviour
{
    [Header("Wall Detection")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;

    [Header("Wall Slide")]
    public float wallSlidingSpeed = 2f;

    [Header("Wall Jump")]
    public Vector2 wallJumpPower = new Vector2(12f, 16f);
    public float wallJumpDuration = 0.4f;
    public float wallJumpTime = 0.2f;
    
    [Header("Wall Jump Animation")]
    public float wallJumpAnimationTime = 0.3f; // How long wall jump animation plays
    
    [Header("Wall Jump Control")]
    [Tooltip("Time before player can influence direction after wall jump")]
    public float wallJumpControlDelay = 0.15f;
    
    [Tooltip("How much air control player has after wall jump (0 = none, 1 = full Celeste-style control)")]
    [Range(0f, 1f)]
    public float wallJumpAirControl = 0.6f;

    [Header("References")]
    public MovementUpdate movementController;

    [Header("Debug")]
    [SerializeField][ReadOnly] bool wallSliding;
    [SerializeField][ReadOnly] bool wallJumping;
    [SerializeField][ReadOnly] float wallJumpCounter;
    [SerializeField][ReadOnly] float wallJumpAnimTimer;
    [SerializeField][ReadOnly] float controlTimer;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;

    bool isWallSliding;
    bool isWallJumping;
    float wallJumpingCounter;
    float wallJumpingDirection;
    bool jumpInputQueued;
    float wallJumpTimer;
    float wallJumpAnimationTimer;
    float wallJumpControlTimer;
    
    // Track which side of the wall we're on
    bool isOnRightWall; // true = wall is on right side, false = wall is on left side

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (movementController == null)
            movementController = GetComponent<MovementUpdate>();
    }

    void Update()
    {
        CheckWallSlide();
        UpdateWallJump();
        UpdateSpriteDirection();
        UpdateAnimations();
        UpdateDebug();
        
        // Count down wall jump timer
        if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.deltaTime;
        }
        
        // Count down control timer
        if (wallJumpControlTimer > 0)
        {
            wallJumpControlTimer -= Time.deltaTime;
        }
        
        // Count down wall jump animation timer
        if (wallJumpAnimationTimer > 0)
        {
            wallJumpAnimationTimer -= Time.deltaTime;
            
            // When animation timer ends, transition to fall
            if (wallJumpAnimationTimer <= 0 && animator != null)
            {
                animator.SetBool("IsWallJumping", false);
            }
        }
    }

    void FixedUpdate()
    {
        ApplyWallSlide();
        ApplyWallJumpMovement();
    }

    void CheckWallSlide()
    {
        bool wasWallSliding = isWallSliding;
        
        // Check both sides for walls
        bool rightWall = IsWalled(true);
        bool leftWall = IsWalled(false);
        
        bool isGrounded = movementController != null && movementController.IsGrounded;

        // Determine which wall we're on
        if (rightWall && !leftWall)
        {
            isOnRightWall = true;
        }
        else if (leftWall && !rightWall)
        {
            isOnRightWall = false;
        }

        // Only wall slide if: touching wall, not grounded, and not wall jumping
        if ((rightWall || leftWall) && !isGrounded && !isWallJumping)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    void ApplyWallSlide()
    {
        if (isWallSliding)
        {
            // Clamp downward velocity for wall slide effect
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue)
            );
        }
    }
    
    void ApplyWallJumpMovement()
    {
        // Apply Celeste-style air control during wall jump
        if (isWallJumping && wallJumpTimer > 0 && movementController != null)
        {
            // During control delay period, FORCE the wall jump direction
            if (wallJumpControlTimer > 0)
            {
                // Maintain the wall jump velocity during delay (no player control)
                float maintainSpeed = wallJumpingDirection * wallJumpPower.x;
                rb.linearVelocity = new Vector2(maintainSpeed, rb.linearVelocity.y);
            }
            else
            {
                // After delay, allow player to influence movement
                float inputX = movementController.InputX;
                
                if (Mathf.Abs(inputX) > 0.01f)
                {
                    float targetSpeed = inputX * movementController.moveSpeed;
                    float currentXVelocity = rb.linearVelocity.x;
                    
                    // Blend between current velocity and player input
                    float newXVelocity = Mathf.Lerp(
                        currentXVelocity, 
                        targetSpeed, 
                        wallJumpAirControl * Time.fixedDeltaTime * 8f
                    );
                    
                    rb.linearVelocity = new Vector2(newXVelocity, rb.linearVelocity.y);
                }
            }
        }
    }

    void UpdateWallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;

            // Determine jump direction (opposite of wall side)
            wallJumpingDirection = isOnRightWall ? -1f : 1f;
            wallJumpingCounter = wallJumpTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        // Check if jump input was queued while on wall
        if (jumpInputQueued && wallJumpingCounter > 0f)
        {
            PerformWallJump();
            jumpInputQueued = false;
        }
    }

    void PerformWallJump()
    {
        isWallJumping = true;
        wallJumpTimer = wallJumpDuration;
        wallJumpControlTimer = wallJumpControlDelay;
        wallJumpAnimationTimer = wallJumpAnimationTime;

        // Apply wall jump force
        rb.linearVelocity = new Vector2(
            wallJumpingDirection * wallJumpPower.x,
            wallJumpPower.y
        );

        wallJumpingCounter = 0f;

        // Trigger wall jump animation
        if (animator != null)
        {
            animator.SetBool("IsWallJumping", true);
            animator.SetTrigger("WallJump");
        }

        // Play jump sound
        if (Audio_Manager.Instance != null)
        {
            Audio_Manager.Instance.PlaySFX(Audio_Manager.Instance.jump);
        }

        // Stop wall jumping after duration
        Invoke(nameof(StopWallJumping), wallJumpDuration);
    }

    void StopWallJumping()
    {
        isWallJumping = false;
        wallJumpTimer = 0f;
        wallJumpControlTimer = 0f;
        
        // Make sure animation bool is off
        if (animator != null)
        {
            animator.SetBool("IsWallJumping", false);
        }
    }
    
    // Handle sprite direction for wall interactions
    void UpdateSpriteDirection()
    {
        if (isWallSliding)
        {
            // Face TOWARDS the wall
            // If wall is on right, face right (flipX = false)
            // If wall is on left, face left (flipX = true)
            spriteRenderer.flipX = isOnRightWall;
        }
        else if (isWallJumping)
        {
            // Face the direction we're jumping
            spriteRenderer.flipX = wallJumpingDirection < 0;
        }
        // Otherwise, let MovementUpdate handle sprite direction
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isWallSliding)
        {
            jumpInputQueued = true;
        }
    }

    bool IsWalled(bool checkRight)
    {
        // Calculate wall check position based on side
        float direction = checkRight ? 1f : -1f;
        Vector2 wallCheckPos = (Vector2)transform.position + new Vector2(direction * wallCheckDistance, 0f);
        
        // Check for wall using circle cast
        return Physics2D.OverlapCircle(wallCheckPos, wallCheckRadius, wallLayer);
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Wall Climb (sliding down wall)
            animator.SetBool("IsWallSliding", isWallSliding);
        }
    }

    void UpdateDebug()
    {
        wallSliding = isWallSliding;
        wallJumping = isWallJumping;
        wallJumpCounter = wallJumpingCounter;
        wallJumpAnimTimer = wallJumpAnimationTimer;
        controlTimer = wallJumpControlTimer;
    }

    void OnDrawGizmosSelected()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Draw RIGHT wall check (Green)
        Vector2 rightWallCheckPos = (Vector2)transform.position + new Vector2(wallCheckDistance, 0f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(rightWallCheckPos, wallCheckRadius);
        Gizmos.DrawLine(transform.position, rightWallCheckPos);
        
        // Draw LEFT wall check (Blue)
        Vector2 leftWallCheckPos = (Vector2)transform.position + new Vector2(-wallCheckDistance, 0f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(leftWallCheckPos, wallCheckRadius);
        Gizmos.DrawLine(transform.position, leftWallCheckPos);
        
        // Draw label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(rightWallCheckPos + Vector2.up * 0.5f, "Right Wall");
        UnityEditor.Handles.Label(leftWallCheckPos + Vector2.up * 0.5f, "Left Wall");
        #endif
    }

    public bool IsWallSliding => isWallSliding;
    public bool IsWallJumping => isWallJumping;
}