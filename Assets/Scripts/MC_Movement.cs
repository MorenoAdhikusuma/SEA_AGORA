using UnityEngine;
using System.Collections;

public class MC_Movement : MonoBehaviour
{
    // =====================
    // CONFIG
    // =====================
    public float speed = 5f;
    public float jumpForce = 8f;
    public int maxJumpCount = 2;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    [Header("Dash")]
    public float dashSpeed = 5f;
    public float dashTime = 0.5f;
    public float dashingCooldown = 1f;

    public TrailRenderer tr; 

    [Header("Wall Jump")]
    public Transform wallCheck;
    public LayerMask wallLayer;

    public float wallSlidingSpeed = 2f;
    public float wallJumpingTime = 0.2f;
    public float wallJumpingDuration = 0.4f;
    public Vector2 wallJumpingPower = new Vector2(8f, 16f);

    private Rigidbody2D rb;
    private Animator anim;

    // =====================
    // STATE
    // =====================
    private bool isGrounded;
    private bool wasFalling;
    private bool facingRight = true;
    private int jumpCount;
    private int Coins = 0;

    private bool isDashing; 
    private bool canDash = true; 

    // WALL JUMP STATE
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingCounter;

    private float horizontal;
    private bool isFacingRight = true;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        horizontal = moveInput;

        if (!isDashing)
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);


        if(Input.GetKeyDown(KeyCode.E))
        {
            Attack();
        }
        
        // =====================
        // FLIP SPRITE
        // FIXED: prevent flip during wall jump
        // =====================
        if (!isWallJumping)
        {
            if (moveInput > 0 && !facingRight)
                Flip();
            else if (moveInput < 0 && facingRight)
                Flip();
        }

        anim.SetBool("Run", moveInput != 0);
        anim.SetBool("Jump", !isGrounded);

        // =====================
        // JUMP INPUT (DOUBLE JUMP)
        // =====================
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); 
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            jumpCount++;
            wasFalling = false;
        }

        // =====================
        // FALL TRANSITION
        // =====================
        if (!isGrounded && rb.linearVelocity.y < 0 && !wasFalling) 
        {
            anim.SetTrigger("jump_trans");
            wasFalling = true;
        }

        // =====================
        // DASH
        // =====================

        if (Input.GetKeyDown(KeyCode.LeftShift)  && canDash)
        {
            StartCoroutine(Dash());
        }

        // WALL
        WallSlide();
        WallJump();
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (isGrounded)
        {
            jumpCount = 0;
            wasFalling = false;
        }

        if (isDashing)
        {
            return;
        }
    }

   void Attack()
    {
        anim.SetTrigger("Attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.Die();
            }
        }

        Debug.Log("Attack triggered, hit " + hitEnemies.Length + " enemies");
    }

 
    private void Flip()
    {
        facingRight = !facingRight;
        isFacingRight = facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // COINS
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coins"))
        {
            Game_Manager.Instance.AddCoin(1);
            Destroy(collision.gameObject);
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float Gravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0f); 

        tr.emitting = true;

        yield return new WaitForSeconds(dashTime);

        tr.emitting = false;

        rb.gravityScale = Gravity;
        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);

        canDash = true;
    }


    // =====================
    // WALL JUMP
    // =====================

    private bool IsWalled()
    {
        bool isWalled = Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
        Debug.Log("IsWalled: " + isWalled); 
        return isWalled;
    }

    private bool IsGrounded()
    {
        return isGrounded;
    }


    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue)
            );
        }
        else
        {
            isWallSliding = false;
        }
    }


     private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;

            wallJumpingDirection = -transform.localScale.x;

            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && wallJumpingCounter > 0f)
        {
            isWallJumping = true;

            rb.linearVelocity = new Vector2(
                wallJumpingDirection * wallJumpingPower.x,
                wallJumpingPower.y
            );

            wallJumpingCounter = 0f;

            if ((wallJumpingDirection > 0 && transform.localScale.x < 0) || (wallJumpingDirection < 0 && transform.localScale.x > 0))
            {
                isFacingRight = !isFacingRight;

                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }


    private void StopWallJumping()
    {
        isWallJumping = false;
        facingRight = transform.localScale.x > 0;
    }


    // =====================
    // DEBUG
    // =====================
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(wallCheck.position, 0.2f);
        }
    } 

}
