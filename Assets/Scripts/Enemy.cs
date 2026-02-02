using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    public int health = 3;
    public Transform leftPoint;
    public Transform rightPoint;
    public float speed = 2f;

    private float direction = 1f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        leftPoint.parent = null;
        rightPoint.parent = null;
    }

    public void Die()
    {
        health--;
        anim.SetTrigger("Hit");
        Debug.Log("Health: " + health);

        if (health <= 0)
        {
            anim.SetTrigger("Death");
            Destroy(gameObject, 1f);
        }
    }

    private void Update()
    {
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        anim.SetTrigger("Walk");

        if (direction > 0 && transform.position.x >= rightPoint.position.x)
            Flip();

        if (direction < 0 && transform.position.x <= leftPoint.position.x)
            Flip();
    }

    private void Flip()
    {
        direction *= -1;
        sr.flipX = direction < 0; // ✅ no scale touched
    }
}
