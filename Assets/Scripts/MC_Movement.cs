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

    private bool isDashing; 
    private bool canDash = true; 

    private bool isWallSliding;
    private bool isWallJumping;

    private float wallJumpingDirection;
    private float wallJumpingCounter;

    private float horizontal;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }


    void Update()
    {

        horizontal = Input.GetAxisRaw("Horizontal");


        // =====================
        // MOVEMENT
        // =====================

        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }


        // =====================
        // WALK AUDIO FIX
        // =====================

        bool isWalking = isGrounded && Mathf.Abs(horizontal) > 0.1f;

        if (isWalking)
        {
            Audio_Manager.Instance.PlayLoop(Audio_Manager.Instance.walk);
        }
        else
        {
            Audio_Manager.Instance.StopLoop();
        }



        // =====================
        // FLIP
        // =====================

        if (!isWallJumping)
        {
            if (horizontal > 0 && !facingRight)
                Flip();
            else if (horizontal < 0 && facingRight)
                Flip();
        }


        // =====================
        // ANIMATION
        // =====================

        anim.SetBool("Run", horizontal != 0);
        anim.SetBool("Jump", !isGrounded);



        // =====================
        // JUMP
        // =====================

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
        {

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            jumpCount++;

            Audio_Manager.Instance.PlaySFX(Audio_Manager.Instance.jump);

            wasFalling = false;
        }



        // =====================
        // ATTACK
        // =====================

        if(Input.GetKeyDown(KeyCode.E))
        {

            Attack();

            Audio_Manager.Instance.PlaySFX(Audio_Manager.Instance.sword);

        }



        // =====================
        // DASH
        // =====================

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {

            //Audio_Manager.Instance.PlaySFX(Audio_Manager.Instance.dash);

            StartCoroutine(Dash());

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
        // WALL SYSTEM
        // =====================

        WallSlide();
        WallJump();

        anim.SetBool("wall_climb", isWallSliding && rb.linearVelocity.y < 0);

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
            return;

    }



    // =====================
    // ATTACK LOGIC
    // =====================

    void Attack()
    {

        anim.SetTrigger("Attack");

        Collider2D[] hitEnemies =
        Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );


        foreach (Collider2D enemy in hitEnemies)
        {

            Enemy enemyScript =
            enemy.GetComponent<Enemy>();


            if (enemyScript != null)
            {
                enemyScript.Die();
            }

        }

    }



    // =====================
    // COIN
    // =====================

    void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Coins"))
        {

            Game_Manager.Instance.AddCoin(1);

            Audio_Manager.Instance.PlaySFX(Audio_Manager.Instance.collect);

            Destroy(collision.gameObject);

        }

    }



    // =====================
    // DASH
    // =====================

    private IEnumerator Dash()
    {

        canDash = false;

        isDashing = true;

        float Gravity = rb.gravityScale;

        rb.gravityScale = 0f;

        rb.linearVelocity =
        new Vector2(transform.localScale.x * dashSpeed, 0f);

        tr.emitting = true;

        yield return new WaitForSeconds(dashTime);

        tr.emitting = false;

        rb.gravityScale = Gravity;

        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);

        canDash = true;

    }



    // =====================
    // WALL
    // =====================

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }



    private void WallSlide()
    {

        if (IsWalled() && !isGrounded)
        {

            isWallSliding = true;

            rb.linearVelocity =
            new Vector2(
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

            rb.linearVelocity =
            new Vector2(
                wallJumpingDirection * wallJumpingPower.x,
                wallJumpingPower.y
            );


            wallJumpingCounter = 0f;

            Invoke(nameof(StopWallJumping), wallJumpingDuration);

        }

    }



    private void StopWallJumping()
    {

        isWallJumping = false;

        facingRight = transform.localScale.x > 0;

    }



    // =====================
    // FLIP
    // =====================

    private void Flip()
    {

        facingRight = !facingRight;

        Vector3 scale = transform.localScale;

        scale.x *= -1;

        transform.localScale = scale;

    }

}