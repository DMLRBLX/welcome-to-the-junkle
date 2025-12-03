using UnityEngine;
public class HerdingController : MonoBehaviour
{

    [SerializeField] private string playerTag = "Player2";
    [SerializeField] private GameObject playerOverride;
    [SerializeField] private float herdingDuration = 3f;
    [SerializeField] private float followDistance = 3f;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            TriggerHerding();
        }
    }

    public void TriggerHerding()
    {
        GameObject player = FindPlayer();
        if (player == null)
        {
            Debug.LogWarning("HerdingController: no player found with tag '" + playerTag + "' to herd towards.");
            return;
        }

        Vector3 followPosition = player.transform.position - player.transform.forward * followDistance;

        SimpleAnimalAI[] animals = FindObjectsOfType<SimpleAnimalAI>();
        foreach (var animal in animals)
        {
            if (animal != null)
            {
                animal.StartHerding(followPosition, herdingDuration);
            }
        }

        Debug.Log($"HerdingController: triggered herding for {animals.Length} animals toward {player.name} for {herdingDuration} seconds.");
    }

    private GameObject FindPlayer()
    {
        if (playerOverride != null)
        {
            return playerOverride;
        }

        if (!string.IsNullOrEmpty(playerTag))
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) return p;
        }

        GameObject[] all = GameObject.FindGameObjectsWithTag("Player");
        if (all != null && all.Length > 0)
            return all[0];

        return null;
    }
}
