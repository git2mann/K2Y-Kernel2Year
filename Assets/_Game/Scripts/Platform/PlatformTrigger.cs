using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public PlatformRebuilder platformRebuilder;
    public AudioClip triggerSound; // 🎵 Assign in Inspector

    private bool triggered = false;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            // Play sound if available
            if (audioSource && triggerSound)
                audioSource.PlayOneShot(triggerSound);

            platformRebuilder.StartBuilding();
        }
    }
}
