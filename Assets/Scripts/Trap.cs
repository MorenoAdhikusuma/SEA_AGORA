using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 1;
    public bool useKnockback = true;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Try new PlayerHealth system first
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (useKnockback)
                {
                    // Damage with knockback
                    playerHealth.TakeDamage(damage, transform.position);
                }
                else
                {
                    // Damage without knockback (just flash)
                    playerHealth.TakeDamage(damage);
                }
            }
            else
            {
                // Fallback to old Game_Manager system
                if (Game_Manager.Instance != null)
                {
                    Game_Manager.Instance.MC_Hit(other.gameObject);
                }
            }
        }
    }
}
