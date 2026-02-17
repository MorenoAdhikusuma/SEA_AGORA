using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class Game_Manager : MonoBehaviour
{
    public static Game_Manager Instance;

    [Header("Player Data")]
    public int playerHealth = 3;

    [Header("Game Data")]
    public int coins = 0;

//SINGLETON
void Awake()
{
    Instance = this;
}

    // ========================
    // Coins
    // ========================
    public void AddCoin(int amount)
    {
        coins += amount;
        Debug.Log("Coins: " + coins);

        // TODO:
        // update UI
        // play sound
        // save data
    }

 
    // TODO: NTAR DIGANTI SAMA LOGIC YANG LEBIH PROPER
   public void MC_Die(GameObject player)
{
    Animator anim = player.GetComponent<Animator>();
    anim.SetBool("Death", true);
    Destroy(player.gameObject,0.5f);
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
public void MC_Hit(GameObject player)
    {
        playerHealth--;
            Debug.Log("Player hit! Health: " + playerHealth);
    
            if (playerHealth <= 0)
            {
                MC_Die(player);
            }
    }
}
