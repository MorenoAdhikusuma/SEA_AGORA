using UnityEngine;

public class PlayerCoinCollector : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we collided with a coin
        if (collision.CompareTag("Coins"))
        {
            // Add coin to Game Manager
            if (Game_Manager.Instance != null)
            {
                Game_Manager.Instance.AddCoin(1);
            }
            
            // Play collect sound
            if (Audio_Manager.Instance != null)
            {
                Audio_Manager.Instance.PlaySFX(Audio_Manager.Instance.collect);
            }
            
            // Destroy the coin
            Destroy(collision.gameObject);
            
            Debug.Log("Coin collected!");
        }
    }
}