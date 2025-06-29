using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public PlatformRebuilder platformRebuilder;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            platformRebuilder.StartBuilding();
        }
    }
}
