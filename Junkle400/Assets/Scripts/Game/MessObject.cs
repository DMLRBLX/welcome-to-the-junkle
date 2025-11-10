using UnityEngine;
using UnityEngine.InputSystem;

public class MessObject : MonoBehaviour
{
    [Header("Cleanup Settings")]
    [SerializeField] private float cleanupTime = 2f; // Time to clean up mess
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode interactKey = KeyCode.E; 
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private InputActionReference interactAction; 
    private InputAction instantiatedInteractAction;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject cleanupEffectPrefab;
    [SerializeField] private AudioClip cleanupSound;
    [Header("Scoring")]
    [SerializeField] private int pointsValue = 10; // how many team points this mess gives when cleaned
    [SerializeField] private Canvas interactPrompt; // Optional UI prompt
    
    private bool isBeingCleaned = false;
    private bool playerInRange = false;
    private GameObject currentPlayer;
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && cleanupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(false);
        }
    }

    // private void OnEnable()
    // {
    //     if (interactAction != null && interactAction.action != null)
    //     {
    //         instantiatedInteractAction = interactAction.action;
    //         instantiatedInteractAction.started += OnInteractStarted;
    //         instantiatedInteractAction.Enable();
    //     }
    // }

    // private void OnDisable()
    // {
    //     if (instantiatedInteractAction != null)
    //     {
    //         instantiatedInteractAction.started -= OnInteractStarted;
    //         instantiatedInteractAction.Disable();
    //         instantiatedInteractAction = null;
    //     }
    // }

    // private void OnInteractStarted(InputAction.CallbackContext ctx)
    // {
    //     HandleCleanup();
    // }
    
    // public void HandleCleanup()
    // {
    //     if (!playerInRange && currentPlayer == null)
    //     {
    //         TryFindPlayerNearby();
    //     }

    //     if (playerInRange && !isBeingCleaned)
    //     {
    //         StartCleanup();
    //     }
    // }
    
    // private void Update()
    // {
    //     if (interactAction == null || interactAction.action == null)
    //     {
    //         return;
    //     }

    //     if (currentPlayer != null)
    //     {
    //         float distance = Vector3.Distance(transform.position, currentPlayer.transform.position);
    //         if (distance > interactRange)
    //         {
    //             OnPlayerLeft();
    //         }
    //     }
    // }
    
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag(playerTag))
    //     {
    //         OnPlayerEntered(other.gameObject);
    //     }
    // }
    
    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag(playerTag) && other.gameObject == currentPlayer)
    //     {
    //         OnPlayerLeft();
    //     }
    // }
    
    // private void OnPlayerEntered(GameObject player)
    // {
    //     if (isBeingCleaned) return;
        
    //     playerInRange = true;
    //     currentPlayer = player;
        
    //     // Show interact prompt
    //     if (interactPrompt != null)
    //     {
    //         interactPrompt.gameObject.SetActive(true);
    //     }
        
    //     Debug.Log($"Press {interactKey} to clean up {gameObject.name}");
    // }
    
    // private void OnPlayerLeft()
    // {
    //     playerInRange = false;
    //     currentPlayer = null;
        
    //     // Hide interact prompt
    //     if (interactPrompt != null)
    //     {
    //         interactPrompt.gameObject.SetActive(false);
    //     }
    // }
    
    public void StartCleanup()
    {
        if (isBeingCleaned) return;
        
        isBeingCleaned = true;
        
        Debug.Log($"Cleaning up {gameObject.name}...");
        
        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(false);
        }
        
        if (cleanupEffectPrefab != null)
        {
            GameObject fx = Instantiate(cleanupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, Mathf.Max(2f, cleanupTime)); // keep effect at least 2s or until cleanupTime
        }

        Invoke(nameof(FinishCleanup), cleanupTime);

        if (audioSource != null && cleanupSound != null)
        {
            audioSource.PlayOneShot(cleanupSound);
        }
    }

    // private void TryFindPlayerNearby()
    // {
    //     Collider[] cols = Physics.OverlapSphere(transform.position, interactRange);
    //     foreach (var c in cols)
    //     {
    //         if (c.CompareTag(playerTag))
    //         {
    //             OnPlayerEntered(c.gameObject);
    //             return;
    //         }
    //     }
    // }
    
    private void FinishCleanup()
    {
        Debug.Log($"{gameObject.name} has been cleaned up!");
        
       
    
        OnMessCleaned();
        
        Destroy(gameObject);
    }
    
    private void OnMessCleaned()
    {
      
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPoints(pointsValue);
        }
    }
    
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