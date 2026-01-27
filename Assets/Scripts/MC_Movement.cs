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
            anim.SetTrigger("Jump_Trans");
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
    }
}
