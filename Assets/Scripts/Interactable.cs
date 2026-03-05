using UnityEngine;
using TMPro;

public class Interactable : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas interactionCanvas;
    public GameObject promptUI; // "Press E to interact" UI element

    [Header("Content")]
    [TextArea(3, 10)]
    public string signContent = "Enter your sign text here...";
    public TextMeshProUGUI contentText; // Text component inside the canvas

    private bool playerInRange = false;
    private bool isInteracting = false;

    void Start()
    {
        // Make sure everything is hidden at start
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(false);
        }

        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        // Only allow interaction when player is in range and not already interacting
        if (playerInRange && Input.GetKeyDown(KeyCode.T) && !isInteracting)
        {
            OpenInteraction();
        }
        else if (isInteracting && Input.GetKeyDown(KeyCode.T))
        {
            CloseInteraction();
        }
    }

    void OpenInteraction()
    {
        isInteracting = true;

        // Hide prompt
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

        // Show canvas
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(true);

            // Set the text content
            if (contentText != null)
            {
                contentText.text = signContent;
            }
        }

        Debug.Log("Interaction opened");
    }

    void CloseInteraction()
    {
        isInteracting = false;

        // Hide canvas
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(false);
        }

        // Show prompt again if player still in range
        if (playerInRange && promptUI != null)
        {
            promptUI.SetActive(true);
        }

        Debug.Log("Interaction closed");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Show prompt if not already interacting
            if (!isInteracting && promptUI != null)
            {
                promptUI.SetActive(true);
            }

            Debug.Log("Player in range");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Hide prompt
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            // Close interaction if player walks away
            if (isInteracting)
            {
                CloseInteraction();
            }

            Debug.Log("Player left range");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visual helper - shows trigger area
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}