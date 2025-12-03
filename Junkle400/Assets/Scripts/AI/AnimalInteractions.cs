using System.Collections;
using UnityEngine;

public class AnimalInteractions : MonoBehaviour
{
    [Header("Player Interaction")]
    [SerializeField] private float freezePlayerDuration = 5f;
    [SerializeField] private LayerMask playerLayer = 1;
    
    [Header("Feeding & Herding")]
    [SerializeField] private float herdingDuration = 5f;
    [SerializeField] private float feedingRange = 2f;
    [SerializeField] private string feedItemTag = "Food";
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] interactionSounds;
    [SerializeField] private AudioClip[] feedingSounds;
    
    private SimpleAnimalAI animalAI;
    private bool isBeingHerded = false;
    
    private void Awake()
    {
        animalAI = GetComponent<SimpleAnimalAI>();
        
        // Get or create audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    // Called when player touches the animal
    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            FreezePlayer(other.gameObject);
            PlayInteractionSound();
        }
        else if (other.CompareTag(feedItemTag))
        {
            OnFed(other.gameObject);
        }
    }
    
    private bool IsPlayer(Collider other)
    {
        return ((1 << other.gameObject.layer) & playerLayer) != 0 || other.CompareTag("Player");
    }
    
    private void FreezePlayer(GameObject player)
    {
        Debug.Log($"{gameObject.name} froze the player for {freezePlayerDuration} seconds!");
        
        // Deduct team points when the player is frozen
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPoints(-50);
        }
        
        // Try to find common player controller components and disable them
        MonoBehaviour[] playerComponents = player.GetComponents<MonoBehaviour>();
        
        StartCoroutine(FreezePlayerCoroutine(playerComponents));
    }
    
    private IEnumerator FreezePlayerCoroutine(MonoBehaviour[] playerComponents)
    {
        // Disable player movement components
        foreach (var component in playerComponents)
        {
            // Disable common player controller types
            if (component.GetType().Name.ToLower().Contains("player") ||
                component.GetType().Name.ToLower().Contains("controller") ||
                component.GetType().Name.ToLower().Contains("movement") ||
                component.GetType().Name.ToLower().Contains("input"))
            {
                component.enabled = false;
            }
        }
        
        // You could also freeze rigidbody if the player uses physics
        Rigidbody playerRb = playerComponents[0].GetComponent<Rigidbody>();
        bool wasKinematic = false;
        if (playerRb != null)
        {
            wasKinematic = playerRb.isKinematic;
            playerRb.isKinematic = true;
        }
        
        // Wait for freeze duration
        yield return new WaitForSeconds(freezePlayerDuration);
        
        // Re-enable player components
        foreach (var component in playerComponents)
        {
            if (component.GetType().Name.ToLower().Contains("player") ||
                component.GetType().Name.ToLower().Contains("controller") ||
                component.GetType().Name.ToLower().Contains("movement") ||
                component.GetType().Name.ToLower().Contains("input"))
            {
                component.enabled = true;
            }
        }
        
        // Restore rigidbody
        if (playerRb != null)
        {
            playerRb.isKinematic = wasKinematic;
        }
        
        Debug.Log("Player is no longer frozen!");
    }
    
    private void OnFed(GameObject foodItem)
    {
        if (isBeingHerded) return; // Already being herded
        
        Debug.Log($"{gameObject.name} was fed and can now be herded!");
        
        // Destroy or disable the food item
        Destroy(foodItem);
        
        // Play feeding sound
        PlayFeedingSound();
        
        // Start herding behavior
        StartHerding();
    }
    
    private void StartHerding()
    {
        if (animalAI == null) return;
        
        isBeingHerded = true;
        
        // The animal will follow the player for the herding duration
        StartCoroutine(HerdingBehavior());
    }
    
    private IEnumerator HerdingBehavior()
    {
        GameObject player = FindPlayer();
        if (player == null) 
        {
            isBeingHerded = false;
            yield break;
        }
        
        // Calculate follow position behind player
        Vector3 playerPos = player.transform.position;
        Vector3 followPosition = playerPos - player.transform.forward * 3f; // Follow 3 units behind
        
        // Start herding behavior in the AI
        animalAI.StartHerding(followPosition, herdingDuration);
        
        // Wait for herding duration to complete
        yield return new WaitForSeconds(herdingDuration);
        
        isBeingHerded = false;
        Debug.Log($"{gameObject.name} stopped following the player.");
    }    private GameObject FindPlayer()
    {
        // Try to find player by tag first
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player;
        
        // If no tag, look for common player controller components
        MonoBehaviour[] allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var component in allComponents)
        {
            if (component.GetType().Name.ToLower().Contains("player"))
            {
                return component.gameObject;
            }
        }
        
        return null;
    }
    
    private void PlayInteractionSound()
    {
        if (audioSource != null && interactionSounds != null && interactionSounds.Length > 0)
        {
            AudioClip randomSound = interactionSounds[Random.Range(0, interactionSounds.Length)];
            audioSource.PlayOneShot(randomSound);
        }
    }
    
    private void PlayFeedingSound()
    {
        if (audioSource != null && feedingSounds != null && feedingSounds.Length > 0)
        {
            AudioClip randomSound = feedingSounds[Random.Range(0, feedingSounds.Length)];
            audioSource.PlayOneShot(randomSound);
        }
    }
    
    // Public method to feed the animal (can be called from other scripts)
    public void FeedAnimal()
    {
        if (!isBeingHerded)
        {
            StartHerding();
        }
    }
    
    // Public method to check if animal can be herded
    public bool CanBeHerded()
    {
        return !isBeingHerded;
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Show feeding range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, feedingRange);
        
        // Show herding status
        if (Application.isPlaying && isBeingHerded)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}