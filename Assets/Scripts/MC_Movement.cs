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

    public TrailRenderer tr; // FIXED: missing declaration

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (!isDashing)
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    

        if(Input.GetKeyDown(KeyCode.E))
        {
            Attack();
        }
        
        // =====================
        // FLIP SPRITE
        // =====================
        if (moveInput > 0 && !facingRight)
            Flip();
        else if (moveInput < 0 && facingRight)
            Flip();

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
    // DEBUG
    // =====================
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (attackPoint == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    } 

}
