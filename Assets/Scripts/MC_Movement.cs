using UnityEngine;

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
    

    // =====================
    // COMPONENTS
    // =====================
    private Rigidbody2D rb;
    private Animator anim;

    // =====================
    // STATE
    // =====================
    private bool isGrounded;
    private bool wasFalling;
    private bool facingRight = true;
    private int jumpCount;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        // =====================
        // HORIZONTAL MOVEMENT
        // =====================
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

    // =====================
    // FLIP FUNCTION
    // =====================
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
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
