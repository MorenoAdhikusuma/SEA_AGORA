using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Death Settings")]
    [Tooltip("Set to 0 for instant respawn, or add delay if desired")]
    public float respawnDelay = 0f;

    [Header("Debug")]
    public bool showGizmo = true;
    public Color gizmoColor = Color.red;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered death zone - triggering instant death");

            // Try new PlayerHealth system first
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Deal massive damage to trigger death instantly
                playerHealth.TakeDamage(playerHealth.MaxHealth);
            }
            else
            {
                // Fallback to old Game_Manager system
                if (Game_Manager.Instance != null)
                {
                    Game_Manager.Instance.MC_Die(other.gameObject);
                }
            }
        }
    }

    // Visual helper in Scene view
    void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = gizmoColor;

        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        if (boxCol != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.offset, boxCol.size);
        }
    }
}