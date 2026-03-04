using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI; // Optional: if you have an options menu panel

    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape;
    public string mainMenuSceneName = "Main Menu"; // Name of your main menu scene

    private bool isPaused = false;

    void Start()
    {
        // Make sure pause menu is hidden at start
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (optionsMenuUI != null)
        {
            optionsMenuUI.SetActive(false);
        }

        // Ensure game is running
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        // Check for pause input
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // ========================
    // Button Functions
    // ========================

    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // Close options menu if it's open
        if (optionsMenuUI != null)
        {
            optionsMenuUI.SetActive(false);
        }

        Time.timeScale = 1f; // Resume game time
        isPaused = false;

        // Re-enable player input if needed
        EnablePlayerInput(true);

        Debug.Log("Game Resumed");
    }

    public void Pause()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Time.timeScale = 0f; // Freeze game time
        isPaused = true;

        // Disable player input while paused
        EnablePlayerInput(false);

        Debug.Log("Game Paused");
    }

    public void OpenOptions()
    {
        // Hide pause menu
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // Show options menu
        if (optionsMenuUI != null)
        {
            optionsMenuUI.SetActive(true);
        }

        Debug.Log("Options Menu Opened");
    }

    public void CloseOptions()
    {
        // Hide options menu
        if (optionsMenuUI != null)
        {
            optionsMenuUI.SetActive(false);
        }

        // Show pause menu again
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Debug.Log("Options Menu Closed");
    }

    public void ExitToMainMenu()
    {
        // Resume time before loading scene
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log($"Exiting to Main Menu: {mainMenuSceneName}");

        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ========================
    // Helper Functions
    // ========================

    void EnablePlayerInput(bool enable)
    {
        // Find and disable/enable player scripts
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Disable movement scripts
            MC_Movement movement = player.GetComponent<MC_Movement>();
            if (movement != null)
                movement.enabled = enable;

            MovementUpdate movementUpdate = player.GetComponent<MovementUpdate>();
            if (movementUpdate != null)
                movementUpdate.enabled = enable;

            DashAbility dash = player.GetComponent<DashAbility>();
            if (dash != null)
                dash.enabled = enable;

            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            if (attack != null)
                attack.enabled = enable;

            WallInteraction wallInteraction = player.GetComponent<WallInteraction>();
            if (wallInteraction != null)
                wallInteraction.enabled = enable;
        }
    }

    // Public getter for pause state
    public bool IsPaused => isPaused;

    // ========================
    // Cleanup
    // ========================

    void OnDestroy()
    {
        // Ensure time scale is reset when scene changes
        Time.timeScale = 1f;
    }
}