using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyKill : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player died!");
            // Option 1: Disable player
            // collision.gameObject.SetActive(false);
            collision.GetComponent<GlitchDeathEffect>()?.GlitchOutAndDie();


            // Option 2: Restart scene (for testing)
        }
    }
}
