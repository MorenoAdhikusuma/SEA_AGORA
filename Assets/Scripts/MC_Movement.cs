using UnityEngine;

public class MC_Movement : MonoBehaviour
{
    // Initialize variables
    public float speed = 5f;
    public float jumpForce = 5f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;

    private bool isGrounded;
    private bool wasFalling;
    private bool facingRight = true; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float moveInput = Input.GetAxis("Horizontal");

        // =====================
        // MOVEMENT
        // =====================
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // =====================
        // FLIP LOGIC
        // =====================
        if (moveInput > 0 && !facingRight)
            Flip();
        else if (moveInput < 0 && facingRight)
            Flip();


        anim.SetBool("Run", moveInput != 0);
        anim.SetBool("Jump", !isGrounded);

        // =====================
        // JUMP INPUT
        // =====================
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            wasFalling = false;
        }

        // =====================
        // FALL DETECTION
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
            wasFalling = false;
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

    // DEBUG
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
