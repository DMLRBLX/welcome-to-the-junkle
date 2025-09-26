using UnityEngine;
using System.Collections;

namespace ithappy.Animals_FREE
{
   

    public class DistractionEffects : MonoBehaviour
    {
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem dustParticles;
        [SerializeField] private ParticleSystem excitementParticles;
        [SerializeField] private GameObject[] itemsToKnockOver;
        
        [Header("Screen Effects")]
        [SerializeField] private bool enableScreenShake = true;
        [SerializeField] private float shakeIntensity = 0.1f;
        [SerializeField] private float shakeDuration = 0.5f;
        
        [Header("UI Notifications")]
        [SerializeField] private bool showDistractionUI = true;
        [SerializeField] private string[] distractionMessages = {
            "A wild animal appeared!",
            "Something is causing a commotion!",
            "An animal is making a mess!",
            "You hear rustling nearby..."
        };
        
        [Header("Interaction")]
        [SerializeField] private bool canBeScared = true;
        [SerializeField] private float scareDistance = 2f;
        [SerializeField] private float scareForce = 5f;
        
        private AnimalAI animalAI;
        private Camera playerCamera;
        
        private void Start()
        {
            animalAI = GetComponent<AnimalAI>();
            playerCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        }
        
        // Not set up fully. Right click on component in inspector to test.
        [ContextMenu("Test Distraction")]
        public void TestDistraction()
        {
            TriggerDistraction();
        }
        
      
        public void TriggerDistraction()
        {
            StartCoroutine(DistractionSequence());
        }
        
        private IEnumerator DistractionSequence()
        {
            if (showDistractionUI)
            {
                ShowDistractionMessage();
            }
            
            if (dustParticles != null)
                dustParticles.Play();
                
            if (excitementParticles != null)
                excitementParticles.Play();
            
            KnockOverNearbyItems();
            
            if (enableScreenShake && playerCamera != null)
            {
                StartCoroutine(CameraShake());
            }
            
            yield return new WaitForSeconds(2f);
            
            if (dustParticles != null)
                dustParticles.Stop();
                
            if (excitementParticles != null)
                excitementParticles.Stop();
        }
        
        private void ShowDistractionMessage()
        {
            if (distractionMessages.Length > 0)
            {
                string message = distractionMessages[Random.Range(0, distractionMessages.Length)];
                Debug.Log(message); 
                
            }
        }
        
        private void KnockOverNearbyItems()
        {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 2f);
            
            foreach (var obj in nearbyObjects)
            {
                if (obj.CompareTag("Knockable") || obj.name.Contains("Prop"))
                {
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 forceDirection = (obj.transform.position - transform.position).normalized;
                        rb.AddForce(forceDirection * 3f, ForceMode.Impulse);
                    }
                }
            }
        }
        
        private IEnumerator CameraShake()
        {
            Vector3 originalPosition = playerCamera.transform.position;
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-1f, 1f) * shakeIntensity;
                float y = Random.Range(-1f, 1f) * shakeIntensity;
                
                playerCamera.transform.position = originalPosition + new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            playerCamera.transform.position = originalPosition;
        }
        
       
        public void ScareAway()
        {
            if (!canBeScared) return;
            
            Vector3 awayDirection = (transform.position - playerCamera.transform.position).normalized;
            Vector3 scareTarget = transform.position + awayDirection * scareForce;
            
            Debug.Log($"{gameObject.name} got scared and ran away!");
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                float distance = Vector3.Distance(transform.position, other.transform.position);
                if (distance < scareDistance)
                {
                    ScareAway();
                }
            }
        }
        
       
    }
}