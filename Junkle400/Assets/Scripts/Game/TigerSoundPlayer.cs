using UnityEngine;

public class TigerSoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] soundClips;
    public float minTime = 3f;
    public float maxTime = 7f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        StartCoroutine(PlayRandomSound());
    }

    System.Collections.IEnumerator PlayRandomSound()
    {
        while (true)
        {
            // Wait a random number of seconds
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));

            // Pick a random sound
            AudioClip clip = soundClips[Random.Range(0, soundClips.Length)];

            audioSource.PlayOneShot(clip);
        }
    }
}
