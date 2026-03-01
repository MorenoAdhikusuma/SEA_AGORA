using UnityEngine;

public class Flying_Enemy : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sr;

    public int health = 3;

    public float projectile_speed = 2f;

    public GameObject projectilePrefab;

    private Transform player; // ADD THIS

    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        player = GameObject.FindGameObjectWithTag("Player").transform; // FIND PLAYER

        InvokeRepeating(nameof(ShootAtPlayer), 1f, 2f);
    }

    void Update()
    {

    }

    // =====================
    // SHOOT AT PLAYER
    // =====================

    public void ShootAtPlayer()
    {
        if (player == null) return;

        Debug.Log("Shoot at Player!");

        // calculate direction
        Vector2 direction = (player.position - transform.position).normalized;

        // create bullet
        GameObject bullet = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = direction * projectile_speed;
    }


    // =====================
    // DIE
    // =====================

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
}