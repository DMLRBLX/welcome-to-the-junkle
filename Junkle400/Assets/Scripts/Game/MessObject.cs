using UnityEngine;

public class MessObject : MonoBehaviour
{
    [Header("Cleanup Settings")]
    [SerializeField] private float cleanupTime = 2f; // Time to clean up mess
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRange = 2f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject cleanupEffect; // Optional particle effect
    [SerializeField] private AudioClip cleanupSound;
    [SerializeField] private Canvas interactPrompt; // Optional UI prompt
    
    private bool isBeingCleaned = false;
    private bool playerInRange = false;
    private GameObject currentPlayer;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && cleanupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Hide interact prompt initially
        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Check for player input if player is in range
        if (playerInRange && !isBeingCleaned && Input.GetKeyDown(interactKey))
        {
            StartCleanup();
        }
        
        // Check if player is still in range
        if (currentPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, currentPlayer.transform.position);
            if (distance > interactRange)
            {
                OnPlayerLeft();
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            OnPlayerEntered(other.gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && other.gameObject == currentPlayer)
        {
            OnPlayerLeft();
        }
    }
    
    private void OnPlayerEntered(GameObject player)
    {
        if (isBeingCleaned) return;
        
        playerInRange = true;
        currentPlayer = player;
        
        // Show interact prompt
        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(true);
        }
        
        Debug.Log($"Press {interactKey} to clean up {gameObject.name}");
    }
    
    private void OnPlayerLeft()
    {
        playerInRange = false;
        currentPlayer = null;
        
        // Hide interact prompt
        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(false);
        }
    }
    
    private void StartCleanup()
    {
        if (isBeingCleaned) return;
        
        isBeingCleaned = true;
        
        Debug.Log($"Cleaning up {gameObject.name}...");
        
        // Hide interact prompt during cleanup
        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(false);
        }
        
        // Start cleanup process
        Invoke(nameof(FinishCleanup), cleanupTime);
        
        // Play cleanup sound
        if (audioSource != null && cleanupSound != null)
        {
            audioSource.PlayOneShot(cleanupSound);
        }
    }
    
    private void FinishCleanup()
    {
        Debug.Log($"{gameObject.name} has been cleaned up!");
        
        // Spawn cleanup effect
        if (cleanupEffect != null)
        {
            Instantiate(cleanupEffect, transform.position, transform.rotation);
        }
        
        // Award points or trigger cleanup event
        OnMessCleaned();
        
        // Destroy the mess object
        Destroy(gameObject);
    }
    
    private void OnMessCleaned()
    {
        // You can expand this to:
        // - Award points to player
        // - Update game manager
        // - Trigger achievements
        // - Play additional effects
        
        // Example: Find game manager and notify
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
        {
            // gameManager.SendMessage("OnMessCleaned", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    // Public method for other scripts to clean this mess (e.g., vacuum cleaner)
    public void CleanInstantly()
    {
        if (isBeingCleaned) return;
        
        OnMessCleaned();
        Destroy(gameObject);
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Show interact range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}