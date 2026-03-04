using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Game_Manager : MonoBehaviour
{
    public static Game_Manager Instance;

    [Header("Player Data")]
    public int maxPlayerHealth = 3;
    public int playerHealth = 3;

    [Header("UI Elements")]
    public Slider healthBarSlider;
    public TextMeshProUGUI healthText; // Optional: shows "3/3"

    [Header("Game Data")]
    public int coins = 0;
    public TextMeshProUGUI coinText;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize health bar
        InitializeHealthBar();
    }

    void Start()
    {
        UpdateHealthBar();
        UpdateCoinUI();
    }

    // ========================
    // Health Bar System
    // ========================

    void InitializeHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxPlayerHealth;
            healthBarSlider.value = playerHealth;
            healthBarSlider.wholeNumbers = true; // Ensure integer values
        }
    }

    public void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = playerHealth;
        }

        // Update health text if assigned
        if (healthText != null)
        {
            healthText.text = $"{playerHealth}/{maxPlayerHealth}";
        }

        Debug.Log($"Health Bar Updated: {playerHealth}/{maxPlayerHealth}");
    }

    public void SetHealth(int newHealth)
    {
        playerHealth = Mathf.Clamp(newHealth, 0, maxPlayerHealth);
        UpdateHealthBar();
    }

    public void DamagePlayer(int damage)
    {
        playerHealth = Mathf.Max(0, playerHealth - damage);
        UpdateHealthBar();

        if (playerHealth <= 0)
        {
            // Player death handled by PlayerHealth script
            Debug.Log("Player health depleted!");
        }
    }

    public void HealPlayer(int amount)
    {
        playerHealth = Mathf.Min(maxPlayerHealth, playerHealth + amount);
        UpdateHealthBar();
    }

    public void ResetHealth()
    {
        playerHealth = maxPlayerHealth;
        UpdateHealthBar();
    }

    // ========================
    // Coins
    // ========================

    public void AddCoin(int amount)
    {
        coins += amount;
        UpdateCoinUI();
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = coins.ToString();
        }
    }

    // ========================
    // Player (Legacy - mostly handled by PlayerHealth now)
    // ========================

    public void MC_Die(GameObject player)
    {
        Animator anim = player.GetComponent<Animator>();

        if (anim != null)
        {
            anim.SetBool("Death", true);
        }

        // Reset health on death
        ResetHealth();

        Destroy(player.gameObject, 0.5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MC_Hit(GameObject player)
    {
        DamagePlayer(1);

        Debug.Log("Player hit! Health: " + playerHealth);

        if (playerHealth <= 0)
        {
            MC_Die(player);
        }
    }

    // ========================
    // Scene Management
    // ========================

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset health when scene loads (respawn)
        ResetHealth();

        // Re-find UI elements if they were lost
        if (healthBarSlider == null)
        {
            healthBarSlider = GameObject.Find("HealthBar")?.GetComponent<Slider>();
        }
        if (coinText == null)
        {
            coinText = GameObject.Find("CoinText")?.GetComponent<TextMeshProUGUI>();
        }

        InitializeHealthBar();
        UpdateCoinUI();
    }
}